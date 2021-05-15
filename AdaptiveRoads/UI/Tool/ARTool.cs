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

    public class ARTool : KianToolBase {
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

        public NetInfo HoveredNetInfo {
            get {
                if (!HoverValid)
                    return null;
                else if (NodeMode)
                    return HoveredNodeID.ToNode().Info;
                else
                    return HoveredSegmentID.ToSegment().Info;
            }
        }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();
            if (Hoverable())
                ToolCursor = ToolsModifierControl.toolController.Tools.OfType<NetTool>().FirstOrDefault()?.m_upgradeCursor;
            else
                ToolCursor = null;
        }

        public bool Hoverable() {
            var net = HoveredNetInfo?.GetMetaData();
            if (net == null)
                return false;

            if (NodeMode) {
                return net.UsedCustomFlags.Node != 0;
            } else if (SegmentMode) {
                return net.UsedCustomFlags.Segment != 0 || net.UsedCustomFlags.Lane != 0;
            } else if (SegmentEndMode) {
                return net.UsedCustomFlags.SegmentEnd != 0;
            }

            throw new Exception("Unreachable code");
        }

        public static void LogModes() {
            Log.Info($"={SegmentMode} NodeMode={NodeMode} SegmentEndMode={SegmentEndMode}\n" +
                $"SegmentMode=!NodeMode && !SegmentEndMode={!NodeMode} && {!SegmentEndMode}={!NodeMode && !SegmentEndMode}={SegmentMode} ");
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

        public static void Create() {
            ToolsModifierControl.toolController.gameObject.AddComponent<ARTool>();
        }

        public static void Release() {
            DestroyImmediate(ToolsModifierControl.toolController?.GetComponent<ARTool>());
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
