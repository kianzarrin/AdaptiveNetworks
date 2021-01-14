using ColossalFramework.UI;
using KianCommons.UI;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SummaryLabel : UILabel {
        public override void Awake() {
            base.Awake();
            wordWrap = true;
            autoSize = false;
            color = Color.black;
            textColor = Color.white;
            padding = new RectOffset(5, 5, 5, 5);
            atlas = TextureUtil.Ingame;
        }
    }
}
