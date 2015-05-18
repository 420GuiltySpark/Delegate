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
    public class S3DBSP : S3DModelBase
    {
        //public string Name;
        //public List<S3DATPL.ATPL_Material> Materials;
        //public List<S3DATPL.ATPL_Object> Objects;

        public int xF000;
        public int unkAddress0;
        public int x2C01;
        public int unk1;

        //public render_model.BoundingBox RenderBounds;
        public RealQuat unkCoords0;

        public S3DBSP(S3DPak Pak, S3DPak.PakItem Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            Name = Item.Name;
            reader.Skip(28);

            int count = reader.ReadInt32();
            Materials = new List<S3DMaterial>();
            for (int i = 0; i < count; i++)
                Materials.Add(new S3DMaterial(Pak, Item));

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address to 1F02
            reader.ReadInt16(); //1F02
            var addr = reader.ReadInt32();
            reader.ReadInt16(); //2002
            reader.ReadInt32(); //address to 2102
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
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
            reader.ReadInt32(); //address
            reader.ReadInt32(); //object count?

            reader.SeekTo(Item.Offset + addr);
            reader.Skip(8); //0100, address, 8204
            addr = reader.ReadInt32();
            reader.SeekTo(Item.Offset + addr);
            reader.ReadInt16(); //8404
            addr = reader.ReadInt32();
            reader.SeekTo(Item.Offset + addr);

            xF000 = reader.ReadInt16();
            unkAddress0 = reader.ReadInt32();
            x2C01 = reader.ReadInt16();
            unk1 = reader.ReadInt32();

            count = reader.ReadInt32();
            Objects = new List<S3DObject>();
            for (int i = 0; i < count; i++)
                Objects.Add(new S3DObject(Pak, Item));

            foreach (var obj in Objects)
                if (obj.isInheritor)
                    Objects[obj.inheritIndex].isInherited = true;
            var pos = reader.Position - Item.Offset;
        }

        //public S3DATPL.ATPL_Object ObjectByID(int ID)
        //{
        //    foreach (var obj in Objects)
        //        if (obj.ID == ID) return obj;

        //    return null;
        //}
    }
}
