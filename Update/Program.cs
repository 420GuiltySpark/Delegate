using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace update
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0 && args.Length != 2) throw new Exception("Invalid arguments");
            var path = Directory.GetCurrentDirectory() + '\\';
            var exe = "<Adjutant.exe>";
            var txt = "<changelog.txt>";

            if (args.Length != 0)
            {
                exe = args[0];
                txt = args[1];
            }

            Console.Out.WriteLine("Please wait while Adjutant is updated...");
            System.Threading.Thread.Sleep(2500); //give time for Adjutant to fully close before attempting file access
            Update(exe, txt, path);
        }

        static void Update(string exe, string txt, string path)
        {
            try
            {
                WebClient client = new WebClient();
                client.DownloadFile(exe, path + "Adjutant.exe");
                client.DownloadFile(txt, path + "changelog.txt");
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine();
                Console.Out.WriteLine("Error updating:");
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine();
                Console.Out.WriteLine("Trying again in 10 seconds...");
                System.Threading.Thread.Sleep(10000);
                Console.Out.WriteLine("Re-attempting download...");
                Update(exe, txt, path);
                return;
            }

            Process.Start(path + "Adjutant.exe");
        }
    }
}
