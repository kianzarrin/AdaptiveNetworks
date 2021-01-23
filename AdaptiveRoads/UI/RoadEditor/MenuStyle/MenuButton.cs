using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class MenuButton : UIButton {
        public override void Awake() {
            base.Awake();
            width = 153;
            height = 47;
            textScale = 1.3f;
            //textPadding = new RectOffset(10, 10, 10, 10);

            horizontalAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;

            hoveredTextColor = new Color32(7, 132, 255, 255);
            disabledColor = new Color32(153, 153, 153, 255);
            disabledTextColor = new Color32(46, 46, 46, 255);
            atlas = TextureUtil.Ingame;
            normalBgSprite = "ButtonMenu";
        }

#if DEBUG
        protected override void OnClick(UIMouseEventParameter p) {
            base.OnClick(p);
            Log.Debug($"{this} was clicked: text:{text} parent:{parent.ToSTR()}");
        }
#endif

        public override void Start() {
            base.Start();
            absolutePosition = absolutePosition; // work-around for the position of the apply button.
        }        

    }
}
