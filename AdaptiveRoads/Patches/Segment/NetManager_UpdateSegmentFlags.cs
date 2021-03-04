namespace AdaptiveRoads.Patches.Segment {
    using AdaptiveRoads.Manager;
    using HarmonyLib;

    [InGamePatch]
    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch(nameof(NetManager.UpdateSegmentFlags))]
    class NetManager_UpdateSegmentFlags {
        static void Postfix(ushort segment) {
            NetworkExtensionManager.Instance.SegmentBuffer[segment].UpdateAllFlags();
        }
    }
}