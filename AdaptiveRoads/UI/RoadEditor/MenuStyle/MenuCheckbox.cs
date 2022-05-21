using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using System.Reflection;
using KianCommons.UI.Helpers;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class MenuCheckbox: UICheckBox {
        public override void Awake() {
            base.Awake();
            name = nameof(UICheckBoxExt);
            height = 30;
            width = 100;

            //clipChildren = true;

            UISprite sprite = AddUIComponent<UISprite>();
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(height, height);
            sprite.relativePosition = new Vector2(0, (height - sprite.height) / 2);
            sprite.atlas = TextureUtil.Ingame;


            var sprite2 = sprite.AddUIComponent<UISprite>();
            sprite2.atlas = TextureUtil.Ingame;
            sprite2.spriteName = "ToggleBaseFocused";
            checkedBoxObject = sprite2;
            checkedBoxObject.size = sprite.size;
            checkedBoxObject.relativePosition = Vector3.zero;

            label = AddUIComponent<UILabel>();
            label.text = GetType().Name;
            label.textScale = 0.9f;
            label.relativePosition = new Vector2(
                sprite.width + 5f,
                (height - label.height) / 2 + 1);

            eventCheckChanged += OnCheckChanged;
        }

        public virtual void OnCheckChanged(UIComponent component, bool value) {
            Invalidate();
#if DEBUG
            Log.Debug($"{this} check changed to {value}: label:{Label} parent:{parent.ToSTR()}");
#endif
        }

        public virtual string Label {
            get => label.text;
            set {
                label.text = value;
                Invalidate();
            }
        }

        public virtual string Tooltip {
            get => tooltip;
            set {
                tooltip = value;
                RefreshTooltip();
            }
        }
    }
}
