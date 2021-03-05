using HarmonyLib;
using KianCommons;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.Patches.Segment {
    [HarmonyPatch(typeof(NetSegment))]
    [HarmonyPatch(nameof(NetSegment.UpdateSegment))]
    [InGamePatch]
    class NetSegment_UpdateSegment {
        static void Postfix(ushort segmentID) {
            Log.Debug("NetSegment_UpdateSegment.PostFix() was called for segment:" + segmentID);
            NetworkExtensionManager.Instance.UpdateSegment(segmentID);
        }
    }
}