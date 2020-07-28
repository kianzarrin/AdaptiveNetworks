namespace AdvancedRoads.Manager {
    public class NetInfoExt {

        public class SegmentInfoFlags {
            public NetSegmentExt.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        public class SegmentEndInfoFlags {
            public NetSegmentEnd.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        public class NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        public class LaneInfoFlags {
            public NetLaneExt.Flags Required, Forbidden;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        public class SegmentInfoExt {
            public SegmentInfoFlags Flags;
            public bool CheckFlags(NetSegmentExt.Flags flags) => Flags.CheckFlags(flags);
        }

        public class NodeInfoExt {
            public NodeInfoFlags NodeFlags;
            public SegmentEndInfoFlags SegmentEndFlags;
            public bool CheckFlags(NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags);
        }

        public class LaneInfoExt {
            public LaneInfoFlags LaneFlags;

            public PropInfoExt[] PropInfoExts;

            public LaneInfoExt(NetInfo.Lane template) {
                PropInfoExts = new PropInfoExt[template.m_laneProps.m_props.Length];
            }
        }

        public class PropInfoExt {
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
        }

        public LaneInfoExt[] LaneInfoExts;

        public SegmentInfoExt[] SegmentInfoExts;

        public NodeInfoExt[] NodeInfoExts;

        public NetInfoExt(NetInfo template) {
            LaneInfoExts = new LaneInfoExt[template.m_lanes.Length];
            SegmentInfoExts = new SegmentInfoExt[template.m_lanes.Length];
            NodeInfoExts = new NodeInfoExt[template.m_lanes.Length];
        }
    }
}
