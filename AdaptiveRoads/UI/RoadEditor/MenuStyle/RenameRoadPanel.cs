using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using ColossalFramework;
using UnityEngine;


namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class RenameRoadPanel : MenuPanelBase {
        const float WIDTH = 350;

        public static RenameRoadPanel Display(SaveAssetPanel panel) {
            var ret = panel.component.AddUIComponent<RenameRoadPanel>();
            ret.BringToFront();
            return ret;
        }

        public override void Awake() {
            base.Awake();
            width = WIDTH + PAD*2;

            var panel = AddSubPanel();
            panel.width = WIDTH;


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



            var PanelBottom = AddBottomPanel(panel);
            {
                var apply = PanelBottom.AddUIComponent<MenuButton>();
                apply.text = "Apply";
                apply.eventClick += (_, __) => {
                    RoadUtils.RenameEditNet(name.text, false);
                    Destroy(this.gameObject);
                };
                void RefreshAppy() => apply.isEnabled = !name.text.IsNullOrWhiteSpace();
                name.eventTextChanged += (_, __) => RefreshAppy();
                RefreshAppy();

                var close = PanelBottom.AddUIComponent<MenuButton>();
                close.text = "Close";
                close.eventClick += (_, __) => Destroy(this.gameObject);
            }

            AddDrag("Rename Road");
            verticalSpacing = PAD;
            FitChildrenVertically(PAD);
        }

        protected override void OnVisibilityChanged() {
            base.OnVisibilityChanged();
            if (!isVisible)
                Destroy(this.gameObject);
        }
    }
}
