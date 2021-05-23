namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using AdaptiveRoads.Util;
    using System.Reflection;
    using System.Linq;
    using KianCommons.UI.Helpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Plugins;



    public class BitMaskPanel : BitMaskPanelBase{
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
                subPanel.Initialize();
                subPanel.Label.text = label + ":";
                subPanel.Hint = hint;
                //if (dark)
                //    subPanel.opacity = 0.1f;
                //else
                //    subPanel.opacity = 0.3f;

                container.AttachUIComponent(subPanel.gameObject);
                roadEditorPanel.FitToContainer(subPanel);
                subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;

                return subPanel;
            }catch(Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        protected override void Initialize() {
            //Disable();
            Populate(DropDown, FlagData.GetValueLong(), FlagData.EnumType);
            UpdateText();
            Enable();
        }

        protected override void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown) {
            SetValue(GetCheckedFlags());
            UpdateText();
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

        private string ToText(IConvertible value) =>
            Enum.Format(enumType: FlagData.EnumType, value: value, format: "G");

        private void UpdateText() {
            var flags = FlagData.GetValue();
            string text = ToText(flags);
            ApplyText(DropDown, text);
        }
    }
}
