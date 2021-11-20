namespace AdaptiveRoads.NSInterface.UI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;
    using AdaptiveRoads.Data.NetworkExtensions;

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

        public static LanePanelCollapsable Add(UIComponent parent, int laneIndex, NetLaneExt.Flags mask) {
            Log.Called();
            Assertion.AssertNotNull(parent, "parent");
            var ret = parent.AddUIComponent<LanePanelCollapsable>();
            Assertion.AssertNotNull(ret, "laneContainer");

            var caption = LaneCaptionButton.Add(ret, laneIndex);

            var innerPanel = AddPanel(ret);
            caption.SetTarget(innerPanel);
            innerPanel.name = "lanePanel";

            foreach (var flag in mask.ExtractPow2Flags()) {
                LaneFlagToggle.Add(innerPanel, laneIndex , flag);
            }

            return ret;
        }

        public void Shrink() {
            var caption = GetComponentInChildren<LaneCaptionButton>();
            autoSize = caption.autoSize = true;
        }

        public void FitParent() {
            autoFitChildrenHorizontally = false;
            autoSize = false;
            var p = parent as UIPanel;

            var caption = GetComponentInChildren<LaneCaptionButton>();
            caption.autoSize = false;
            width = p.width - p.padding.horizontal - p.autoLayoutPadding.horizontal;
            caption.width = width;
        }
    }
}
