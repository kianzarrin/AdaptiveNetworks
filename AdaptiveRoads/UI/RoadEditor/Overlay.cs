using KianCommons;
using KianCommons.UI;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;

namespace AdaptiveRoads.UI.RoadEditor {
    internal static class Overlay {
        static Color ORANGE = new Color32(255, 165, 0, 255); //orange

        internal struct PropData {
            internal PropInfo Prop;
            internal Vector3 Pos;
            internal float Angle;
            internal float Scale;
        }
        internal static object HoveredInfo; // NetInfo[.Node/Segment/Prop]

        internal static Queue<PropData> PropQueue = new Queue<PropData>();

        internal struct SegmentData {
            internal ushort SegmentID;
            internal bool TurnAround;
        }
        internal static Queue<SegmentData> SegmentQueue = new Queue<SegmentData>();

        internal struct NodeData {
            internal ushort NodeID;
            internal ushort SegmentID;
            internal ushort SegmentID2; // DC and bend
            internal bool IsBendNode;
            internal bool TurnAround; // bend node only

            internal bool IsDC => SegmentID2 != 0 && !IsBendNode;

        }
        internal static Queue<NodeData> NodeQueue = new Queue<NodeData>();

        internal static void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            //Log.DebugWait($"Managers_RenderOverlay.Postfix(): LaneIndex={LaneIndex} Info={Info}");
            if (HoveredInfo is NetInfo.Lane laneInfo) {
                for (ushort segmentID = 1; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                    if (!NetUtil.IsSegmentValid(segmentID)) continue;
                    var m_lanes = segmentID.ToSegment().Info.m_lanes;
                    int laneIndex = m_lanes.IndexOf(laneInfo);
                    if (laneIndex < 0) continue;
                    uint laneID = NetUtil.GetlaneID(segmentID, laneIndex);
                    LaneData lane = new LaneData(laneID, laneIndex);
                    RenderUtil.RenderLaneOverlay(
                        cameraInfo,
                        lane,
                        Color.yellow);
                }
            }

            while (SegmentQueue.Count > 0) {
                var item = SegmentQueue.Dequeue();
                Color color = item.TurnAround ? ORANGE :Color.yellow;

                NetTool.RenderOverlay(
                    cameraInfo, ref item.SegmentID.ToSegment(),
                    color, color);
            }

            while (NodeQueue.Count > 0) {
                var item = NodeQueue.Dequeue();
                if (item.IsBendNode) {
                    RenderUtil.DrawNodeCircle(
                        cameraInfo,
                        Color.yellow,
                        item.NodeID);
                    continue;
                } 
                RenderUtil.DrawCutSegmentEnd(
                    cameraInfo: cameraInfo,
                    segmentId: item.SegmentID,
                    cut: 0.5f,
                    bStartNode: item.SegmentID.ToSegment().IsStartNode(item.NodeID),
                    color: Color.yellow);
                if (item.IsDC) {
                    RenderUtil.DrawCutSegmentEnd(
                        cameraInfo: cameraInfo,
                        segmentId: item.SegmentID,
                        cut: 0.5f,
                        bStartNode: item.SegmentID.ToSegment().IsStartNode(item.NodeID),
                        color: ORANGE);
                }
            }

            while (PropQueue.Count > 0) {
                var item = PropQueue.Dequeue();
                PropTool.RenderOverlay(
                    cameraInfo, item.Prop, item.Pos, item.Scale, item.Angle, Color.yellow);
            }
        }


    }
}
