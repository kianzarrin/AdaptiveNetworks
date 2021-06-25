namespace AdaptiveRoads.UI.Tool {
    extern alias UnifedUILib;
    using System;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.Tool;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using ColossalFramework.UI;
    using UnifedUILib::UnifiedUI.Helpers;
    using System.Linq;
    using static KianCommons.ReflectionHelpers;
    using Patches.AsymPavements;

    public class ARTool : KianToolBase<ARTool> {
        // for mod tools:
        RefreshJunctionDataPatch.Util.Operation [,] Operations => RefreshJunctionDataPatch.Util.Operations;
        NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        public ushort SelectedSegmentID;
        public ushort SelectedNodeID;
        public bool SelectedStartNode => SelectedSegmentID.ToSegment().IsStartNode(SelectedNodeID);

        public static bool NodeMode => Helpers.ControlIsPressed;
        public static bool SegmentEndMode => Helpers.AltIsPressed;

        public static bool SegmentMode => !NodeMode && !SegmentEndMode;

        public static bool MultiSelect => Helpers.ShiftIsPressed;

        UIComponent button_;
        FlagsPanel flagsPanel_;

        void OpenPanel() {
            ClosePanel();
            flagsPanel_ = FlagsPanel.Open(SelectedSegmentID, SelectedNodeID);
        }

        void ClosePanel() {
            flagsPanel_?.Close();
            flagsPanel_ = null;
        }

        protected override void OnPrimaryMouseClicked() {
            LogCalled();
            if (!Hoverable())
                return;
            if (SegmentMode) {
                SelectedSegmentID = HoveredSegmentID;
                SelectedNodeID = 0;
            } else if (NodeMode) {
                SelectedNodeID = HoveredNodeID;
                SelectedSegmentID = 0;
            } else if (SegmentEndMode) {
                SelectedSegmentID = HoveredSegmentID;
                SelectedNodeID = HoveredNodeID;
            }
            OpenPanel();
        }

        protected override void OnSecondaryMouseClicked() {
            LogCalled();
            if(SelectedSegmentID != 0 || SelectedNodeID != 0) {
                SelectedSegmentID = 0;
                SelectedNodeID = 0;
                ClosePanel();
            } else {
                enabled = false;
            }
        }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();
            if (Hoverable())
                ToolCursor = ToolsModifierControl.toolController.Tools.OfType<NetTool>().FirstOrDefault()?.m_upgradeCursor;
            else
                ToolCursor = null;
        }

        public static NetNodeExt.Flags GetUsedFlagsNode(ushort nodeID) {
            NetNodeExt.Flags ret = nodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags.Node ?? 0;
            foreach(ushort segmentID in nodeID.ToNode().IterateSegments()) {
                ret |= segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags.Node ?? 0;
            }
            return ret;
        }

        public static CustomFlags GetUsedFlagsSegment(ushort segmentID) {
            CustomFlags ret = segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags ?? new CustomFlags();
            ushort startNodeID = segmentID.ToSegment().m_startNode;
            ushort endNodeID = segmentID.ToSegment().m_endNode;
            ret |= startNodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags ?? new CustomFlags();
            ret |= endNodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags ?? new CustomFlags();
            return ret;
        }

        public static NetSegmentEnd.Flags GetUsedFlagsSegmentEnd(ushort segmentID, ushort nodeID) {
            NetSegmentEnd.Flags ret = segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags.SegmentEnd ?? 0;
            ret |= nodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags.SegmentEnd ?? 0;
            return ret;
        }
        public static NetLaneExt.Flags GetUsedCustomFlagsLane(LaneData lane) {
            NetLaneExt.Flags mask = 0;
            var props = (lane.LaneInfo.m_laneProps?.m_props).EmptyIfNull();
            foreach (var prop in props) {
                var metadata = prop.GetMetaData();
                if (metadata != null)
                    mask |= (metadata.LaneFlags.Required | metadata.LaneFlags.Forbidden);
            }
            return mask & NetLaneExt.Flags.CustomsMask;
        }

        public bool Hoverable() {
            if (NodeMode) {
                return GetUsedFlagsNode(HoveredNodeID) != 0;
            } else if (SegmentMode) {
                var usedCustomFlags = GetUsedFlagsSegment(HoveredSegmentID);
                return usedCustomFlags.Segment != 0 || usedCustomFlags.Lane != 0;
            } else if (SegmentEndMode) {
                return GetUsedFlagsSegmentEnd(segmentID:HoveredSegmentID, nodeID:HoveredNodeID) != 0;
            }

            throw new Exception("Unreachable code");
        }

        public static void LogModes() {
            Log.Info($"SegmentMode={SegmentMode} NodeMode={NodeMode} SegmentEndMode={SegmentEndMode}");
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);

            if (SelectedSegmentID != 0 && SelectedNodeID != 0)
                HighlightSegmentEnd(cameraInfo, SelectedSegmentID, SelectedNodeID, Color.white);
            else if (SelectedNodeID != 0)
                RenderUtil.DrawNodeCircle(cameraInfo, Color.white, SelectedNodeID, true);
            else if (SelectedSegmentID != 0)
                RenderUtil.RenderSegmnetOverlay(cameraInfo, SelectedSegmentID, Color.white, true);

            if (flagsPanel_ && flagsPanel_.HighlighLaneID != 0) {
                var laneData = new LaneData(flagsPanel_.HighlighLaneID);
                RenderUtil.RenderLaneOverlay(cameraInfo, laneData, Color.yellow, false);
            }

            if (!HoverValid)
                return;

            Color color;
            if (Input.GetMouseButton(0))
                color = GetToolColor(true, false);
            else if (Hoverable())
                color = GetToolColor(false, false);
            else
                color = GetToolColor(false, true);

            if (SegmentMode) {
                RenderUtil.RenderSegmnetOverlay(cameraInfo, HoveredSegmentID, color, true);
            } else if (NodeMode) {
                RenderUtil.DrawNodeCircle(cameraInfo, color, HoveredNodeID, true);
            } else if (SegmentEndMode) {
                HighlightSegmentEnd(cameraInfo, HoveredSegmentID, HoveredNodeID, color);
            }
        }

        static void HighlightSegmentEnd(RenderManager.CameraInfo cameraInfo, ushort segmentID, ushort nodeID, Color color, bool alpha = false) {
            RenderUtil.DrawCutSegmentEnd(
                cameraInfo: cameraInfo,
                segmentId: segmentID,
                cut: 0.5f,
                bStartNode: segmentID.ToSegment().IsStartNode(nodeID),
                color: color,
                alpha: alpha);
        }

        protected override void Awake() {
            try {
                base.Awake();
                string sprites = UUIHelpers.GetFullPath<LifeCycle.UserMod>("B.png");
                Debug.Log("[UUIExampleMod] ExampleTool.Awake() sprites=" + sprites);
                button_ = UUIHelpers.RegisterToolButton(
                    name: nameof(ARTool),
                    groupName: null, // default group
                    tooltip: "Adaptive Roads",
                    spritefile: sprites,
                    tool: this,
                    activationKey: ModSettings.Hotkey);

            } catch (Exception ex) {
                ex.Log();
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            button_?.Destroy();
            flagsPanel_?.Close();
            SetAllDeclaredFieldsToNull(this);
        }

        protected override void OnDisable() {
            base.OnDisable();
            ClosePanel();
            SelectedNodeID = 0;
            SelectedSegmentID = 0;
        }


    }
}
