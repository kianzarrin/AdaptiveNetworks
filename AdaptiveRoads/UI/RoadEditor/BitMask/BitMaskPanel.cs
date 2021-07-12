namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using AdaptiveRoads.Util;



    public class BitMaskPanel : BitMaskPanelBase {
        internal FlagDataT FlagData;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static BitMaskPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            FlagDataT flagData) {
            try {
                Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label}, enumType:{flagData.EnumType})");
                var subPanel = UIView.GetAView().AddUIComponent(typeof(BitMaskPanel)) as BitMaskPanel;
                subPanel.FlagData = flagData;
                subPanel.Target = roadEditorPanel.GetTarget();
                subPanel.Label.text = label + ":";
                subPanel.Hint = hint;
                subPanel.Initialize();

                container.AttachUIComponent(subPanel.gameObject);
                roadEditorPanel.FitToContainer(subPanel);
                subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;
                

                return subPanel;
            } catch (Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        protected override void Initialize() {
            try {
                //Disable();
                Populate(DropDown, FlagData.GetValueLong(), FlagData.EnumType);
                UpdateText();
                Enable();
            } catch (Exception ex) {
                ex.Log();
            }
        }


        protected override void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown) {
            try {
                SetValue(GetCheckedFlags());
                UpdateText();
            } catch (Exception ex) {
                ex.Log();
            }
        }

        // apply checked flags from UI to prefab
        protected void SetValue(long value) {
            if (FlagData.GetValueLong() != value) {
                FlagData.SetValueLong(value);
                OnPropertyChanged();
            }
        }

        // get checked flags in UI
        private long GetCheckedFlags() {
            long ret = 0;
            for (int i = 0; i < DropDown.items.Length; i++) {
                if (DropDown.GetChecked(i)) {
                    ret |= (DropDown.GetItemUserData(i) as IConvertible).ToInt64();
                }
            }
            return ret;
        }

        private string ToText(IConvertible value) {
            value = Convert2RawInteger(value, FlagData.UnderlyingType);
            return Enum.Format(enumType: FlagData.EnumType, value: value, format: "G");
        }

        private void UpdateText() {
            var flags = FlagData.GetValue();
            string text = ToText(flags);
            ApplyText(DropDown, text);
        }
    }
}
