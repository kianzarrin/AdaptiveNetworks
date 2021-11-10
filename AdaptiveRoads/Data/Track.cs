namespace AdaptiveRoads.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AdaptiveRoads.Manager;
    using UnityEngine;

    [Serializable]
    public struct Track {
        [NonSerialized]
        public NetInfo ParentInfo;

        [NonSerialized]
        public int ArrayIndex;

        [NonSerialized]
        public float m_lodRenderDistance;

        [NonSerialized]
        public bool m_requireSurfaceMaps;

        [NonSerialized]
        public bool m_requireHeightMap;

        [NonSerialized]
        public bool m_requireWindSpeed;

        [NonSerialized]
        public bool m_preserveUVs;

        [NonSerialized]
        public bool m_generateTangents;

        [NonSerialized]
        public int m_layer;

        public Mesh m_mesh;

        public Mesh m_lodMesh;

        public Material m_material;

        public Material m_lodMaterial;

        public Mesh m_segmentMesh;

        [NonSerialized]
        public Material m_segmentMaterial;

        [Hint("render this track at nodes")]
        [CustomizableProperty("Node")]
        public bool RenderNode;

        [Hint("used by other mods to decide how hide tracks/medians")]
        [CustomizableProperty("Segment/Bend")]
        public bool RenderSegment;

        [NonSerialized]
        public NetInfo.LodValue m_combinedLod;

        public static int [] LaneIndeces;


        public Track CreateDefaultTrack() {
            return new Track {
                RenderNode = true,
                RenderSegment = true,
            };
        }



    }
}
