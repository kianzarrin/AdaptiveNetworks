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
    public class TextFieldByte_8 : UITextField {
        public override string ToString() => GetType().Name + $"({name})";

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }
        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            size = new Vector2(370, 20);
            horizontalAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;
            selectionSprite = "EmptySprite";
            normalBgSprite = "TextFieldPanel";
            selectionBackgroundColor = new Color32(0, 105, 210, 255);
            text = "0";
            isVisible = true;

            builtinKeyNavigation = true;
            readOnly = false;
            isInteractive = true;
            submitOnFocusLost = true;
            selectOnFocus = true;
            numericalOnly = true;
            allowFloats = false;
            allowNegative = false;

        }

        public bool TryGetValue(out byte value) {
            if (text == "") {
                value = 0;
                return true;
            } else if(byte.TryParse(text, out value)) {
                value = Math.Min((byte)8, value);
                return true;
            } else {
                return false;
            }
            
        }

        public virtual byte Value {
            set => text = value.ToString();
            get {
                if (TryGetValue(out byte ret)) {
                    return ret;
                } else {
                    return 0;
                }
            }
        }

        protected override void OnSubmit() {
            base.OnSubmit();
            Value = Value;
        }
    }
}
