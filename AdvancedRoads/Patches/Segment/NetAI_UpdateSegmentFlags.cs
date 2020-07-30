using ColossalFramework;
using HarmonyLib;
using KianCommons;

namespace AdvancedRoads.Patches.Segment {
    using Util;
    [HarmonyPatch(typeof(NetAI), nameof(NetAI.UpdateSegmentFlags))]
    class NetAI_UpdateSegmentFlags {
        static void Postfix(ushort segmentID, ref NetSegment data) {
            //Log.Debug("NetAI_UpdateSegmentFlags.PostFix() was called");
            if (!NetUtil.IsNodeValid(segmentID)) return;
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];

            netSegmentExt.Start.UpdateFlags();
            netSegmentExt.Start.UpdateDirections();

            netSegmentExt.End.UpdateFlags();
            netSegmentExt.End.UpdateDirections();
        } // end postfix
    }
}