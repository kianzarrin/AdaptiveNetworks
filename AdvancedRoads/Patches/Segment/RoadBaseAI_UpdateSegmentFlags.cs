using HarmonyLib;
using KianCommons;
namespace AdaptiveRoads.Patches.Segment {
    [HarmonyPatch(typeof(RoadBaseAI))]
    [HarmonyPatch(nameof(RoadBaseAI.UpdateSegmentFlags))]
    class RoadBaseAI_UpdateSegmentFlags {
        static void Postfix(ushort segmentID) {
            Log.Debug("RoadBaseAI_UpdateSegmentFlags.PostFix() was called for segment:" + segmentID);
        }
    }
}