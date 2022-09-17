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
            Log.Called($"container:{container}, label:{label}");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(MultiBitMaskPanel)) as MultiBitMaskPanel;
            subPanel.FlagDatas = flagDatas;
            subPanel.Label.text = label + ":";
            subPanel.Hint = hint;
            subPanel.Target = roadEditorPanel.GetTarget();
            subPanel.Initialize();

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
            try {
                SetValue(GetCheckedFlags());
                UpdateText();
            } catch (Exception ex) {
                ex.Log();
            }
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

        private string GetText() {
            string ret = "";
            for (int i = 0; i < FlagDatas.Length; ++i) {
                var text = FlagDatas[i].GetValueString();
                if (text == "" || text == "None") continue;
                if (ret != "") ret += ", ";
                ret += text;
            }
            if (ret == "") ret = "None";
            return ret;
        }

        private void UpdateText() {
            string text = GetText();
            ApplyText(DropDown, text);
        }
    }
}
