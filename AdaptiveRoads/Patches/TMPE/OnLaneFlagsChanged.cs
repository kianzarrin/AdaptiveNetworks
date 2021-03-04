namespace AdaptiveRoads.Patches.TMPE {
    using HarmonyLib;
    using KianCommons;
    using TrafficManager.Manager.Impl;
    using System;
    using System.Diagnostics;
    using AdaptiveRoads.Manager;

    [HarmonyPatch]
    class OnLaneFlagsChanged {
        // private void LaneArrowManager.OnLaneChange(uint laneId) {
        [InGamePatch]
        [HarmonyPatch(typeof(LaneArrowManager))]
        [HarmonyPatch("OnLaneChange")]
        static void Postfix(uint laneId) {
            ushort segmentId = laneId.ToLane().m_segment;
            Log.Debug($"OnLaneFlagsChanged.PostFix() was called for " +
                $"laneid:{laneId} segment:{segmentId} " +
                $"caller:{new StackFrame(1)}");
            NetworkExtensionManager.Instance.UpdateSegment(segmentId);
        }
    }
}
