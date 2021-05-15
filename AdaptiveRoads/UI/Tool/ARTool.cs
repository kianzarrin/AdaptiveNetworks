namespace AdaptiveRoads.UI.Tool {
    extern alias UnifedUILib;
    using System;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.Tool;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using KianCommons.IImplict;
    using ColossalFramework.UI;
    using UnifedUILib::UnifiedUI.Helpers;
    using ColossalFramework;
    using System.Linq;

    public class ARTool : KianToolBase {
        NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        public ushort SelectedSegmentID;
        public ushort SelectedNodeID;
        public bool SelectedStartNode => SelectedSegmentID.ToSegment().IsStartNode(SelectedNodeID);

        public static bool NodeMode => Helpers.ControlIsPressed;
        public static bool SegmentEndMode => Helpers.AltIsPressed;

        public static bool SegmentMode = !NodeMode && !SegmentEndMode;

        public static bool MultiSelect = Helpers.ShiftIsPressed;

        UIComponent button_;

        protected override void OnPrimaryMouseClicked() {
        }

        protected override void OnSecondaryMouseClicked() {
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
            ToolCursor = ToolsModifierControl.toolController.Tools.OfType<NetTool>().FirstOrDefault()?.m_upgradeCursor;
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


        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);

            if (SelectedSegmentID != 0 && SelectedNodeID != 0)
                RenderUtil.DrawCutSegmentEnd(cameraInfo, SelectedSegmentID, 0.5f, SelectedStartNode, Color.white, true);
            else if (SelectedNodeID != 0)
                RenderUtil.DrawNodeCircle(cameraInfo, Color.white, SelectedNodeID, true);
            else if (SelectedSegmentID != 0)
                RenderUtil.RenderSegmnetOverlay(cameraInfo, SelectedSegmentID, Color.white, true);

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
                RenderUtil.DrawNodeCircle(cameraInfo, Color.white, HoveredNodeID, true);
            } else if (SegmentEndMode) {
                RenderUtil.DrawCutSegmentEnd(cameraInfo, SelectedSegmentID, 0.5f, SelectedStartNode, color, true);
            }
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
            button_ = null;
        }


    }
}
