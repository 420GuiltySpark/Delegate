using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;

namespace Adjutant.Library.DataTypes
{
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

        public override string ToString()
        {
            return Min.ToString() + ", " + Max.ToString();
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

    public struct Matrix
    {
        public float m11, m12, m13;
        public float m21, m22, m23;
        public float m31, m32, m33;
        public float m41, m42, m43;

        public bool IsIdentity
        {
            get
            {
                if (m11 == 1 && m12 == 0 && m13 == 0 &&
                    m21 == 0 && m22 == 1 && m23 == 0 &&
                    m31 == 0 && m32 == 0 && m33 == 1 &&
                    m41 == 0 && m42 == 0 && m43 == 0)
                    return true;
                else
                    return false;

            }
        }

        public static Matrix Identity
        {
            get
            {
                var m = new Matrix();

                m.m11 = m.m22 = m.m33 = 1;

                return m;
            }
        }

        public bool Equals(Matrix M)
        {
            return (
                m11 == M.m11 &&
                m12 == M.m12 &&
                m13 == M.m13 &&
                m21 == M.m21 &&
                m22 == M.m22 &&
                m23 == M.m23 &&
                m31 == M.m31 &&
                m32 == M.m32 &&
                m33 == M.m33 &&
                m41 == M.m41 &&
                m42 == M.m42 &&
                m43 == M.m43);
        }

        public Matrix(float M11, float M12, float M13,
            float M21, float M22, float M23,
            float M31, float M32, float M33,
            float M41, float M42, float M43)
        {
            m11 = M11; m12 = M12; m13 = M13;
            m21 = M21; m22 = M22; m23 = M23;
            m31 = M31; m32 = M32; m33 = M33;
            m41 = M41; m42 = M42; m43 = M43;
        }

        public static implicit operator System.Windows.Media.Media3D.Matrix3D(Matrix m)
        {
            return new System.Windows.Media.Media3D.Matrix3D(
                m.m11, m.m12, m.m13, 0,
                m.m21, m.m22, m.m23, 0,
                m.m31, m.m32, m.m33, 0,
                m.m41, m.m42, m.m43, 1);
        }

        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.IsIdentity) return matrix2;
            if (matrix2.IsIdentity) return matrix1;

            Matrix result = new Matrix(
                matrix1.m11 * matrix2.m11 + matrix1.m12 * matrix2.m21 +
                matrix1.m13 * matrix2.m31 + 0 * matrix2.m41,
                matrix1.m11 * matrix2.m12 + matrix1.m12 * matrix2.m22 +
                matrix1.m13 * matrix2.m32 + 0 * matrix2.m42,
                matrix1.m11 * matrix2.m13 + matrix1.m12 * matrix2.m23 +
                matrix1.m13 * matrix2.m33 + 0 * matrix2.m43,
                matrix1.m21 * matrix2.m11 + matrix1.m22 * matrix2.m21 +
                matrix1.m23 * matrix2.m31 + 0 * matrix2.m41,
                matrix1.m21 * matrix2.m12 + matrix1.m22 * matrix2.m22 +
                matrix1.m23 * matrix2.m32 + 0 * matrix2.m42,
                matrix1.m21 * matrix2.m13 + matrix1.m22 * matrix2.m23 +
                matrix1.m23 * matrix2.m33 + 0 * matrix2.m43,
                matrix1.m31 * matrix2.m11 + matrix1.m32 * matrix2.m21 +
                matrix1.m33 * matrix2.m31 + 0 * matrix2.m41,
                matrix1.m31 * matrix2.m12 + matrix1.m32 * matrix2.m22 +
                matrix1.m33 * matrix2.m32 + 0 * matrix2.m42,
                matrix1.m31 * matrix2.m13 + matrix1.m32 * matrix2.m23 +
                matrix1.m33 * matrix2.m33 + 0 * matrix2.m43,
                matrix1.m41 * matrix2.m11 + matrix1.m42 * matrix2.m21 +
                matrix1.m43 * matrix2.m31 + 1 * matrix2.m41,
                matrix1.m41 * matrix2.m12 + matrix1.m42 * matrix2.m22 +
                matrix1.m43 * matrix2.m32 + 1 * matrix2.m42,
                matrix1.m41 * matrix2.m13 + matrix1.m42 * matrix2.m23 +
                matrix1.m43 * matrix2.m33 + 1 * matrix2.m43);

            return result;
        } 
    }

    /// <summary>
    /// Multipurpose quaternion, can be used as point or vector from 1 to 4 dimensions, or just as a collection of 1 to 4 real values.
    /// </summary>
    [Serializable]
    public class RealQuat
    {
        private float[] values;
        public float a
        {
            get { return values[0]; }
            set { values[0] = value; }
        }
        public float b
        {
            get { return values[1]; }
            set { values[1] = value; }
        }
        public float c
        {
            get { return values[2]; }
            set { values[2] = value; }
        }
        public float d
        {
            get { return values[3]; }
            set { values[3] = value; }
        }
        public int Dimensions
        {
            get { return values.Length; }
        }

        //XYZW and IJKW alias for vectors and points.
        //no functionality, just for readability when
        //used in different areas
        #region PointUsage
        public float x
        {
            get { return a; }
            set { a = value; }
        }

        public float y
        {
            get { return b; }
            set { b = value; }
        }

        public float z
        {
            get { return c; }
            set { c = value; }
        }

        public float w
        {
            get { return d; }
            set { d = value; }
        }
        #endregion
        #region VectorUsage
        public float i
        {
            get { return a; }
            set { a = value; }
        }

        public float j
        {
            get { return b; }
            set { b = value; }
        }

        public float k
        {
            get { return c; }
            set { c = value; }
        }
        #endregion

        #region init
        public RealQuat()
        {
            values = new float[4];
        }

        public RealQuat(float A)
        {
            values = new float[1];
            a = A;
        }

        public RealQuat(float A, float B)
        {
            values = new float[2];
            a = A;
            b = B;
        }

        public RealQuat(float A, float B, float C)
        {
            values = new float[3];
            a = A;
            b = B;
            c = C;
        }

        public RealQuat(float A, float B, float C, float D)
        {
            values = new float[4];
            a = A;
            b = B;
            c = C;
            d = D;
        }
        #endregion

        public float Length
        {
            get { return (float)Math.Sqrt(a * a + b * b + c * c + d * d); }
        }

        public override string ToString()
        {
            string s = a.ToString("F6") + ", " + b.ToString("F6");
            if (values.Length > 2) s += ", " + c.ToString("F6");
            if (values.Length > 3) s += ", " + d.ToString("F6");
            return s;
        }

        public string ToString(int Dimensions, string Separator)
        {
            if (Dimensions < 1 || Dimensions > 4) throw new ArgumentOutOfRangeException("Dimensions", "Dimensions must be between 1 and 4 (inclusive).");
            string val = a.ToString("F6");

            if (Dimensions > 1) val += Separator + b.ToString("F6");
            if (Dimensions > 2) val += Separator + c.ToString("F6");
            if (Dimensions > 3) val += Separator + d.ToString("F6");

            return val;
        }

        public void Point3DTransform(Matrix Transform)
        {
            if (Transform.IsIdentity) return;

            float newX = (x * Transform.m11 + y * Transform.m21 + z * Transform.m31 + Transform.m41);
            float newY = (x * Transform.m12 + y * Transform.m22 + z * Transform.m32 + Transform.m42);
            float newZ = (x * Transform.m13 + y * Transform.m23 + z * Transform.m33 + Transform.m43);

            x = newX;
            y = newY;
            z = newZ;
        }

        public void Vector3DTransform(Matrix Transform)
        {
            if (Transform.IsIdentity) return;

            float newX = x * Transform.m11 + y * Transform.m21 + z * Transform.m31;
            float newY = x * Transform.m12 + y * Transform.m22 + z * Transform.m32;
            float newZ = x * Transform.m13 + y * Transform.m23 + z * Transform.m33;

            x = newX;
            y = newY;
            z = newZ;
        }

        #region operators
        //public static RealQuat operator +(RealQuat q1, RealQuat q2)
        //{
        //    if (q1.Dimensions <= 2 && q2.Dimensions <= 2) return new RealQuat(q1.a + q2.a, q1.b + q2.b);
        //    else if (q1.Dimensions <= 3 && q2.Dimensions <= 3) return new RealQuat(q1.a + q2.a, q1.b + q2.b, q1.c + q2.c);
        //    else return new RealQuat(q1.a + q2.a, q1.b + q2.b, q1.c + q2.c, q1.d + q2.d);
        //}

        public static RealQuat operator *(RealQuat q, float scalar)
        {
            if (q.Dimensions <= 2) return new RealQuat(q.a * scalar, q.b * scalar);
            else if (q.Dimensions == 3) return new RealQuat(q.a * scalar, q.b * scalar, q.c * scalar);
            else return new RealQuat(q.a * scalar, q.b * scalar, q.c * scalar, q.d * scalar);
        }

        public static RealQuat operator *(float scalar, RealQuat q)
        {
            return (q * scalar);
        }
        #endregion

        #region static From...
        // 10/11/11/00
        public static RealQuat FromDHenN3(uint DHenN3)
        {
            float a, b, c;
            uint[] SignExtendX = { 0x00000000, 0xFFFFFC00 };
            uint[] SignExtendYZ = { 0x00000000, 0xFFFFF800 };
            uint temp;

            temp = DHenN3 & 0x3FF;
            a = (float)(short)(temp | SignExtendX[temp >> 9]) / (float)0x1FF;

            temp = (DHenN3 >> 10) & 0x7FF;
            b = (float)(short)(temp | SignExtendYZ[temp >> 10]) / (float)0x3FF;

            temp = (DHenN3 >> 21) & 0x7FF;
            c = (float)(short)(temp | SignExtendYZ[temp >> 10]) / (float)0x3FF;

            //q.d = 0;

            return new RealQuat(a, b, c);
        }

        public static RealQuat FromUDHenN3(uint UDHenN3)
        {
            float a, b, c;
            a = (float)(UDHenN3 & 0x3FF)         / (float)0x3FF;
            b = (float)((UDHenN3 >> 10) & 0x7FF) / (float)0x7FF;
            c = (float)((UDHenN3 >> 21) & 0x7FF) / (float)0x7FF;
            //q.d = 0;
            return new RealQuat(a, b, c);
        }

        // 11/11/10/00
        public static RealQuat FromHenDN3(uint HenDN3)
        {
            float a, b, c;
            uint[] SignExtendXY = { 0x00000000, 0xFFFFF800 };
            uint[] SignExtendZ = { 0x00000000, 0xFFFFFC00 };
            uint temp;

            temp = HenDN3 & 0x7FF;
            a = (float)(short)(temp | SignExtendXY[temp >> 10]) / (float)0x3FF;

            temp = (HenDN3 >> 11) & 0x7FF;
            b = (float)(short)(temp | SignExtendXY[temp >> 10]) / (float)0x3FF;

            temp = (HenDN3 >> 22) & 0x3FF;
            c = (float)(short)(temp | SignExtendZ[temp >> 9]) / (float)0x1FF;

            //q.d = 0;

            return new RealQuat(a, b, c);
        }

        public static RealQuat FromUHenDN3(uint UHenDN3)
        {
            float a, b, c;
            a = (float)(UHenDN3 & 0x7FF)         / (float)0x7FF;
            b = (float)((UHenDN3 >> 11) & 0x7FF) / (float)0x7FF;
            c = (float)((UHenDN3 >> 22) & 0x3FF) / (float)0x3FF;
            //q.d = 0;
            return new RealQuat(a, b, c);
        }

        // 10/10/10/02
        public static RealQuat FromDecN4(uint DecN4)
        {
            float a, b, c, d;

            uint[] SignExtend = {0x00000000, 0xFFFFFC00};
            uint[] SignExtendW = {0x00000000, 0xFFFFFFFC};
            uint temp;

            temp = DecN4 & 0x3FF;
            a = (float)(short)(temp | SignExtend[temp >> 9]) / 511.0f;
            
            temp = (DecN4 >> 10) & 0x3FF;
            b = (float)(short)(temp | SignExtend[temp >> 9]) / 511.0f;
            
            temp = (DecN4 >> 20) & 0x3FF;
            c = (float)(short)(temp | SignExtend[temp >> 9]) / 511.0f;
            
            temp = DecN4 >> 30;
            d = (float)(short)(temp | SignExtendW[temp >> 1]);

            return new RealQuat(a, b, c, d);
        }

        public static RealQuat FromUDecN4(uint UDecN4)
        {
            float a, b, c, d;
            a = (float)(UDecN4 & 0x3FF)         / (float)0x3FF;
            b = (float)((UDecN4 >> 10) & 0x3FF) / (float)0x3FF;
            c = (float)((UDecN4 >> 20) & 0x3FF) / (float)0x3FF;
            d = (float)(UDecN4 >> 30)           / (float)0x003;
            return new RealQuat(a, b, c, d);
        }

        public static RealQuat FromUByte4(uint UByte4)
        {
            float a, b, c, d;
            d = (float)(UByte4 & 0xFF);
            c = (float)((UByte4 >> 8) & 0xFF);
            b = (float)((UByte4 >> 16) & 0xFF);
            a = (float)(UByte4 >> 24);
            return new RealQuat(a, b, c, d);
        }

        public static RealQuat FromUByteN4(uint UByteN4)
        {
            float a, b, c, d;
            d = (float)(UByteN4 & 0xFF)         / (float)0xFF;
            c = (float)((UByteN4 >> 8) & 0xFF)  / (float)0xFF;
            b = (float)((UByteN4 >> 16) & 0xFF) / (float)0xFF;
            a = (float)(UByteN4 >> 24)          / (float)0xFF;
            return new RealQuat(a, b, c, d);
        }


        // 11/11/10/00
        public static RealQuat From11101100(uint HenDN3)
        {
            float a, b, c;
            uint[] SignExtendXZ = { 0x00000000, 0xFFFFF800 };
            uint[] SignExtendY = { 0x00000000, 0xFFFFFC00 };
            uint temp;

            temp = HenDN3 & 0x7FF;
            a = (float)(short)(temp | SignExtendXZ[temp >> 10]) / (float)0x3FF;

            temp = (HenDN3 >> 10) & 0x3FF;
            b = (float)(short)(temp | SignExtendY[temp >> 9]) / (float)0x1FF;

            temp = (HenDN3 >> 21) & 0x7FF;
            c = (float)(short)(temp | SignExtendXZ[temp >> 10]) / (float)0x3FF;

            //q.d = 0;

            return new RealQuat(a, b, c);
        }
        #endregion
    }
}
