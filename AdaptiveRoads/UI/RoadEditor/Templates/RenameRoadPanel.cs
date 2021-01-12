using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using ColossalFramework;
using UnityEngine;


namespace AdaptiveRoads.UI.RoadEditor.Templates {
    public class RenameRoadPanel : MenuPanelBase {
        public static RenameRoadPanel Display(SaveAssetPanel panel) {
            var ret = panel.component.AddUIComponent<RenameRoadPanel>();
            ret.BringToFront();
            return ret;
        }

        const float WIDTH = 350;
        public override void Awake() {
            base.Awake();
            width = WIDTH + PAD*2;

            var panel = AddUIComponent<UIPanel>();
            panel.width = WIDTH;
            panel.autoFitChildrenVertically = true;
            panel.autoLayout = true;
            panel.autoLayoutPadding = new RectOffset(0, 0, 0, 10);
            panel.autoLayoutDirection = LayoutDirection.Vertical;


            var name = panel.AddUIComponent<MenuTextField>();
            name.width = WIDTH;

            var summary = panel.AddUIComponent<SummaryLabel>();
            summary.autoSize = true;
            summary.wordWrap = false;
            summary.minimumSize = Vector2.zero;
            summary.maximumSize = new Vector2(1000,1000);

            name.eventTextChanged += (_, __) => RefreshSummary();
            RefreshSummary();
            void RefreshSummary() {
                if (name.text.IsNullOrWhiteSpace())
                    summary.text = RoadUtils.GatherEditNames().JoinLines();
                else
                    summary.text = RoadUtils.RenameEditNet(name.text, true).JoinLines();
            }


            var PanelBottom = panel.AddUIComponent<UIPanel>();
            {
                PanelBottom.autoFitChildrenHorizontally = true;
                PanelBottom.autoFitChildrenVertically = true;
                PanelBottom.autoLayout = true;
                PanelBottom.autoLayoutPadding = new RectOffset(0, 10, 0, 0);
                PanelBottom.autoLayoutDirection = LayoutDirection.Horizontal;
                panel.relativePosition = new Vector2(PAD, TITLE_HEIGHT);

                var apply = PanelBottom.AddUIComponent<MenuButton>();
                apply.text = "Apply";
                apply.eventClick += (_, __) => {
                    RoadUtils.RenameEditNet(name.text, false);
                    Destroy(this.gameObject);
                };
                void RefreshAppy() {
                    apply.isEnabled = !name.text.IsNullOrWhiteSpace();
                }

                name.eventTextChanged += (_, __) => RefreshAppy();
                RefreshAppy();

                var close = PanelBottom.AddUIComponent<MenuButton>();
                close.text = "Close";
                close.eventClick += (_, __) => Destroy(this.gameObject);
            }

            FitChildrenVertically(PAD);
            AddDrag("Rename Road");

            verticalSpacing = PAD;
        }

        protected override void OnVisibilityChanged() {
            base.OnVisibilityChanged();
            if (!isVisible)
                Destroy(this.gameObject);
        }
    }
}
