using KianCommons;
using PrefabIndeces;
using System;
using System.Linq;

namespace AdvancedRoads.Manager {
    [Serializable]
    public class NetInfoExt {
        [Serializable]
        public struct SegmentInfoFlags {
            public NetSegmentExt.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public struct SegmentEndInfoFlags {
            public NetSegmentEnd.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public struct NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public struct LaneInfoFlags {
            public NetLaneExt.Flags Required, Forbidden;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class Segment {
            public struct FlagsT {
                public SegmentInfoFlags Flags;
                public SegmentEndInfoFlags Start, End;
                public bool CheckFlags(
                    NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags) {
                    return
                        Flags.CheckFlags(flags) &
                        Start.CheckFlags(startFlags) &
                        End.CheckFlags(endFlags);
                }
            }

            public FlagsT ForwardFlags, BackwardFlags;

            public bool CheckFlags(NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags,
                    bool turnAround) {
                if (!turnAround)
                    return ForwardFlags.CheckFlags(flags, startFlags, endFlags);
                else
                    return BackwardFlags.CheckFlags(flags, startFlags, endFlags);
            }

            public static bool CheckFlags(NetInfo.Segment segmentInfo, NetSegment.Flags flags, bool turnAround) {
                if (!turnAround)
                    return flags.CheckFlags(segmentInfo.m_forwardRequired, segmentInfo.m_forwardForbidden);
                else
                    return flags.CheckFlags(segmentInfo.m_backwardRequired, segmentInfo.m_backwardForbidden);
            }

            private Segment() { }
            public Segment(NetInfo.Segment template) { }

            /// <summary>clone</summary>
            public Segment Clone() {
                var clone = new Segment();
                clone.ForwardFlags = this.ForwardFlags;
                clone.BackwardFlags = this.BackwardFlags;
                return clone;
            }

            public static Segment Get(NetInfoExtension.Segment IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex].SegmentInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class Node {
            public NodeInfoFlags NodeFlags;
            public SegmentEndInfoFlags SegmentEndFlags;

            public bool CheckFlags(NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags);

            private Node() { }
            public Node(NetInfo.Node template) { }

            /// <summary>clone</summary>
            public Node Clone() {
                var clone = new Node();
                clone.NodeFlags = this.NodeFlags;
                clone.SegmentEndFlags = this.SegmentEndFlags;
                return clone;
            }

            public static Node Get(NetInfoExtension.Node IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex].NodeInfoExts[IndexExt.Index];
            }

        }

        [Serializable]
        public class Lane {
            public LaneInfoFlags LaneFlags;

            [NonSerialized]
            public LaneProp[] PropInfoExts;

            public Lane(NetInfo.Lane template) {
                PropInfoExts = new LaneProp[template.m_laneProps.m_props.Length];
                for (int i = 0; i < PropInfoExts.Length; ++i) {
                    PropInfoExts[i] = new LaneProp(template.m_laneProps.m_props[i]);
                }
            }
            private Lane() { }
            public Lane Clone() {
                var clone = new Lane();
                clone.PropInfoExts = new LaneProp[PropInfoExts.Length];
                for (int i = 0; i < PropInfoExts.Length; ++i) {
                    clone.PropInfoExts[i] = PropInfoExts[i].Clone();
                }
                return clone;
            }


            public static Lane Get(NetInfoExtension.Lane IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex].LaneInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class LaneProp {
            public LaneInfoFlags LaneFlags = new LaneInfoFlags();
            public SegmentInfoFlags SegmentFlags = new SegmentInfoFlags();
            public SegmentEndInfoFlags SegmentStartFlags = new SegmentEndInfoFlags();
            public SegmentEndInfoFlags SegmentEndFlags = new SegmentEndInfoFlags();
            public NodeInfoFlags StartNodeFlags = new NodeInfoFlags();
            public NodeInfoFlags EndNodeFlags = new NodeInfoFlags();

            public bool CheckFlags(
                NetLaneExt.Flags laneFlags,
                NetSegmentExt.Flags segmentFlags,
                NetNodeExt.Flags startNodeFlags, NetNodeExt.Flags endNodeFlags,
                NetSegmentEnd.Flags segmentStartFlags, NetSegmentEnd.Flags segmentEndFlags) =>
                LaneFlags.CheckFlags(laneFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) &&
                SegmentStartFlags.CheckFlags(segmentStartFlags) &&
                SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                StartNodeFlags.CheckFlags(startNodeFlags) &&
                EndNodeFlags.CheckFlags(endNodeFlags);

            public LaneProp(NetLaneProps.Prop template) { }
            private LaneProp() { }
            public LaneProp Clone() {
                var clone = new LaneProp();
                clone.LaneFlags = LaneFlags;
                clone.SegmentFlags = SegmentFlags;
                clone.SegmentStartFlags = SegmentStartFlags;
                clone.SegmentEndFlags = SegmentEndFlags;
                clone.StartNodeFlags = StartNodeFlags;
                clone.EndNodeFlags = EndNodeFlags;
                return clone;
            }


            public static LaneProp Get(NetInfoExtension.Lane.Prop IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex]
                .LaneInfoExts[IndexExt.LaneIndex]
                .PropInfoExts[IndexExt.Index];
            }
        }

        public Version Version;

        public Node[] NodeInfoExts;

        public Segment[] SegmentInfoExts;

        public Lane[] LaneInfoExts;

        public NetInfoExt(NetInfo template) {
            Version = HelpersExtensions.VersionOf(this);
            SegmentInfoExts = new Segment[template.m_segments.Length];
            for (int i = 0; i < SegmentInfoExts.Length; ++i) {
                SegmentInfoExts[i] = new Segment(template.m_segments[i]);
            }

            NodeInfoExts = new Node[template.m_nodes.Length];
            for (int i = 0; i < NodeInfoExts.Length; ++i) {
                NodeInfoExts[i] = new Node(template.m_nodes[i]);
            }

            LaneInfoExts = new Lane[template.m_lanes.Length];
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                LaneInfoExts[i] = new Lane(template.m_lanes[i]);
            }
        }

        private NetInfoExt() { }
        public NetInfoExt Clone() {
            var clone = new NetInfoExt();
            clone.Version = Version;

            clone.SegmentInfoExts = new Segment[this.SegmentInfoExts.Length];
            for (int i = 0; i < SegmentInfoExts.Length; ++i) {
                clone.SegmentInfoExts[i] = this.SegmentInfoExts[i].Clone();
            }

            clone.NodeInfoExts = new Node[this.NodeInfoExts.Length];
            for (int i = 0; i < NodeInfoExts.Length; ++i) {
                clone.NodeInfoExts[i] = this.NodeInfoExts[i].Clone();
            }

            clone.LaneInfoExts = new Lane[this.LaneInfoExts.Length];
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                clone.LaneInfoExts[i] = this.LaneInfoExts[i].Clone();
            }

            return clone;
        }

        public static NetInfo EditNetInfo =>
            ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;

        public static NetInfoExt EditNetInfoExt {
            get {
                int index = EditNetInfo.GetIndex();
                if (Buffer == null || Buffer.Count() <= index) {
                    Log.Error($"Edited prefab does nto exist. index={index} Buffer.Lenght={Buffer?.Length} " +
                        $"prefab count={PrefabCollection<NetInfo>.PrefabCount()}\n");
                    SetNetInfoExt(index, new NetInfoExt(EditNetInfo)); // recovering from error.
                }
                if (Buffer[index] == null) {
                    SetNetInfoExt(index, new NetInfoExt(EditNetInfo));
                }
                return Buffer[index];
            }
        }


        public static NetInfoExt[] Buffer;
        public static void SetNetInfoExt(int index, NetInfoExt netInfoExt) {
            if (netInfoExt == null)
                return;
            if (Buffer == null)
                Init();
            if (Buffer.Count() < PrefabCollection<NetInfo>.PrefabCount()) {
                var old = Buffer;
                Init();
                for (int i = 0; i < old.Count(); ++i)
                    Buffer[i] = old[i];
            }
            Buffer[index] = netInfoExt;
        }

        public static void Init() {
            Log.Debug("prefab count=" + PrefabCollection<NetInfo>.PrefabCount() + Environment.StackTrace);
            Buffer = new NetInfoExt[PrefabCollection<NetInfo>.PrefabCount()];
        }

        public static void CopyAll(NetInfo source, NetInfo target) {
            NetInfo elevated0 = AssetEditorRoadUtils.TryGetElevated(source);
            NetInfo bridge0 = AssetEditorRoadUtils.TryGetBridge(source);
            NetInfo slope0 = AssetEditorRoadUtils.TryGetSlope(source);
            NetInfo tunnel0 = AssetEditorRoadUtils.TryGetTunnel(source);

            NetInfo elevated1 = AssetEditorRoadUtils.TryGetElevated(target);
            NetInfo bridge1 = AssetEditorRoadUtils.TryGetBridge(target);
            NetInfo slope1 = AssetEditorRoadUtils.TryGetSlope(target);
            NetInfo tunnel1 = AssetEditorRoadUtils.TryGetTunnel(target);

            Copy(target.GetIndex(), source.GetIndex());
            Copy(elevated1.GetIndex(), elevated0.GetIndex());
            Copy(bridge1.GetIndex(), bridge0.GetIndex());
            Copy(slope1.GetIndex(), slope0.GetIndex());
            Copy(tunnel1.GetIndex(), tunnel0.GetIndex());
        }

        public static void Copy(ushort sourceIndex, ushort targetIndex) {
            NetInfoExt sourceNetInfoExt = NetInfoExt.Buffer[sourceIndex];
            NetInfoExt.SetNetInfoExt(targetIndex, sourceNetInfoExt?.Clone());
        }
    }
}
