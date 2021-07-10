namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.Assertion;

    public class LanePanelCollapsable : UIPanel {
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            atlas = TextureUtil.Ingame;
            backgroundSprite = "MenuPanelInfo";
        }

        static UIPanel AddPanel(UIPanel parent) {
            Assertion.AssertNotNull(parent, "parent");
            int padX = 0;
            int padY = 3;
            UIPanel newPanel = parent.AddUIComponent<UIPanel>();
            Assertion.AssertNotNull(newPanel, "newPanel");
            newPanel.autoLayout = true;
            newPanel.autoLayoutDirection = LayoutDirection.Vertical;
            newPanel.autoFitChildrenHorizontally = true;
            newPanel.autoFitChildrenVertically = true;
            newPanel.autoLayoutPadding = new RectOffset(padX, padX, padY, padY);
            newPanel.padding = new RectOffset(padX, padX, padY, padY);

            return newPanel;
        }

        public static LanePanelCollapsable Add(UIComponent parent, LaneData lane, NetLaneExt.Flags mask) {
            Log.Called();
            Assertion.AssertNotNull(parent, "parent");
            var laneContainer = parent.AddUIComponent<LanePanelCollapsable>();
            Assertion.AssertNotNull(laneContainer, "laneContainer");

            var caption = LaneCaptionButton.Add(laneContainer, lane);

            var lanePanel = AddPanel(laneContainer);
            caption.SetTarget(lanePanel);
            lanePanel.name = "lanePanel";

            foreach (var flag in mask.ExtractPow2Flags()) {
                LaneFlagToggle.Add(lanePanel, lane.LaneID, flag);
            }

            return laneContainer;
        }

        public void FitParent() {
            autoFitChildrenHorizontally = false;
            this.FitTo(parent, LayoutDirection.Horizontal);
            GetComponentInChildren<LaneCaptionButton>().FitTo(this, LayoutDirection.Horizontal);
        }
    }
}
