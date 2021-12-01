namespace AdaptiveRoads.UI.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.Plugins;
    using KianCommons.UI;
    using System;
    using System.Reflection;
    using UnityEngine;

    public class RangePanel : UIPanel, IHint {
        public UILabel Label;
        public TextFieldFloat LowerField, UpperField;
        FieldInfo fieldInfo_;
        object target_;

        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        public static RangePanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            object target,
            FieldInfo fieldInfo) {
            Log.Debug($"RangePanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(RangePanel)) as RangePanel;
            subPanel.fieldInfo_ = fieldInfo;
            subPanel.target_ = target;
            subPanel.Initialize();
            subPanel.Label.text = label + ":";

            container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);

            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            //backgroundSprite = "GenericPanelWhite";
            //color = Color.white;

            size = new Vector2(370, 27);
            atlas = TextureUtil.Ingame;
            color = new Color32(87, 97, 100, 255);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            padding = new RectOffset(0, 0, 3, 3);
            autoLayoutPadding = new RectOffset(0, 3, 0, 0);

            Label = AddUIComponent<UILabel>();
            LowerField = AddUIComponent<TextFieldFloat>();
            UpperField = AddUIComponent<TextFieldFloat>();
            UpperField.width = LowerField.width = 100;

            Label.tooltip = "if both are 0, it is ignored.";
            LowerField.tooltip = "from";
            UpperField.tooltip = "to";

            Label.eventSizeChanged += (_c, _val) => {
                float _p = 3 * 3; //padding 3 elements => 3 paddings.
                float widthRemaining = 370 - _p - _val.x;
                LowerField.width = UpperField.width = widthRemaining * 0.5f;
            };

            LowerField.eventTextSubmitted += TextSubmitted;
            UpperField.eventTextSubmitted += TextSubmitted;
        }

        private void Initialize() {
            //Disable();
            RefreshText();
            Enable();
        }

        private void TextSubmitted(UIComponent component, string value) {
            if(LowerField.TryGetValue(out float lower) && UpperField.TryGetValue(out float upper)) {
                if(upper == 0) {
                    Range = null;
                } else {
                    Range = new NetInfoExtionsion.Range {
                        Lower = lower,
                        Upper = upper,
                    };
                }
            }
            EventPropertyChanged?.Invoke();
        }

        public NetInfoExtionsion.Range Range {
            get => fieldInfo_.GetValue(target_) as NetInfoExtionsion.Range;
            set => fieldInfo_.SetValue(target_, value.LogRet("set_Range"));
        }

        private void RefreshText() {
            float lower = Range?.Lower ?? 0;
            float upper = Range?.Upper ?? 0;
            LowerField.Value = lower;
            UpperField.Value = upper;
        }

        [FPSBoosterSkipOptimizations]
        public override void Update() {
            try {
                base.Update();
                if(IsHovered())
                    backgroundSprite = "GenericPanelWhite";
                else
                    backgroundSprite = "";
            } catch(Exception ex) {
                ex.Log();
            }
        }

        public bool IsHovered() => containsMouse;
        public string GetHint() {
            string h = "set both lower and upper to 0 to ignore.";
            if(UpperField.containsMouse) {
                h = h + "\nUpper limit (exclusive).\n";
            } else if(LowerField.containsMouse) {
                h = h + "\nLower limit (inclusive).";
            }
            var h2 = fieldInfo_.GetHints()?.JoinLines();
            if(!string.IsNullOrEmpty(h2))
                h = h + "\n" + h2;

            return h;
        }

    }
}
