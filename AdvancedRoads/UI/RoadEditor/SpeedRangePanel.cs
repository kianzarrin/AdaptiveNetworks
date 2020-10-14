namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;
    using AdaptiveRoads.Patches.RoadEditor;
    using System.Reflection;
    using AdaptiveRoads.Manager;
    using TrafficManager.API.Traffic;

    public class SpeedRangePanel : UIPanel {
        public UILabel Label;
        public TextFieldU32 LowerField, UpperField;
        FieldInfo fieldInfo_;
        object target_;

        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

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

            roadEditorPanel.m_Container.AttachUIComponent(subPanel.gameObject);
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
            LowerField.PostFix = UpperField.PostFix = "kph";
            
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
                    Range = new NetInfoExt.Range {
                        Lower = lower / ApiConstants.SPEED_TO_KMPH,
                        Upper = upper / ApiConstants.SPEED_TO_KMPH,
                    };
                }
            }
            EventPropertyChanged?.Invoke();
        }

        public NetInfoExt.Range Range{
            get => fieldInfo_.GetValue(target_) as NetInfoExt.Range;
            set => fieldInfo_.SetValue(target_, value.LogRet("set_Range"));
        }

        private void RefreshText() {
            float lower = Range?.Lower ?? 0;
            float upper = Range?.Upper ?? 0;
            LowerField.Value = (uint)Mathf.RoundToInt(lower * ApiConstants.SPEED_TO_KMPH);
            UpperField.Value = (uint)Mathf.RoundToInt(upper * ApiConstants.SPEED_TO_KMPH);
        }


        
    }
}
