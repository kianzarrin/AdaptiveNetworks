namespace AdaptiveRoads.Patches.Segment {
    using HarmonyLib;
    using KianCommons;
    using AdaptiveRoads.Manager;

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch(nameof(NetManager.UpdateSegmentFlags))]
    class NetManager_UpdateSegmentFlags {
        static void Postfix(ushort segment) {
            ushort segmentID = segment;
            //Log.Debug("NetSegment_UpdateSegment.PostFix() was called for segment:" + segmentID);
            if (!NetUtil.IsSegmentValid(segmentID)) return;
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];

            netSegmentExt.UpdateAllFlags();
        }
    }
}