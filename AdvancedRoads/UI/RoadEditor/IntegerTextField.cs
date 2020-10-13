namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.HelpersExtensions;

    public class IntegerTextField : UITextField {
        //UIResetButton resetButton_;

        public override string ToString() => GetType().Name + $"({name})";

        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            size = new Vector2(180,22);
            horizontalAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;
            selectionSprite = "EmptySprite";
            normalBgSprite = "TextFieldPanel";
            selectionBackgroundColor = new Color32(0, 105, 210, 255);
            text = "0";

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

        public bool TryGetValue(out int value) {
            string text2 = StrippedText;
            if (text2 == "") {
                value = 0;
                return true;
            }

            var ret = int.TryParse(text2, out value);
            return ret;
        }

        public int Value {
            set => text = value.ToString() + PostFix;
            get => int.Parse(StrippedText);
        }
    }
}
