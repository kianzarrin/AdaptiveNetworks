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
    using System.Collections.Generic;

    internal class ARTool : KianToolBase<ARTool> {
        NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        private ushort[] selectedSegmentIDs_;
        public ushort[] SelectedSegmentIDs {
            get => selectedSegmentIDs_ ?? new ushort[0];
            set => selectedSegmentIDs_ = value;
        }
        public ushort SelectedSegmentID;
        public ushort SelectedNodeID;
        public bool SelectedStartNode => SelectedSegmentID.ToSegment().IsStartNode(SelectedNodeID);
        public ushort[] GetHoveredSegmentIDs() => Util.GetSimilarSegmentsBetweenJunctions(HoveredSegmentID).ToArray();


        public static bool NodeMode => Helpers.ControlIsPressed;
        public static bool SegmentEndMode => Helpers.AltIsPressed;

        public static bool SegmentMode => !NodeMode && !SegmentEndMode;

        public static bool MultiSelectMode => Helpers.ShiftIsPressed;

        UIComponent button_;
        FlagsPanel flagsPanel_;

        void OpenPanel() {
            ClosePanel();
            flagsPanel_ = FlagsPanel.Open(SelectedNodeID, SelectedSegmentID, SelectedSegmentIDs);
        }

        void ClosePanel() {
            flagsPanel_?.Close();
            flagsPanel_ = null;
        }

        protected override void OnPrimaryMouseClicked() {
            LogCalled();
            if (!HoverHasFlags())
                return;
            if (SegmentMode) {
                SelectedSegmentID = HoveredSegmentID;
                SelectedNodeID = 0;
                if(MultiSelectMode) {
                    SelectedSegmentIDs = GetHoveredSegmentIDs();
                } else {
                    SelectedSegmentIDs = null;
                }
            } else if (NodeMode) {
                SelectedNodeID = HoveredNodeID;
                SelectedSegmentID = 0;
                SelectedSegmentIDs = null;
            } else if (SegmentEndMode) {
                SelectedSegmentID = HoveredSegmentID;
                SelectedNodeID = HoveredNodeID;
                SelectedSegmentIDs = null;
            }
            OpenPanel();
        }

        protected override void OnSecondaryMouseClicked() {
            LogCalled();
            if(SelectedSegmentID != 0 || SelectedNodeID != 0) {
                SelectedSegmentIDs = null;
                SelectedSegmentID = 0;
                SelectedNodeID = 0;
                ClosePanel();
            } else {
                enabled = false;
            }
        }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();

            if (HoverHasFlags()) {
                ToolCursor = ToolsModifierControl.toolController.Tools.OfType<NetTool>().FirstOrDefault()?.m_upgradeCursor;
            } else {
                ToolCursor = null;
            }

            if (HoverValid) {
                var hints = new List<string>();
                var usedCustomFlags = GetUsedFlagsSegment(HoveredSegmentID);
                if(usedCustomFlags.Segment != 0 || usedCustomFlags.Lane != 0) {
                    hints.Add("Click => modify custom segment/lane flags");
                    hints.Add("Shift + Click => Select all segments between two junctions.");
                }
                if(GetUsedFlagsNode(HoveredNodeID) != 0)
                    hints.Add("CTRL + Click => modify node flags");
                if (GetUsedFlagsSegmentEnd(segmentID: HoveredSegmentID, nodeID: HoveredNodeID) != 0)
                    hints.Add("CTRL + Click => modify segmentEnd flags");
                if(hints.Count == 0)
                    hints.Add("no custom AR flags to modify");
                ShowToolInfo(true, hints.JoinLines(), HitPos);
            } else {
                ShowToolInfo(false, "", default);
            }
        }

        public static NetNodeExt.Flags GetUsedFlagsNode(ushort nodeID) {
            NetNodeExt.Flags ret = nodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags.Node ?? 0;
            foreach(ushort segmentID in nodeID.ToNode().IterateSegments()) {
                ret |= segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags.Node ?? 0;
            }
            return ret;
        }

        public static CustomFlags GetUsedFlagsSegment(ushort segmentID) {
            CustomFlags ret = segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None;

            // Considering that nodes are segment ends we don't need to take their flags into account.
            //ushort startNodeID = segmentID.ToSegment().m_startNode;
            //ushort endNodeID = segmentID.ToSegment().m_endNode;
            //ret |= startNodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None;
            //ret |= endNodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None;
            return ret;
        }

        public static NetSegmentEnd.Flags GetUsedFlagsSegmentEnd(ushort segmentID, ushort nodeID) {
            NetSegmentEnd.Flags ret = segmentID.ToSegment().Info.GetMetaData()?.UsedCustomFlags.SegmentEnd ?? 0;

            // Considering that nodes are segment ends we don't need to take their flags into account.
            // ret |= nodeID.ToNode().Info.GetMetaData()?.UsedCustomFlags.SegmentEnd ?? 0;
            return ret;
        }

        public bool HoverHasFlags() {
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

            if(SelectedSegmentID != 0 && SelectedNodeID != 0)
                HighlightSegmentEnd(cameraInfo, SelectedSegmentID, SelectedNodeID, Color.white);
            else if(SelectedNodeID != 0)
                RenderUtil.DrawNodeCircle(cameraInfo, Color.white, SelectedNodeID, true);
            else if(SelectedSegmentID != 0) {
                RenderUtil.RenderSegmnetOverlay(cameraInfo, SelectedSegmentID, Color.white, true);
                foreach(var segmentID in SelectedSegmentIDs)
                    RenderUtil.RenderSegmnetOverlay(cameraInfo, segmentID, Color.white, true);
            }


            if(flagsPanel_ && flagsPanel_.HighlighLaneID != 0) {
                var laneData = new LaneData(flagsPanel_.HighlighLaneID);
                RenderUtil.RenderLaneOverlay(cameraInfo, laneData, Color.yellow, false);
                foreach(var laneData2 in Util.GetSimilarLanes(laneData, selectedSegmentIDs_)) {
                    RenderUtil.RenderLaneOverlay(cameraInfo, laneData, Color.yellow, false);
                }
            }

            if (!HoverValid)
                return;

            Color color;
            if (Input.GetMouseButton(0))
                color = GetToolColor(true, false);
            else if (HoverHasFlags())
                color = GetToolColor(false, false);
            else
                color = GetToolColor(false, true);

            if (SegmentMode) {
                RenderUtil.RenderSegmnetOverlay(cameraInfo, HoveredSegmentID, color, true);
                if(MultiSelectMode) {
                    foreach(var segmentID in GetHoveredSegmentIDs())
                        RenderUtil.RenderSegmnetOverlay(cameraInfo, segmentID, color, true);
                }
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
                string sprites = UUIHelpers.GetFullPath<LifeCycle.UserMod>("uui_ar.png");
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
