namespace AdaptiveRoads.UI.VBSTool {
    extern alias UnifedUILib;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.Tool;
    using System;
    using UnifedUILib::UnifiedUI.Helpers;
    using UnityEngine;


    internal class VBSTool : KianToolBase<VBSTool> {
        private VSPanel panel_;
        private UIComponent button_;
        public ushort SelectedSegmentID { get; private set; }

        protected override void Awake() {
            base.Awake();
            try {
                base.Awake();
                string iconPath = UUIHelpers.GetFullPath<LifeCycle.UserMod>("uui_vbs.png");
                button_ = UUIHelpers.RegisterToolButton(
                    name: "AdaptiveNetworks",
                    groupName: null, // default group
                    tooltip: "Adaptive Networks",
                    tool: this,
                    icon: UUIHelpers.LoadTexture(iconPath),
                    hotkeys: new UUIHotKeys { ActivationKey = ModSettings.Hotkey });
            } catch (Exception ex) {
                ex.Log();
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            button_?.Destroy();
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
        }

        public void Select(ushort segmentID) {
            try {
                SelectedSegmentID = segmentID;
                panel_?.Close();
                panel_ = null;
                if (segmentID != 0)
                    panel_ = VSPanel.Open(segmentID);
            } catch(Exception ex) { ex.Log(); }
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);
            if(SelectedSegmentID != 0)
                NetTool.RenderOverlay(cameraInfo, ref SelectedSegmentID.ToSegment(), Color.white, Color.white);
            if (HoveredSegmentID != 0 && HoveredSegmentID != SelectedSegmentID)
                NetTool.RenderOverlay(cameraInfo, ref HoveredSegmentID.ToSegment(), Color.blue, Color.blue);
        }

        protected override void OnPrimaryMouseClicked() {
            if(HoveredSegmentID != 0) {
                Select(HoveredSegmentID);
            }
        }

        protected override void OnSecondaryMouseClicked() {
            if(SelectedSegmentID != 0) {
                Select(0);
            } else {
                this.enabled = false;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            Select(0);
        }

        protected override void OnDisable() {
            base.OnDisable();
            Select(0);
        }
    }
}
