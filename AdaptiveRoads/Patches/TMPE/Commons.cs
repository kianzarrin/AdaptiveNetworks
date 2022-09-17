namespace AdaptiveRoads.Patches.TMPE {
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System;
    using AdaptiveRoads.Util;
    internal static class Commons {
        internal static Type JRManType =>
            Type.GetType("TrafficManager.Manager.Impl.JunctionRestrictionsManager, TrafficManager").LogRet();

        public static bool IsRoadWithDCJunction(ushort segmentId, bool startNode) {
            ref NetSegment segment1 = ref segmentId.ToSegment();
            ushort nodeID = segment1.GetNode(startNode);
            ref NetNode node = ref nodeID.ToNode();
            if (!node.m_flags.IsFlagSet(NetNode.Flags.Junction)) {
                return false;
            }
            ushort segmentId2 = node.GetAnotherSegment(segmentId);
            ref NetSegment segment2 = ref segmentId2.ToSegment();
            NetInfo info1 = segment1.Info;
            NetInfo info2 = segment2.Info;

            bool roadRules1 = (info1?.GetMetaData()?.RoadRules ?? false);
            bool roadRules2 = (info2?.GetMetaData()?.RoadRules ?? false);
            bool roadRules = roadRules1 && roadRules2;
            if (!roadRules) {
                return false;
            }

            bool dc = hasDC(info1, info2) || hasDC(info2, info1);
            return dc;
        }

        static bool hasDC(NetInfo sourceInfo, NetInfo targetInfo) {
            return (sourceInfo.m_nodeConnectGroups & targetInfo.m_connectGroup) != 0 ||
                DirectConnectUtil.ConnectGroupsMatch(
                sourceInfo.GetMetaData()?.CustomConnectGroups.Flags,
                targetInfo.GetMetaData()?.CustomConnectGroups.Flags);
        }
    }
}
