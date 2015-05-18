using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.Imaging
{
    public struct RGBAColor
    {
        public int R, G, B, A;

        public RGBAColor(int Red, int Green, int Blue, int Alpha)
        {
            R = Red;
            G = Green;
            B = Blue;
            A = Alpha;
        }

        public RGBAColor(int Red, int Green, int Blue)
        {
            R = Red;
            G = Green;
            B = Blue;
            A = 0;
        }
    }
}
