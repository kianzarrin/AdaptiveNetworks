namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public class EditorArrayRemoveButton : UIButton {
        public override void Awake() {
            base.Awake();
            name = "EditorArrayDeleteButton";
            tooltip = "Delete";
            
            horizontalAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;

            atlas = TextureUtil.GetAtlas("InMapEditor");
            size = new Vector2(29, 29);
            normalFgSprite = "buttonclose";
            hoveredFgSprite = "buttonclosehover";
            pressedFgSprite = "buttonclosepressed";
        }
        public override void Start() {
            base.Start();
            relativePosition = new Vector3(343, -2);
        }
        protected override void OnVisibilityChanged() {
            base.OnVisibilityChanged();
            if (isVisible) {
                relativePosition = new Vector3(343, -2);
            }
        }
    }
}
