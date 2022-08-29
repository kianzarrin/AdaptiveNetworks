namespace AdaptiveRoads.UI.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Plugins;
    using KianCommons.UI;
    using System;
    using UnityEngine;

    public class RangePanel8 : UIPanel, IHint {
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;
        string hint_;
        UILabel Label;
        TextFieldByte_8 LowerField, UpperField;
        Traverse<byte> from_, to_;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        public static RangePanel8 Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            Traverse<byte> from,
            Traverse<byte> to) {
            Log.Debug($"RangePanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(RangePanel8)) as RangePanel8;
            subPanel.from_ = from;
            subPanel.to_ = to;
            subPanel.Initialize();
            subPanel.Label.text = label + ":";
            subPanel.hint_ = hint;

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
            LowerField = AddUIComponent<TextFieldByte_8>();
            UpperField = AddUIComponent<TextFieldByte_8>();
            UpperField.width = LowerField.width = 100;

            Label.tooltip = "0 to 8";
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
            if(LowerField.TryGetValue(out byte lower))
                from_.Value = lower;
            if (UpperField.TryGetValue(out byte upper))
                to_.Value = upper;
            EventPropertyChanged?.Invoke();
        }

        private void RefreshText() {
            LowerField.Value = from_.Value;
            UpperField.Value = to_.Value;
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
            string h = hint_;
            if(UpperField.containsMouse) {
                h = h + "\nfrom (inclusive).";
            } else if(LowerField.containsMouse) {
                h = h + "\nto (inclusive).";
            }

            return h;
        }

    }
}
