namespace AdaptiveRoads.Patches {
    using HarmonyLib;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;
    using static AdaptiveRoads.Patches.RoadEditor.RoadEditorDynamicPropertyToggle_OnEnable;

    [HarmonyPatch(typeof(RenderManager), "Managers_RenderOverlay")]
    public static class Managers_RenderOverlay {
        public static void Postfix(RenderManager.CameraInfo cameraInfo) {
            //Log.DebugWait($"Managers_RenderOverlay.Postfix(): LaneIndex={LaneIndex} Info={Info}");
            if (LaneIndex >= 0 && Info != null) {
                for (ushort segmentID = 1; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                    if (!NetUtil.IsSegmentValid(segmentID))
                        continue;
                    if (segmentID.ToSegment().Info != Info)
                        continue;
                    uint laneID = NetUtil.GetlaneID(segmentID, LaneIndex);
                    LaneData lane = new LaneData(laneID, LaneIndex);
                    RenderUtil.RenderLaneOverlay(
                        cameraInfo,
                        lane,
                        Color.yellow);
                }
            }
        }
    }
}

