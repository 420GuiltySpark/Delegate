using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes.Space;
using Adjutant.Library.Definitions;

namespace Adjutant.Library.DataTypes
{
    public class Vertex
    {
        public List<RealPoint4D> Positions;
        public List<RealPoint2D> TexPos;
        public RealVector3D Normal, Tangent, Binormal;
        public List<int> Nodes;
        public List<float> Weights;
        public VertexFormat Format;
    }

    public struct RealBounds
    {
        public float Min, Max;

        public RealBounds(float Min, float Max)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public float MidPoint
        {
            get { return (Max + Min) / 2; }
        }

        public float Length
        {
            get { return Max - Min; }
        }
    }

    public struct Bitmask
    {
        public bool[] Values;

        public Bitmask(byte Value)
        {
            Values = new bool[8];

            for (int i = 0; i < 8; i++)
                Values[i] = (Value & (1 << i)) != 0;
        }

        public Bitmask(short Value)
        {
            Values = new bool[16];

            for (int i = 0; i < 16; i++)
                Values[i] = (Value & (1 << i)) != 0;
        }

        public Bitmask(int Value)
        {
            Values = new bool[32];

            for (int i = 0; i < 32; i++)
                Values[i] = (Value & (1 << i)) != 0;
        }
    }
}
