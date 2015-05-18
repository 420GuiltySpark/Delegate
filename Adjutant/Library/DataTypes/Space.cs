using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.DataTypes.Space
{
    #region Point
    public struct RealPoint2D
    {
        public float x, y;

        public RealPoint2D(float X, float Y)
        {
            x = X;
            y = Y;
        }

        public override string ToString()
        {
            return x.ToString("F6") + "\t" + y.ToString("F6");
        }

        public static RealPoint2D operator *(RealPoint2D p, float scalar)
        {
            return new RealPoint2D(p.x * scalar, p.y * scalar);
        }

        public static RealPoint2D operator *(float scalar, RealPoint2D p)
        {
            return new RealPoint2D(p.x * scalar, p.y * scalar);
        }
    }

    public struct RealPoint3D
    {
        public float x, y, z;

        public RealPoint3D(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public override string ToString()
        {
            return x.ToString("F6") + "\t" + y.ToString("F6") + "\t" + z.ToString("F6");
        }

        public static RealPoint3D operator *(RealPoint3D p, float scalar)
        {
            return new RealPoint3D(p.x * scalar, p.y * scalar, p.z * scalar);
        }

        public static RealPoint3D operator *(float scalar, RealPoint3D p)
        {
            return new RealPoint3D(p.x * scalar, p.y * scalar, p.z * scalar);
        }
    }

    public struct RealPoint4D
    {
        public float x, y, z, w;

        public RealPoint4D(float X, float Y, float Z, float W)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        /// <summary>
        /// Loads a Point4D from a UDecN4 value
        /// </summary>
        /// <param name="UDecN4">The UDecN4 value as a uint</param>
        public RealPoint4D(uint UDecN4)
        {
            x = (float)(UDecN4 & 0x3ff);
            y = (float)((UDecN4 >> 10) & 0x3ff);
            z = (float)((UDecN4 >> 20) & 0x3ff);
            w = (float)(UDecN4 >> 30);
        }

        public override string ToString()
        {
            return x.ToString("F6") + "\t" + y.ToString("F6") + "\t" + z.ToString("F6") + "\t" + w.ToString("F6");
        }

        public static RealPoint4D operator *(RealPoint4D p, float scalar)
        {
            return new RealPoint4D(p.x * scalar, p.y * scalar, p.z * scalar, p.w * scalar);
        }

        public static RealPoint4D operator *(float scalar, RealPoint4D p)
        {
            return new RealPoint4D(p.x * scalar, p.y * scalar, p.z * scalar, p.w * scalar);
        }

        public static explicit operator RealPoint3D(RealPoint4D point)
        {
            return new RealPoint3D(point.x, point.y, point.z);
        }
    }
    #endregion

    #region Vector
    public struct RealVector2D
    {
        public float i, j;

        public RealVector2D(float I, float J)
        {
            i = I;
            j = J;
        }

        public float Length()
        {
            return (float)Math.Sqrt(i * i + j * j);
        }

        public override string ToString()
        {
            return i.ToString("F6") + "\t" + j.ToString("F6");
        }

        public static RealVector2D operator +(RealVector2D v1, RealVector2D v2)
        {
            return new RealVector2D(v1.i + v2.i, v1.j + v2.j);
        }

        public static RealVector2D operator *(RealVector2D v, float scalar)
        {
            return new RealVector2D(v.i * scalar, v.j * scalar);
        }

        public static RealVector2D operator *(float scalar, RealVector2D v)
        {
            return new RealVector2D(v.i * scalar, v.j * scalar);
        }
    }

    public struct RealVector3D
    {
        public float i, j, k;

        public RealVector3D(float I, float J, float K)
        {
            i = I;
            j = J;
            k = K;
        }

        /// <summary>
        /// Loads a Vector3D from a DHenN3 value
        /// </summary>
        /// <param name="DHenN3">The DHenN3 value as a uint</param>
        public RealVector3D(uint DHenN3)
        {
            uint[] SignExtendX = { 0x00000000, 0xFFFFFC00 };
            uint[] SignExtendYZ = { 0x00000000, 0xFFFFF800 };
            uint temp;

            temp = DHenN3 & 0x3FF;
            i = (float)((temp | SignExtendX[temp >> 9]) / (float)0x1FF);

            temp = (DHenN3 >> 10) & 0x7FF;
            j = (float)((temp | SignExtendYZ[temp >> 10]) / (float)0x3FF);

            temp = (DHenN3 >> 21) & 0x7FF;
            k = (float)((temp | SignExtendYZ[temp >> 10]) / (float)0x3FF);
        }

        public float Length()
        {
            return (float)Math.Sqrt(i * i + j * j + k * k);
        }

        public override string ToString()
        {
            return i.ToString("F6") + "\t" + j.ToString("F6") + "\t" + k.ToString("F6");
        }

        public static RealVector3D operator +(RealVector3D v1, RealVector3D v2)
        {
            return new RealVector3D(v1.i + v2.i, v1.j + v2.j, v1.k + v2.k);
        }

        public static RealVector3D operator *(RealVector3D v, float scalar)
        {
            return new RealVector3D(v.i * scalar, v.j * scalar, v.k * scalar);
        }

        public static RealVector3D operator *(float scalar, RealVector3D v)
        {
            return new RealVector3D(v.i * scalar, v.j * scalar, v.k * scalar);
        }
    }

    public struct RealVector4D
    {
        public float i, j, k, w;

        public RealVector4D(float I, float J, float K, float W)
        {
            i = I;
            j = J;
            k = K;
            w = W;
        }

        public float Length()
        {
            return (float)Math.Sqrt(i * i + j * j + k * k + w * w);
        }

        public override string ToString()
        {
            return i.ToString("F6") + "\t" + j.ToString("F6") + "\t" + k.ToString("F6") + "\t" + w.ToString("F6");
        }

        public static RealVector4D operator +(RealVector4D v1, RealVector4D v2)
        {
            return new RealVector4D(v1.i + v2.i, v1.j + v2.j, v1.k + v2.k, v1.w + v2.w);
        }

        public static RealVector4D operator *(RealVector4D v, float scalar)
        {
            return new RealVector4D(v.i * scalar, v.j * scalar, v.k * scalar, v.w * scalar);
        }

        public static RealVector4D operator *(float scalar, RealVector4D v)
        {
            return new RealVector4D(v.i * scalar, v.j * scalar, v.k * scalar, v.w * scalar);
        }
    }
    #endregion
}
