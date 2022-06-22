namespace AdaptiveRoads.Manager {
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using UnityEngine;
    using AdaptiveRoads.Util;
    using static AdaptiveRoads.UI.ModSettings;
    using static KianCommons.ReflectionHelpers;
    using System.Xml.Serialization;

    public static partial class NetInfoExtionsion {
        [AfterField(nameof(NetInfo.Node.m_flagsForbidden))]
        [Serializable]
        [Optional(AR_MODE)]
        public class Node : IMetaData, IModel {
            public const string DC_GROUP_NAME = "Direct Connect";

            [CustomizableProperty("Node Extension")]
            public NodeInfoFlags NodeFlags;

            [CustomizableProperty("Segment End")]
            public SegmentEndInfoFlags SegmentEndFlags;

            [CustomizableProperty("Segment")]
            [Optional(NODE_SEGMENT)]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Extension")]
            [Optional(NODE_SEGMENT)]
            public SegmentInfoFlags SegmentFlags;

            [Hint("Apply the same flag requirements to target segment end")]
            [CustomizableProperty("Check target flags", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public bool CheckTargetFlags;

            public string[] ConnectGroups;

            [NonSerialized] [XmlIgnore]
            public int[] ConnectGroupsHash;

            [Hint("used by other mods to decide how hide tracks/medians")]
            [CustomizableProperty("Lane Type", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public NetInfo.LaneType LaneType;

            [Hint("used by other mods to decide how hide tracks/medians")]
            [CustomizableProperty("Vehicle Type", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public VehicleInfo.VehicleType VehicleType;

            [Hint("tell DCR mod to manage this node")]
            [CustomizableProperty("Hide Broken Medians", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public bool HideBrokenMedians = true;

            [CustomizableProperty("Tiling")]
            [Hint("network tiling value")]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public float Tiling;

            [CustomizableProperty("Title")]
            [Hint("title to display(asset editor only)")]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public string Title;
            [XmlIgnore] string IModel.Title => Title;


            public bool CheckFlags(
                NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags,
                NetSegmentExt.Flags segmentFlags, NetSegment.Flags vanillaSegmentFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags);

            public CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                SegmentEnd = SegmentEndFlags.UsedCustomFlags,
                Node = NodeFlags.UsedCustomFlags,
            };

            public void Update() {
                ConnectGroupsHash = ConnectGroups?.Select(item => item.GetHashCode()).ToArray();
            }

            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Node() { }
            public Node(NetInfo.Node template) { }
            public Node Clone() => this.ShalowClone();
            object ICloneable.Clone() => Clone();
            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.GetObjectFields(info, this);

            // deserialization
            public Node(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.SetObjectFields(info, this);
            #endregion

            public void SetupTiling(NetInfo.Node nodeInfo) {
                if(Tiling != 0) {
                    nodeInfo.m_material?.SetTiling(Tiling);
                    nodeInfo.m_nodeMaterial?.SetTiling(Tiling);
                    nodeInfo.m_lodMaterial?.SetTiling(Tiling);
                    nodeInfo.m_combinedLod?.m_material?.SetTiling(Mathf.Abs(Tiling));
                }
            }
        }
    }
}
