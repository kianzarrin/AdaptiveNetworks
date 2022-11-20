namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using ColossalFramework;
    using KianCommons;
    using System;
    using UnityEngine;
    using KianCommons.UI;

    /// <summary>
    /// byte field between 0 to 8
    /// </summary
    public class EditorCheckbox : UICheckBox {
        public override string ToString() => GetType().Name + $"({name})";

        public override void Awake() {
            base.Awake();
            name = nameof(EditorCheckbox);
            size = new Vector2(370, 20);

            UISprite sprite = AddUIComponent<UISprite>();
            sprite.spriteName = "check-unchecked";
            sprite.size = new Vector2(16, 16);
            sprite.relativePosition = new Vector2(346, (height - sprite.height) * 0.5f);
            sprite.atlas = TextureUtil.Ingame;

            var sprite2 = sprite.AddUIComponent<UISprite>();
            sprite2.atlas = TextureUtil.Ingame;
            sprite2.spriteName = "check-checked";
            checkedBoxObject = sprite2;
            checkedBoxObject.size = sprite.size;
            checkedBoxObject.relativePosition = Vector3.zero;

            label = AddUIComponent<UILabel>();
            label.text = GetType().Name;
            label.textScale = 1;
            label.relativePosition = new Vector2(0  , (height - label.height) * 0.5f + 1);
        }
        public virtual string Label {
            get => label.text;
            set {
                label.text = value;
                Invalidate();
            }
        }
    }
}
