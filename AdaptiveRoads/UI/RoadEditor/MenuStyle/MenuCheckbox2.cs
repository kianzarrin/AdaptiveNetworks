using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class MenuCheckbox2 : UICheckBox {
        UISprite UnCheckedSprite, CheckedSprite;

        public override void Awake() {
            base.Awake();
            name = nameof(UICheckBoxExt);
            height = 16;

            //clipChildren = true;

            UnCheckedSprite = AddUIComponent<UISprite>();
            UnCheckedSprite.spriteName = "check-unchecked";
            UnCheckedSprite.size = new Vector2(height, height);
            UnCheckedSprite.relativePosition = new Vector2(0, (height - UnCheckedSprite.height) / 2);
            UnCheckedSprite.atlas = TextureUtil.Ingame;


            CheckedSprite = UnCheckedSprite.AddUIComponent<UISprite>();
            CheckedSprite.atlas = TextureUtil.Ingame;
            CheckedSprite.spriteName = "check-checked";
            checkedBoxObject = CheckedSprite;
            checkedBoxObject.size = UnCheckedSprite.size;
            checkedBoxObject.relativePosition = Vector3.zero;

            label = AddUIComponent<UILabel>();
            label.text = GetType().Name;
            label.textScale = 0.9f;
            label.relativePosition = new Vector2(0, 0);

            eventCheckChanged += OnCheckChanged;
            label.eventTextChanged += Label_eventTextChanged;
            Label_eventTextChanged(null, null);
        }

        private void Label_eventTextChanged(UIComponent _, string __) {
            UnCheckedSprite.relativePosition = new Vector2(
                    label.width + 5,
                    (height - label.height) / 2 + 1);
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
