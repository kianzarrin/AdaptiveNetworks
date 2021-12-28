namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System.Linq;
    using UnityEngine;
    using AdaptiveRoads.Manager;

    public class LanePanelCollapsable : UIPanel, IFittable {
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

        public static LanePanelCollapsable Add(UIComponent parent, LaneData lane, LaneData[] lanes, NetLaneExt.Flags mask) {
            Log.Called();
            Assertion.AssertNotNull(parent, "parent");
            var ret = parent.AddUIComponent<LanePanelCollapsable>();
            Assertion.AssertNotNull(ret, "laneContainer");

            var caption = LaneCaptionButton.Add(ret, lane);

            var innerPanel = AddPanel(ret);
            caption.SetTarget(innerPanel);
            innerPanel.name = "lanePanel";
            innerPanel.eventSizeChanged += InnerPanel_eventSizeChanged;

            var laneIDs = lanes?.Select(item => item.LaneID).ToArray();
            foreach(var flag in mask.ExtractPow2Flags()) {
                LaneFlagToggle.Add(innerPanel, lane.LaneID, laneIDs, flag);
            }

            return ret;
        }

        private static void InnerPanel_eventSizeChanged(UIComponent innerPanel, Vector2 _) {
            Log.Called();
            (innerPanel.parent as IFittable)?.FitRoot();
        }

        void IFittable.Fit2Children() => this.FitChildrenHorizontally();
        
        void IFittable.Fit2Parent() {
            autoFitChildrenHorizontally = false;
            autoSize = false;
            width = parent.width - (parent as UIPanel).padding.horizontal;
        }
    }
}
