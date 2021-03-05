using HarmonyLib;
using KianCommons;
namespace AdaptiveRoads.Patches.Segment {
    [HarmonyPatch(typeof(NetSegment))]
    [HarmonyPatch(nameof(NetSegment.CalculateSegment))]
    [InGamePatch]
    class NetSegment_CalculateSegment {
        static void Postfix(ushort segmentID) {
            Log.Debug("NetSegment_CalculateSegment.PostFix() was called for segment:" + segmentID);
        }
    }
}