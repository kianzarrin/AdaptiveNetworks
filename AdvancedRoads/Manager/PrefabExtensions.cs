using KianCommons;
using System;
using PrefabIndeces;

namespace AdvancedRoads.Manager {
    [Serializable]
    public class NetInfoExt {
        [Serializable]
        public class SegmentInfoFlags {
            public NetSegmentExt.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class SegmentEndInfoFlags {
            public NetSegmentEnd.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class LaneInfoFlags {
            public NetLaneExt.Flags Required, Forbidden;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Serializable]
        public class Segment {
            public class FlagsT {
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

            public Segment(NetInfo.Segment template) { }

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

            public Node(NetInfo.Node template) { }

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

            public static Lane Get(NetInfoExtension.Lane IndexExt) {
                if (IndexExt == null) return null;
                return Buffer[IndexExt.PrefabIndex].LaneInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class LaneProp {
            public LaneInfoFlags LaneFlags;
            public SegmentInfoFlags SegmentFlags;
            public SegmentEndInfoFlags SegmentStartFlags, SegmentEndFlags;
            public NodeInfoFlags StartNodeFlags, EndNodeFlags;
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
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                SegmentInfoExts[i] = new Segment(template.m_segments[i]);
            }

            NodeInfoExts = new Node[template.m_nodes.Length];
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                NodeInfoExts[i] = new Node(template.m_nodes[i]);
            }

            LaneInfoExts = new Lane[template.m_lanes.Length];
            for(int i=0;i<LaneInfoExts.Length;++i) {
                LaneInfoExts[i] = new Lane(template.m_lanes[i]);
            }
        }

        public static NetInfoExt[] Buffer;
        public static NetInfoExt EditInfo;

        public static void Init() => Buffer = new NetInfoExt[PrefabCollection<NetInfo>.PrefabCount()];
    }
}
