namespace AdaptiveRoads.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using UnityEngine;

    [Serializable]
    public struct Track {
        public const VehicleInfo.VehicleType TRACK_VEHICLE_TYPES =
            VehicleInfo.VehicleType.Tram |
            VehicleInfo.VehicleType.Metro |
            VehicleInfo.VehicleType.Train |
            VehicleInfo.VehicleType.Monorail |
            VehicleInfo.VehicleType.Trolleybus | VehicleInfo.VehicleType.TrolleybusLeftPole | VehicleInfo.VehicleType.TrolleybusRightPole;


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

        [NonSerialized]
        public NetInfo.LodValue m_combinedLod;

        [NonSerialized]
        public Mesh m_trackMesh;

        [NonSerialized]
        public Material m_trackMaterial;

        [CustomizableProperty("Render On Segments")]
        public bool RenderSegment;

        [CustomizableProperty("Render On Bend Nodes")]
        public bool RenderBend;

        [CustomizableProperty("Render On Nodes")]
        public bool RenderNode;

        public int [] LaneIndeces;

        public Track CreateDefaultTrack(NetInfo netInfo) {
            var laneIndeces = new List<int>();
            for(int i = 0; i < netInfo.m_lanes.Length; ++i) {
                var laneInfo = netInfo.m_lanes[i];
                if(laneInfo.m_vehicleType.IsFlagSet(TRACK_VEHICLE_TYPES)) {
                    laneIndeces.Add(i);
                }
            }

            return new Track {
                RenderNode = true,
                RenderSegment = true,
                RenderBend = true,
                LaneIndeces = laneIndeces.ToArray(),
            };

        }
    }
}
