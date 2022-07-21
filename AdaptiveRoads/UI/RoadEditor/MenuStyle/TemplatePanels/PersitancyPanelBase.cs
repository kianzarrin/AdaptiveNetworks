using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons.UI;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class PersitancyPanelBase : MenuPanelBase {
        public const int WIDTH_LEFT = 425;
        public const int WIDTH_RIGHT = 475;

        public override void Awake() {
            base.Awake();
            width = WIDTH_LEFT + WIDTH_RIGHT + PAD * 3;
        }

        public UIPanel AddLeftPanel() {
            var panel = AddSubPanel();
            panel.relativePosition = new Vector2(PAD, TITLE_HEIGHT);
            panel.width = WIDTH_LEFT;
            return panel;
        }

        public UIPanel AddRightPanel() {
            var panel = AddSubPanel();
            panel.relativePosition = new Vector2(WIDTH_LEFT + PAD * 2, TITLE_HEIGHT);
            panel.width = WIDTH_RIGHT;
            return panel;
        }
    }
}
