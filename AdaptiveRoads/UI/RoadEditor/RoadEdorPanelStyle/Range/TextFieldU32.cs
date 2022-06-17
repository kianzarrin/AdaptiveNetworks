namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;

    public class TextFieldU32 : UITextField {
        //UIResetButton resetButton_;

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

        private string _postfix = "";
        public string PostFix {
            get => _postfix;
            set {
                if (value.IsNullOrWhiteSpace())
                    _postfix = "";
                else
                    _postfix = value;
            }
        }

        public string StrippedText => PostFix != "" ? text.Replace(PostFix, "") : text;

        public bool TryGetValue(out uint value) {
            string text2 = StrippedText;
            if (text2 == "") {
                value = 0;
                return true;
            } else {
                return uint.TryParse(text2, out value);
            }
        }

        public uint Value {
            set => text = value.ToString() + PostFix;
            get {
                if(TryGetValue(out uint ret)){
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
