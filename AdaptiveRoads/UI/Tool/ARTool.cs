namespace AdaptiveRoads.UI.Tool {
    extern alias UnifedUILib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.Tool;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using KianCommons.IImplict;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnifedUILib::UnifiedUI.Helpers;

    public class ARTool : KianToolBase, IStartingObject {
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
            throw new NotImplementedException();
        }

        protected override void OnSecondaryMouseClicked() {
            throw new NotImplementedException();
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

        public bool Hoverable() {
            if (!HoverValid)
                return false;

            if (NodeMode) {
                return
                    HoveredNetInfo?.GetMetaData() is var netMetaData
                    && netMetaData.UsedCustomFlags.Node != 0;
            } else if (SegmentMode) {
                return
                    HoveredNetInfo?.GetMetaData() is var netMetaData &&
                    netMetaData.UsedCustomFlags.Segment != 0 &&
                    netMetaData.UsedCustomFlags.Lane != 0; 
            } else if (SegmentEndMode) {
                return
                    HoveredNetInfo?.GetMetaData() is var netMetaData &&
                    netMetaData.UsedCustomFlags.Segment != 0 &&
                    netMetaData.UsedCustomFlags.Lane != 0;
            }
            return false;
        }


        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);

            if (SelectedSegmentID != 0 && SelectedNodeID != 0)
                RenderUtil.DrawCutSegmentEnd(cameraInfo, SelectedSegmentID,0.5f, SelectedStartNode, Color.white, true);
            else if (SelectedNodeID != 0)
                RenderUtil.DrawNodeCircle(cameraInfo, Color.white, SelectedNodeID, true);
            else if (SelectedSegmentID != 0)
                RenderUtil.RenderSegmnetOverlay(cameraInfo, SelectedSegmentID, Color.white, true);

            if (!Hoverable())
                return;

            Color color;
            if (Input.GetMouseButton(0))
                color = GetToolColor(true, false);
            else
                color = GetToolColor(false, false);

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
            DestroyImmediate(ToolsModifierControl.toolController.gameObject.AddComponent<ARTool>()?.gameObject);
        }

        public void Start() {
            try {
                string sprites = UUIHelpers.GetFullPath<LifeCycle.UserMod>("Resources", "B.png");
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
