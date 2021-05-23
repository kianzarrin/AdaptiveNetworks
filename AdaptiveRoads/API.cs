namespace AdaptiveRoads {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AdaptiveRoads.Manager;

    public static class API {
        static NetworkExtensionManager man => NetworkExtensionManager.Instance;

        public static bool IsAdaptive(this NetInfo info) =>
            NetInfoExtionsion.IsAdaptive(info);

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
        public static object GetARSegment(ushort id) => man.SegmentBuffer[id];
        public static object GetARNode(ushort id) => man.NodeBuffer[id];
        public static object GetARSegmentEnd(ushort segmentID, ushort nodeID)
            => man.GetSegmentEnd(segmentID, nodeID);
        public static object GetARSegmentEnd(ushort segmentID, bool startNode)
            => man.GetSegmentEnd(segmentID, startNode);

        public static object GetARSegmentFlags(ushort id) => man.SegmentBuffer[id].m_flags;
        public static object GetARNodeFlags(ushort id) => man.NodeBuffer[id].m_flags;
        public static object GetARSegmentEndFlags(ushort segmentID, ushort nodeID)
            => man.GetSegmentEnd(segmentID, nodeID).m_flags;
        public static object GetARLaneFlags(uint id) => man.LaneBuffer[id].m_flags;

        public static float GetShift(NetInfo info) => info.GetMetaData()?.Shift ?? 0;

#pragma warning restore HAA0601 
    }
}
