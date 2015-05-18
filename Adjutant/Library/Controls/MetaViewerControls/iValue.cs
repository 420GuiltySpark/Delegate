using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    internal class iValue
    {
        public iValue(XmlNode Node)
        {
            this.Node = Node;
            Visible = Convert.ToBoolean(Node.Attributes["visible"].Value);
            Type = (ValueType)Enum.Parse(typeof(ValueType), Node.Name, true);
        }

        public XmlNode Node;
        public ValueType Type;
        public bool Visible;

        public enum ValueType
        {
            Struct,

            TagRef,
            
            StringID,
            String,
            
            Bitmask8,
            Bitmask16,
            Bitmask32,

            Comment,
            
            Float32,
            
            Int8,
            Int16,
            Int32,
            
            UInt16,
            UInt32,

            RawID,
           
            Enum8,
            Enum16,
            Enum32,
            
            Undefined,
            
            ShortBounds,
            RealBounds,
            
            ShortPoint2D,
            RealPoint2D,
            RealPoint3D,
            RealPoint4D,

            RealVector2D,
            RealVector3D,
            RealVector4D,

            Colour32RGB,
            Colour32ARGB,
        }

        //private ValueType GetType(string TypeName)
        //{
        //    switch (TypeName.ToLower())
        //    {
        //        case "struct":
        //        case "tagref":
        //        case "string":
        //        case "stringid":
        //        case "bitmask8":
        //        case "bitmask16":
        //        case "bitmask32":
        //            break;
        //    }

        //    return ValueType.Undefined;
        //}
    }
}
