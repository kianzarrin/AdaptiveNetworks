namespace AdvancedRoads.Patches.TMPE {
    using System.Reflection;
    using TrafficManager.Manager.Impl;
    using CSUtil.Commons;
    using AdvancedRoads;
    using AdvancedRoads.Util;
    using HarmonyLib;

    [HarmonyPatch]
    public static class GetDefaultUturnAllowed {
        public static MethodBase TargetMethod() {
            return typeof(JunctionRestrictionsManager).
                GetMethod(nameof(JunctionRestrictionsManager.GetDefaultUturnAllowed));
        }

        public static bool Prefix(ushort segmentId, bool startNode, ref bool __result) {
            ushort nodeID = startNode ? segmentId.ToSegment().m_startNode : segmentId.ToSegment().m_endNode;
            var data = NetworkExtManager.Instance.buffer[nodeID];
            return PrefixUtils.HandleTernaryBool(
                data?.GetDefaultUturnAllowed(),
                ref __result);
        }
    }
}