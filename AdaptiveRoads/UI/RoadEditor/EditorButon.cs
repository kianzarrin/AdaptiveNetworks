using ColossalFramework.UI;
using KianCommons.UI;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor {
    public class EditorButon : UIButton {
        public override void Awake() {
            base.Awake();
            name = GetType().FullName;
            canFocus = false;
            size = new Vector2(363, 30);
            textPadding = new RectOffset(1, 0, 0, 0);
            textHorizontalAlignment = UIHorizontalAlignment.Center;

            atlas = TextureUtil.InMapEditor;
            normalBgSprite = "SubBarButtonBase";
            hoveredBgSprite = "SubBarButtonBaseHovered";
            pressedBgSprite = "SubBarButtonBasePressed";
            disabledBgSprite = "SubBarButtonBaseDisabled";
        }
    }
}
