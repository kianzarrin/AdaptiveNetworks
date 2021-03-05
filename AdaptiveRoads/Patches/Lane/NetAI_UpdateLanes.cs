namespace AdaptiveRoads.Lanes {
    using HarmonyLib;
    using KianCommons;
    using AdaptiveRoads.Manager;

    [HarmonyPatch(typeof(NetAI))]
    [HarmonyPatch(nameof(NetAI.UpdateLanes))]
    [HarmonyAfter("de.viathinksoft.tmpe")]
    [InGamePatch]
    class NetAI_UpdateLanes {
        static void Postfix(ushort segmentID) {
            if (!NetUtil.IsSegmentValid(segmentID)) return;
            foreach (LaneData lane in NetUtil.IterateSegmentLanes(segmentID)) {
                NetworkExtensionManager.Instance.LaneBuffer[lane.LaneID].UpdateLane();
            }
            // TODO update segment end directiosn here?
        }
    }

    [HarmonyPatch(typeof(NetAI))]
    [HarmonyPatch(nameof(NetAI.UpdateLanes))]
    [HarmonyAfter("de.viathinksoft.tmpe")]
    class RoadBaseAI_UpdateLanes {
        static void Postfix(ushort segmentID) {
            if (!NetUtil.IsSegmentValid(segmentID)) return;
            foreach (LaneData lane in NetUtil.IterateSegmentLanes(segmentID)) {
                NetworkExtensionManager.Instance.LaneBuffer[lane.LaneID].UpdateLane();
            }
            // TODO update segment end directiosn here?
        }
    }
}
