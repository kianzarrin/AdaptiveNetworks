using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons.UI;
using ColossalFramework;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor.Templates {
    public class RenameRoadPanel : MenuPanelBase {
        public static RenameRoadPanel Display(SaveAssetPanel panel) {
            var ret = panel.component.AddUIComponent<RenameRoadPanel>();
            ret.BringToFront();
            return ret;
        }

        public override void Awake() {
            base.Awake();

            var panel = AddUIComponent<UIPanel>();
            panel.autoFitChildrenHorizontally = true;
            panel.autoFitChildrenVertically = true;
            panel.autoLayout = true;
            panel.autoLayoutPadding = new RectOffset(0, 0, 0, 10);
            panel.autoLayoutDirection = LayoutDirection.Vertical;

            var name = panel.AddUIComponent<MenuTextField>();

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
                    RoadUtils.RenameEditNet(name.text);
                    Destroy(this.gameObject);
                };
                name.eventTextChanged += (_, __) =>
                    apply.isEnabled = !name.text.IsNullOrWhiteSpace();
                apply.isEnabled = !name.text.IsNullOrWhiteSpace();

                var cancel = PanelBottom.AddUIComponent<MenuButton>();
                cancel.text = "Cencel";
                cancel.eventClick += (_, __) => Destroy(this.gameObject);
            }

            FitChildrenVertically(PAD);
            FitChildrenHorizontally(PAD);
            name.width = panel.width;
            AddDrag("Rename Road");
        }
    }
}
