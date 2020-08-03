using HarmonyLib;
using KianCommons;
namespace AdvancedRoads.Patches.Segment {
    [HarmonyPatch(typeof(NetSegment))]
    [HarmonyPatch(nameof(NetSegment.UpdateSegment))]
    class NetSegment_UpdateSegment {
        static void Postfix(ushort segmentID) {
            Log.Debug("NetSegment_UpdateSegment.PostFix() was called for segment:" + segmentID);
        }
    }
}