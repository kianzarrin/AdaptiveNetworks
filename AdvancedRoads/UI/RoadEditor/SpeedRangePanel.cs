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
        public IntegerTextField LowerField, UpperField;
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
            size = new Vector2(370, 54);
            atlas = TextureUtil.Ingame;
            //backgroundSprite = "GenericPanelWhite";
            //color = Color.white;

            Label = AddUIComponent<UILabel>();
            Label.relativePosition = new Vector2(0, 6);

            LowerField = AddUIComponent<IntegerTextField>();
            UpperField = AddUIComponent<IntegerTextField>();

            LowerField.eventTextSubmitted += TextSubmitted;
            UpperField.eventTextSubmitted += TextSubmitted;
        }

        private void Initialize() {
            //Disable();
            RefreshText();
            Enable();
        }

        private void TextSubmitted(UIComponent component, string value) {
            if (LowerField.TryGetValue(out int lower) && UpperField.TryGetValue(out int upper)) {
                if (lower == 0 && upper == 0) {
                    Range = null;
                } else {
                    Range = new NetInfoExt.Range {
                        Lower = lower / ApiConstants.SPEED_TO_KMPH,
                        Upper = upper / ApiConstants.SPEED_TO_KMPH,
                    };
                }
            }
        }

        public NetInfoExt.Range Range{
            get => fieldInfo_.GetValue(target_) as NetInfoExt.Range;
            set => fieldInfo_.SetValue(target_,value);
        }

        private void RefreshText() {
            float lower = Range?.Lower ?? 0;
            float upper = Range?.Upper ?? 0;
            LowerField.Value = Mathf.RoundToInt(lower * ApiConstants.SPEED_TO_KMPH);
            UpperField.Value = Mathf.RoundToInt(upper * ApiConstants.SPEED_TO_KMPH);
        }


        
    }
}
