namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using AdaptiveRoads.Util;
    using System.Linq;

    public class MultiBitMaskPanel : BitMaskPanelBase {
        internal FlagDataT[] FlagDatas;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static MultiBitMaskPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            params FlagDataT[] flagDatas) {
            Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(MultiBitMaskPanel)) as MultiBitMaskPanel;
            subPanel.FlagDatas = flagDatas;
            subPanel.Initialize();
            subPanel.Label.text = label + ":";
            subPanel.Hint = hint;

            container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);
            subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;

            return subPanel;
        }

        protected override void Initialize() {
            //Disable();
            Populate(DropDown, FlagDatas);
            UpdateText();
            Enable();
        }

        internal static void Populate(UICheckboxDropDown dropdown, FlagDataT[] flagDatas) {
            foreach (FlagDataT flagData in flagDatas) {
                Populate(dropdown, flagData.GetValueLong(), flagData.EnumType);
            }
        }

        protected override void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown) {
            SetValue(GetCheckedFlags());
            UpdateText();
        }

        // apply checked flags from UI to prefab
        protected void SetValue(long[] enumFlags) {
            for (int i = 0; i < FlagDatas.Length; ++i) {
                long originalValue = FlagDatas[i].GetValueLong();
                if (originalValue != enumFlags[i]) {
                    FlagDatas[i].SetValueLong(enumFlags[i]);
                    OnPropertyChanged();
                }
            }
        }

        // get checked flags in UI
        private long[] GetCheckedFlags() {
            long[] ret = new long[FlagDatas.Length];
            for (int i = 0; i < DropDown.items.Length; i++) {
                if (DropDown.GetChecked(i)) {
                    IConvertible flag = DropDown.GetItemUserData(i) as IConvertible;
                    int j = FlagDatas.FindIndex(item => item.EnumType == flag.GetType());
                    Assertion.GTEq(j, 0, "j");
                    ret[j] |= flag.ToInt64();
                }
            }
            return ret;
        }

        private string ToText(IConvertible[] enumFlags) {
            string ret = "";
            for (int i = 0; i < FlagDatas.Length; ++i) {
                if (enumFlags[i].ToInt64() == 0) continue;
                if (ret != "") ret += ", ";
                var value = Convert2RawInteger(enumFlags[i], FlagDatas[i].UnderlyingType);
                ret += Enum.Format(enumType: FlagDatas[i].EnumType, value: value, "G");
            }
            if (ret == "") ret = "None";
            return ret;
        }

        private void UpdateText() {
            var enumFlags = FlagDatas.Select(item => item.GetValue()).ToArray();
            string text = ToText(enumFlags);
            ApplyText(DropDown, text);
        }
    }
}
