using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Definitions;
using Adjutant.Library.Controls;
using System.IO;
using System.Drawing;

namespace Adjutant
{
    public class Settings
    {
        public string mapFolder, dataFolder, pluginFolder, classFilter, permFilter;
        public SettingsFlags Flags;
        public Color ViewerColour;
        public Language Language;
        public BitmapFormat BitmFormat;
        public ModelFormat ModeFormat;
        public SoundFormat Snd_Format;
        public byte mapScale;
        public byte pakScale;

        public Settings()
        {
            mapFolder = dataFolder = classFilter = "";
            permFilter = "base default standard";
            pluginFolder = ".\\Plugins\\";
            Flags = SettingsFlags.AutoUpdateCheck | SettingsFlags.BitmapAlpha | SettingsFlags.LoadSpecular | SettingsFlags.SortTags | SettingsFlags.UsePermFilter | SettingsFlags.UseClassFilter;
            ViewerColour = Color.CornflowerBlue;
            Language = Language.English;
            BitmFormat = 0;
            ModeFormat = 0;
            Snd_Format = 0;
        }

        public Settings(MemoryStream Stream)
        {
            var br = new BinaryReader(Stream);

            mapFolder = br.ReadString();
            dataFolder = br.ReadString();
            pluginFolder = br.ReadString();
            classFilter = br.ReadString();
            permFilter = br.ReadString();

            Flags = (SettingsFlags)br.ReadUInt16();
            ViewerColour = Color.FromArgb(br.ReadInt32());
            Language = (Language)br.ReadByte();
            BitmFormat = (BitmapFormat)br.ReadByte();
            ModeFormat = (ModelFormat)br.ReadByte();
            Snd_Format = (SoundFormat)br.ReadByte();
            mapScale = br.ReadByte();
            pakScale = br.ReadByte();
        }

        public MemoryStream ToStream()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            bw.Write(mapFolder);
            bw.Write(dataFolder);
            bw.Write(pluginFolder);
            bw.Write(classFilter);
            bw.Write(permFilter);

            bw.Write((ushort)Flags);
            bw.Write(ViewerColour.ToArgb());
            bw.Write((byte)Language);
            bw.Write((byte)BitmFormat);
            bw.Write((byte)ModeFormat);
            bw.Write((byte)Snd_Format);

            bw.Write(mapScale);
            bw.Write(pakScale);

            return ms;
        }
    }

    [Flags]
    public enum SettingsFlags : ushort
    {
        None             = 0,       //0000000000000000
        AutoUpdateCheck  = 1,       //0000000000000001
        ShortClassNames  = 2,       //0000000000000010
        SortTags         = 4,       //0000000000000100
        OverwriteTags    = 8,       //0000000000001000 IMPLEMENT BETTER
        QuickExtract     = 16,      //0000000000010000
        LoadSpecular     = 32,      //0000000000100000
        BitmapAlpha      = 64,      //0000000001000000
        SplitMeshes      = 128,     //0000000010000000
        UsePermFilter    = 256,     //0000000100000000
        UseClassFilter   = 512,     //0000001000000000
        ShowInvisibles   = 1024,    //0000010000000000
        ForceLoadModels  = 2048,    //0000100000000000
        bit13            = 4096,    //0001000000000000
        bit14            = 8192,    //0010000000000000
        bit15            = 16384,   //0100000000000000
        HierarchyView    = 32768,   //1000000000000000
    }
}
