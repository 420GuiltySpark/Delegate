using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class scenario_structure_bsp
    {
        public CacheBase cache;
        public int geomRawID;
        public string BSPName;

        public RealBounds XBounds, YBounds, ZBounds;

        public List<Cluster> Clusters;
        public List<InstancedGeometry> GeomInstances;
        public List<render_model.Shader> Shaders;
        public List<render_model.ModelSection> ModelSections;
        public List<render_model.BoundingBox> BoundingBoxes;

        public int RawID1; //decorator vertex buffers
        public int RawID2; //???
        public int RawID3; //bsp raw

        public List<render_model.VertexBufferInfo> VertInfoList;
        public List<render_model.UnknownInfo1> Unknown1List;
        public List<render_model.IndexBufferInfo> IndexInfoList;
        public List<render_model.UnknownInfo2> Unknown2List;
        public List<render_model.UnknownInfo3> Unknown3List;

        public bool RawLoaded = false;

        public scenario_structure_bsp()
        {
            Clusters = new List<Cluster>();
            GeomInstances = new List<InstancedGeometry>();
            Shaders = new List<render_model.Shader>();
            ModelSections = new List<render_model.ModelSection>();
            BoundingBoxes = new List<render_model.BoundingBox>();
        }

        public virtual void LoadRaw()
        {
            throw new NotImplementedException();
        }

        public abstract class Cluster
        {
            public RealBounds XBounds, YBounds, ZBounds;

            public int SectionIndex;
        }

        public abstract class InstancedGeometry
        {
            public float TransformScale;
            public Matrix TransformMatrix;
            public int SectionIndex;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
