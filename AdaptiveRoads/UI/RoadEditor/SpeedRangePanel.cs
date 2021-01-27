namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;
    using AdaptiveRoads.Util;
    using System.Reflection;
    using AdaptiveRoads.Manager;
    using TrafficManager.API.Traffic;
    using TrafficManager.Manager.Impl;
    using System;
    using static ModSettings;

    public class SpeedRangePanel : UIPanel, IHint {
        static float HUMAN_TO_GAME => 1 / GAME_TO_HUMAN;
        static float MAX_HUMAN_SPEED => SpeedLimitManager.MAX_SPEED * GAME_TO_HUMAN;
        static float GAME_TO_HUMAN =>
            (SpeedUnitType)SpeedUnit.value switch {
                SpeedUnitType.KPH => ApiConstants.SPEED_TO_KMPH,
                SpeedUnitType.MPH => ApiConstants.SPEED_TO_MPH,
                _ => throw new Exception("unreachable code"),
            };
        static string unit_ =>
            (SpeedUnitType)SpeedUnit.value switch {
                SpeedUnitType.KPH => "kph",
                SpeedUnitType.MPH => "mph",
                _ => throw new Exception("unreachable code"),
            };


        public UILabel Label;
        public TextFieldU32 LowerField, UpperField;
        FieldInfo fieldInfo_;
        object target_;

        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        public static SpeedRangePanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            object target,
            FieldInfo fieldInfo) {
            Log.Debug($"RangePanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(SpeedRangePanel)) as SpeedRangePanel;
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
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            padding = new RectOffset(0, 0, 3, 3);
            autoLayoutPadding = new RectOffset(0, 3, 0, 0);

            Label = AddUIComponent<UILabel>();
            LowerField = AddUIComponent<TextFieldU32>();
            UpperField = AddUIComponent<TextFieldU32>();
            UpperField.width = LowerField.width = 100;

            Label.tooltip = "if both are 0, it is ignored.";
            LowerField.tooltip = "from";
            UpperField.tooltip = "to";
            LowerField.PostFix = UpperField.PostFix = unit_;
            
            Label.eventSizeChanged += (_c, _val) => {
                float _p = 3 * 3; //padding 3 elements => 3 paddings.
                float widthRemaining = 370 - _p - _val.x;
                LowerField.width = UpperField.width = widthRemaining * 0.5f;
            };

            LowerField.eventTextSubmitted += TextSubmitted ;
            UpperField.eventTextSubmitted += TextSubmitted;
        }

        private void Initialize() {
            //Disable();
            RefreshText();
            Enable();
        }

        private void TextSubmitted(UIComponent component, string value) {
            if (LowerField.TryGetValue(out uint lower) && UpperField.TryGetValue(out uint upper)) {
                if (upper == 0) {
                    Range = null;
                } else {
                    Range = new NetInfoExtionsion.Range {
                        Lower = lower * HUMAN_TO_GAME,
                        Upper = upper * HUMAN_TO_GAME,
                    };
                }
            }
            EventPropertyChanged?.Invoke();
        }

        public NetInfoExtionsion.Range Range{
            get => fieldInfo_.GetValue(target_) as NetInfoExtionsion.Range;
            set => fieldInfo_.SetValue(target_, value.LogRet("set_Range"));
        }

        private void RefreshText() {
            float lower = Range?.Lower ?? 0;
            float upper = Range?.Upper ?? 0;
            LowerField.Value = (uint)Mathf.RoundToInt(lower * GAME_TO_HUMAN);
            UpperField.Value = (uint)Mathf.RoundToInt(upper * GAME_TO_HUMAN);
        }

        public bool IsHovered() => containsMouse;
        public string GetHint() {
            string h = "set both speed limits to 0 to ignore speed limit";
            if(UpperField.containsMouse) {
                h = h + "\nUpper speed limit (exclusive).\n" +
                    $"set to {MAX_HUMAN_SPEED + 1}{unit_} or greator for unlimit speed";
            }else if(LowerField.containsMouse) {
                h = h + "\nLower speed limit (inclusive).";
            }

            return h;
        }
        
    }
}
