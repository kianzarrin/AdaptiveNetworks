using ColossalFramework;
using HarmonyLib;
using KianCommons;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.Patches.Segment {
    
    [HarmonyPatch(typeof(NetSegment), nameof(NetSegment.CalculateSegment))]
    class CalculateSegment {
        static void Postfix(ref NetSegment __instance) {
            if (!__instance.IsValid())return;
            //Log.Debug("CalculateSegment.PostFix() was called");
            ushort segmentID = NetUtil.GetID(__instance);
            NetworkExtensionManager.Instance.UpdateSegment(segmentID);
        } // end postfix
    }
}