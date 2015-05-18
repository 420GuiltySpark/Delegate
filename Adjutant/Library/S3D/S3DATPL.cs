using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using Adjutant.Library.Definitions;
using System.IO;

namespace Adjutant.Library.S3D
{
    public class S3DATPL : S3DModelBase
    {
        public int x1000;
        public int unk0;
        public int xF000;
        public int PreMatricesAddress;
        public int x2C01;
        public int unk1;

        public S3DATPL(S3DPak Pak, S3DPak.PakItem Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            reader.Skip(16);
            Name = reader.ReadNullTerminatedString();
            reader.Skip(16);

            int count = reader.ReadInt32();
            Materials = new List<S3DMaterial>();
            for (int i = 0; i < count; i++)
                Materials.Add(new S3DMaterial(Pak, Item));

            x1000 = reader.ReadInt16();
            unk0 = reader.ReadInt32();
            xF000 = reader.ReadInt16();
            PreMatricesAddress = reader.ReadInt32();
            x2C01 = reader.ReadInt16();
            unk1 = reader.ReadInt32();

            count = reader.ReadInt32();
            Objects = new List<S3DObject>();
            for (int i = 0; i < count; i++)
                Objects.Add(new S3DObject(Pak, Item));

            foreach (var obj in Objects)
                if (obj.isInheritor)
                    Objects[obj.inheritIndex].isInherited = true;

            try
            {
                reader.SeekTo(Item.Offset + PreMatricesAddress);
                reader.Skip(8);
                reader.SeekTo(Item.Offset + reader.ReadInt32());
               
                reader.ReadInt16(); //E602
                reader.SeekTo(Item.Offset + reader.ReadInt32());
                
                reader.ReadInt16(); //0100
                reader.SeekTo(Item.Offset + reader.ReadInt32());
                
                if (reader.ReadInt16() == 541) //1D02
                    reader.SeekTo(Item.Offset + reader.ReadInt32());
                else //BA01
                {
                    reader.SeekTo(Item.Offset + reader.ReadInt32());
                    reader.ReadInt16(); //1D01
                    reader.SeekTo(Item.Offset + reader.ReadInt32());
                    reader.ReadInt16(); //1103
                    reader.SeekTo(Item.Offset + reader.ReadInt32());
                }
                
                reader.ReadInt16(); //0403
                reader.SeekTo(Item.Offset + reader.ReadInt32());
                
                if(reader.ReadInt16() == 773) //0503
                    reader.SeekTo(Item.Offset + reader.ReadInt32() + 2);
                
                reader.Skip(8);
                var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                RenderBounds = new render_model.BoundingBox();
                RenderBounds.XBounds = new RealBounds(min.x, max.x);
                RenderBounds.YBounds = new RealBounds(min.y, max.y);
                RenderBounds.ZBounds = new RealBounds(min.z, max.z);
                RenderBounds.UBounds = new RealBounds(0f, 0f);
                RenderBounds.VBounds = new RealBounds(0f, 0f);
            }
            catch {
                int x = 0;
            }

            //for (int i = 0; i < Objects.Count; i++)
            //{
            //    var obj = Objects[i];
            //    if (obj.VertCount == 0) continue;
            //    if (obj.BoundingBox.Length == 0)
            //    {
            //        for (int j = 0; j < obj.VertCount; j++)
            //            Controls.ModelFunctions.DecompressVertex(ref obj.Vertices[j], unkBounds0);
            //    }
            //}
        }
    }
}
