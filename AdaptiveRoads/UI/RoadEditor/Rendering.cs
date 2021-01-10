using KianCommons;
using KianCommons.UI;
using UnityEngine;
using static AdaptiveRoads.Patches.RoadEditor.RoadEditorDynamicPropertyToggle_OnEnable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveRoads.UI.RoadEditor {
    internal static class Rendering {
        internal struct PropData {
            internal PropInfo Prop;
            internal Vector3 Pos;
            internal float Angle;
            internal float Scale;
        }
        internal static Queue<PropData> PropRenderQueue = new Queue<PropData>();
        internal static int LaneIndex;
        internal static NetInfo Info;
        internal static NetLaneProps.Prop Prop;

        internal static void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
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

            while(PropRenderQueue.Count > 0) {
                var item = PropRenderQueue.Dequeue();
                PropTool.RenderOverlay(
                    cameraInfo, item.Prop, item.Pos, item.Scale, item.Angle, Color.yellow);
            }
        }


    }
}
