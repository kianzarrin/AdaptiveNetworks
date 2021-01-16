using ColossalFramework;
using ColossalFramework.Math;
using KianCommons;
using KianCommons.UI;
using System.Collections.Generic;
using UnityEngine;
using KianCommons.Math;

namespace AdaptiveRoads.UI.RoadEditor {
    internal static class Overlay {
        static Color ORANGE = new Color32(255, 165, 0, 255); //orange
        internal static object HoveredInfo; // NetInfo[.Node/Segment/Prop]

        internal struct PropData {
            internal PropInfo Prop;
            internal Vector3 Pos;
            internal float Angle;
            internal float Scale;
        }
        internal static Queue<PropData> PropQueue = new Queue<PropData>();

        internal struct TreeData {
            internal TreeInfo Tree;
            internal Vector3 Pos;
            internal float Scale;
        }
        internal static Queue<TreeData> TreeQueue = new Queue<TreeData>();

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
            internal bool IsDC;
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
                    RenderUtil.RenderLaneOverlay(cameraInfo, lane, Color.yellow);
                }
            }

            while (SegmentQueue.Count > 0) {
                var item = SegmentQueue.Dequeue();
                Color color = item.TurnAround ? ORANGE : Color.yellow;
                RenderUtil.RenderSegmnetOverlay(cameraInfo, item.SegmentID, color);
            }

            while (NodeQueue.Count > 0) {
                var item = NodeQueue.Dequeue();
                bool end = item.NodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.End);
                if (item.IsBendNode || end) {
                    RenderUtil.DrawNodeCircle(cameraInfo, Color.yellow, item.NodeID);
                } else if (item.IsDC) {
                    DrawDC(cameraInfo, item.SegmentID, item.SegmentID2, item.NodeID);
                } else {
                    HighlightSegmentEnd(cameraInfo, item.SegmentID, item.NodeID, Color.yellow);
                }

            }


            while (PropQueue.Count > 0) {
                var item = PropQueue.Dequeue();
                PropTool.RenderOverlay(
                    cameraInfo, item.Prop, item.Pos, item.Scale, item.Angle, Color.yellow);
            }
            while (TreeQueue.Count > 0) {
                var item = TreeQueue.Dequeue();
                TreeTool.RenderOverlay(cameraInfo, item.Tree, item.Pos, item.Scale, Color.yellow);
            }
        }

        static void HighlightSegmentEnd(RenderManager.CameraInfo cameraInfo, ushort segmentID, ushort nodeID, Color color) {
            RenderUtil.DrawCutSegmentEnd(
                cameraInfo: cameraInfo,
                segmentId: segmentID,
                cut: 0.5f,
                bStartNode: segmentID.ToSegment().IsStartNode(nodeID),
                color: color);
        }

        static void DrawDC(RenderManager.CameraInfo cameraInfo,
            ushort segmentID1, ushort segmentID2, ushort nodeID) {
            NetUtil.CalculateSegEndCenter(segmentID1, nodeID, out var pos1, out var dir1);
            NetUtil.CalculateSegEndCenter(segmentID2, nodeID, out var pos2, out var dir2);
            Bezier3 b = BezierUtil.Bezier3ByDir(pos1, -dir1, pos2, -dir2);
            b.RenderArrow(cameraInfo, Color.cyan, 1);
        }

    }
}
