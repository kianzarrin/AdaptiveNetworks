using KianCommons;
using KianCommons.UI;
using UnityEngine;
using static AdaptiveRoads.Patches.RoadEditor.DPT_OnEnable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveRoads.UI.RoadEditor {
    internal static class Overlay {
        internal struct PropData {
            internal PropInfo Prop;
            internal Vector3 Pos;
            internal float Angle;
            internal float Scale;
        }
        internal static NetLaneProps.Prop Prop;
        internal static Queue<PropData> PropQueue = new Queue<PropData>();
        
        internal static NetInfo Info;
        internal static int LaneIndex;

        internal struct SegmentData {
            internal ushort SegmentID;
            internal bool TurnAround;
        }
        internal static NetInfo.Segment SegmentInfo;
        internal static Queue<SegmentData> SegmentQueue = new Queue<SegmentData>();


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

            while (SegmentQueue.Count > 0) {
                var item = SegmentQueue.Dequeue();
                Color color = Color.yellow;
                if (item.TurnAround)
                    color = new Color32(255, 165, 0, 255); //orange

                NetTool.RenderOverlay(
                    cameraInfo, ref item.SegmentID.ToSegment(),
                    color, color);
            }


            while (PropQueue.Count > 0) {
                var item = PropQueue.Dequeue();
                PropTool.RenderOverlay(
                    cameraInfo, item.Prop, item.Pos, item.Scale, item.Angle, Color.yellow);
            }
        }


    }
}
