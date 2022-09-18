namespace AdaptiveRoads.Util {
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    public static class SegmentHelpers {
        public static NetInfo.Segment[] Clone(this NetInfo.Segment[] src) =>
            src.Select(segment => segment.Clone()).ToArray();

        public static NetInfo.Segment Clone(this NetInfo.Segment segment) {
            if (segment is ICloneable cloneable)
                return cloneable.Clone() as NetInfo.Segment;
            else
                return segment.ShalowClone();
        }

        public static float CornerAngle(this ref NetSegment segment, ushort nodeId) {
            return segment.IsStartNode(nodeId) ? segment.m_cornerAngleStart : segment.m_cornerAngleEnd;
        }
    }
}
