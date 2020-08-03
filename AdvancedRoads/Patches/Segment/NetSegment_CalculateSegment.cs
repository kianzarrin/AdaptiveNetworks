using HarmonyLib;
using KianCommons;
namespace AdvancedRoads.Patches.Segment {
    [HarmonyPatch(typeof(NetSegment))]
    [HarmonyPatch(nameof(NetSegment.CalculateSegment))]
    class NetSegment_CalculateSegment {
        static void Postfix(ushort segmentID) {
            Log.Debug("NetSegment_CalculateSegment.PostFix() was called for segment:" + segmentID);
        }
    }
}