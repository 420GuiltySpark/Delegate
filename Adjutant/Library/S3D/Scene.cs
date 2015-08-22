using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;
using Adjutant.Library;
using Adjutant.Library.S3D;
using Adjutant.Library.Endian;
using Adjutant.Library.Definitions;
using Adjutant.Library.Controls;

namespace Adjutant.Library.S3D
{
    public class Scene : TemplateBase
    {
        public int xF000;
        public int unkAddress0;
        public int x2C01;
        public int unk1;

        public RealQuat unkCoords0;

        public List<S3DScript> Scripts;

        public Scene(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            Name = Item.Name;

            reader.ReadInt16(); //C003
            reader.ReadInt32(); //so far just 22 (0x16000000)
            reader.Skip(16); //maybe all uint16
            reader.ReadInt16(); //5501
            reader.ReadInt32(); //address to end of shaders (0100)

            int count = reader.ReadInt32();
            Materials = new List<S3DMaterial>();
            for (int i = 0; i < count; i++)
                Materials.Add(new S3DMaterial(Pak, Item));

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address to 1F02
            reader.ReadInt16(); //1F02
            reader.ReadInt32(); //address to section after next
            reader.ReadInt16(); //2002
            reader.ReadInt32(); //address to 2102

            reader.ReadInt32(); // ]
            reader.ReadInt32(); // ] unknown purpose, often all 60
            reader.ReadInt32(); // ]
            
            var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            RenderBounds = new render_model.BoundingBox()
            {
                XBounds = new RealBounds(min.x, max.x),
                YBounds = new RealBounds(min.y, max.y),
                ZBounds = new RealBounds(min.z, max.z),
                UBounds = new RealBounds(-1, 1),
                VBounds = new RealBounds(-1, 1)
            };
            unkCoords0 = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            reader.ReadInt16(); //2102
            var addr = reader.ReadInt32(); //address to next section
            reader.ReadInt32(); //object count?
            //loads of empty space here
            
            reader.SeekTo(Item.Offset + addr);

            reader.ReadInt16(); //2202
            addr = reader.ReadInt32(); //address to next section
            reader.ReadInt32(); //max value of int list
            //12 bytes of 0
            //int list; int32 * object count, unknown purpose
            
            reader.SeekTo(Item.Offset + addr);

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address to 8204
            reader.ReadInt16(); //8204
            addr = reader.ReadInt32();

            count = reader.ReadInt32();
            Scripts = new List<S3DScript>();
            for (int i = 0; i < count; i++)
                Scripts.Add(new S3DScript(Pak, Item));

            reader.SeekTo(Item.Offset + addr);
            reader.ReadInt16(); //8404
            addr = reader.ReadInt32();

            reader.SeekTo(Item.Offset + addr);

            xF000 = reader.ReadInt16();
            unkAddress0 = reader.ReadInt32();
            x2C01 = reader.ReadInt16();
            unk1 = reader.ReadInt32(); //address to first object

            count = reader.ReadInt32();
            Objects = new List<S3DObject>();
            for (int i = 0; i < count; i++)
                Objects.Add(new S3DObject(Pak, Item));

            foreach (var obj in Objects)
                if (obj.isInheritor)
                    Objects[obj.inheritIndex].isInherited = true;
            var pos = reader.Position - Item.Offset;
        }

        public override void Parse()
        {
            foreach (var obj in Objects)
            {
                if (!obj.isInheritor && !obj.isInherited && obj.Vertices != null && obj.BoundingBox != null)
                    ModelFunctions.DecompressVertex(ref obj.Vertices, obj.BoundingBox);
            }

            foreach (var obj in Objects)
            {
                if (obj.isInheritor && obj.Submeshes.Count > 0)
                {
                    var pObj = ObjectByID(obj.inheritIndex);
                    int maxVert = 0;
                    int maxIndx = 0;

                    foreach (var sub in obj.Submeshes)
                    {
                        maxVert = Math.Max(maxVert, sub.VertStart + obj.vertOffset + sub.VertLength);
                        maxIndx = Math.Max(maxIndx, sub.FaceStart + obj.faceOffset + sub.FaceLength);
                    }

                    int vLength = maxVert - obj.vertOffset;
                    int fLength = (maxIndx - obj.faceOffset) * 3;

                    obj.Vertices = new Vertex[vLength];
                    obj.Indices = new int[fLength];

                    Array.Copy(pObj.Vertices, obj.vertOffset, obj.Vertices, 0, vLength);
                    Array.Copy(pObj.Indices, obj.faceOffset * 3, obj.Indices, 0, fLength);

                    //if (obj.Vertices != null && obj.BoundingBox != null)
                    //    ModelFunctions.DecompressVertex(ref obj.Vertices, obj.BoundingBox);
                }
            }

            isParsed = true;
        }
    }

    public class S3DScript
    {
        public int xBA01;
        public int AddressOfNext;
        public string Data;

        public S3DScript(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;

            xBA01 = reader.ReadInt16();
            AddressOfNext = reader.ReadInt32();
            Data = reader.ReadNullTerminatedString();
        }
    }
}
