namespace AdvancedRoads.Lanes {
    using HarmonyLib;
    using KianCommons;

    [HarmonyPatch(typeof(NetAI))]
    [HarmonyPatch(nameof(NetAI.UpdateLanes))]
    [HarmonyAfter("de.viathinksoft.tmpe")]
    class NetAI_UpdateLanes {
        static void Postfix(ref NetAI __instance, ushort segmentID) {
            if (!NetUtil.IsSegmentValid(segmentID)) return;
            foreach (LaneData lane in NetUtil.IterateSegmentLanes(segmentID)) {
                NetworkExtensionManager.Instance.LaneBuffer[lane.LaneID].UpdateLane();
            }
            // TODO update segment end directiosn here?
        }
    }
}
