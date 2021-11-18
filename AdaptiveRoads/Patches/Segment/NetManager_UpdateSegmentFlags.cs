namespace AdaptiveRoads.Patches.Segment {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;

    [InGamePatch]
    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch(nameof(NetManager.UpdateSegmentFlags))]
    class NetManager_UpdateSegmentFlags {
        static void Postfix(ushort segment) {
            //Log.Called(segment);
            NetworkExtensionManager.Instance.UpdateSegment(segment);
        }
    }
}