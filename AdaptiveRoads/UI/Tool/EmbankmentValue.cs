namespace AdaptiveRoads.UI.Tool {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;

    public class EmbankmentValue : UITextField {
        ushort segmentID_, nodeID_;
        ref NetSegmentEnd segEnd => ref segmentID_.ToSegmentExt().GetEnd(nodeID_);

        public static EmbankmentValue Add(UIPanel parent, ushort segmentID, ushort nodeID) {
            var ret = parent.AddUIComponent<EmbankmentValue>();
            ret.segmentID_ = segmentID;
            ret.nodeID_ = nodeID;
            return ret;
        }

        public override string ToString() => GetType().Name + $"({name})";

        private string _postfix = "°";
        public string PostFix {
            get => _postfix;
            set {
                if(value.IsNullOrWhiteSpace())
                    _postfix = "";
                else
                    _postfix = value;
            }
        }

        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            size = new Vector2(100, 20);
            padding = new RectOffset(4, 4, 3, 3);
            builtinKeyNavigation = true;
            isInteractive = true;
            readOnly = false;
            horizontalAlignment = UIHorizontalAlignment.Center;
            selectionSprite = "EmptySprite";
            selectionBackgroundColor = new Color32(0, 172, 234, 255);
            normalBgSprite = "TextFieldPanelHovered";
            disabledBgSprite = "TextFieldPanelHovered";
            textColor = new Color32(0, 0, 0, 255);
            disabledTextColor = new Color32(80, 80, 80, 128);
            color = new Color32(255, 255, 255, 255);
            textScale = 0.9f;
            useDropShadow = true;

            submitOnFocusLost = true;
            selectOnFocus = true;
            numericalOnly = true;
            allowFloats = true;
            allowNegative = true;
            tooltip =
                "mouse-wheel => increment/decrement.\n" +
                "shift + mouse-wheel => small increment/decrement.\n" +
                "alt + mouse-wheel => large increment/decrement.\n" +
                "del => reset hovered value to default"; ;
        }

        public override void Start() {
            base.Start();
            UIValue = AngleDeg;
        }


        public float MinStep => 1;
        private string format_ = "0.##";

        float ScrollStep {
            get {
                if(Helpers.ShiftIsPressed) {
                    return 1;
                } else if(Helpers.AltIsPressed) {
                    return 10;
                } else {
                    return 5;
                }
            }
        }

        public override void Update() {
            if(containsMouse && !containsFocus && Input.GetKeyDown(KeyCode.Delete))
                UIValue = AngleDeg = 0;
            base.Update();
        }

        protected override void OnMouseWheel(UIMouseEventParameter p) {
            base.OnMouseWheel(p);
            AddDelta(p.wheelDelta * ScrollStep, ScrollStep);
        }

        /// <summary>
        /// adds delta to Value rounding to step.
        /// </summary>
        /// <returns>final delta in Value after rounding</returns>
        public void AddDelta(float delta, float step) {
            Log.Debug(Environment.StackTrace);
            UIValue = AngleDeg = (AngleDeg + delta).RoundToNearest(step);
        }

        public string StrippedText {
            get {
                if(text.IsNullorEmpty() || PostFix.IsNullorEmpty())
                    return text;
                else
                    return text?.Replace(PostFix, "");
            }
        }

        public bool TryGetValue(out float value) {
            string text2 = StrippedText;
            if(text2 == "") {
                value = 0;
                return true;
            }

            var ret = float.TryParse(text2, out value);
            value = value.RoundToNearest(MinStep);
            return ret;
        }

        bool refreshing_;
        public float UIValue {
            set {
                refreshing_ = true;
                text = value.RoundToNearest(MinStep).ToString(format_) + PostFix;
                refreshing_ = false;
            }
            get => float.Parse(StrippedText).RoundToNearest(MinStep);
        }

        // get/set real value
        public float AngleDeg {
            get => segEnd.DeltaAngle * Mathf.Rad2Deg;
            set {
                segEnd.DeltaAngle = value * Mathf.Deg2Rad;
                NetworkExtensionManager.Instance.UpdateSegment(segmentID_);
            }
        }

        protected override void OnTextChanged() {
            //Log.Debug($"UICornerTextField.OnTextChanged() called");
            base.OnTextChanged();
            if(refreshing_) return;
            if(TryGetValue(out float value)) {
                AngleDeg = value;
            }
        }

        /// <summary>
        /// clean text value on submit.
        /// </summary>
        protected override void OnSubmit() {
            Log.Debug(this + $".OnSubmit() called");
            base.OnSubmit(); // called when focus is lost. deep refresh
            if(TryGetValue(out var value)) {
                AngleDeg = value;
            }
            UIValue = AngleDeg;
        }
    }
}
