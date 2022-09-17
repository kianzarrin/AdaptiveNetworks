namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using ColossalFramework.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using System;
    using AdaptiveRoads.Util;
    using System.Linq;
    using HarmonyLib;
    using System.Collections.Generic;
    using static AdaptiveRoads.Manager.NetInfoExtionsion;

    public class BitMaskPanelCustomisable : BitMaskPanelBase {
        class CustomItemUserData {
            public bool AddCutstomItem;
            public string Hint;
            public override string ToString() => Hint;
        }


        internal FlagDataT FlagData;
        internal TagBase Source;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static BitMaskPanelCustomisable Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            FlagDataT flagData,
            TagBase tagSource) {
            Log.Called($"container:{container}, label:{label}");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(BitMaskPanelCustomisable)) as BitMaskPanelCustomisable;
            subPanel.Target = roadEditorPanel.GetTarget();
            subPanel.FlagData = flagData;
            subPanel.Source = tagSource;
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
            DropDown.Clear();
            Populate(DropDown, FlagData, Source.TagSource.AllTags, Source.Selected);
            UpdateText();
            DropDown.eventCheckedChanged -= DropDown_eventCheckedChanged;
            DropDown.eventCheckedChanged += DropDown_eventCheckedChanged;
            Enable();
        }

        internal static void Populate(UICheckboxDropDown dropdown, FlagDataT flagData, string []allStrings, string []checkedStrings) {
            Populate(dropdown, flagData.GetValueLong(), flagData.EnumType);
            foreach(var item in checkedStrings) {
                dropdown.AddItem(
                    item: item,
                    isChecked: true,
                    userData: new CustomItemUserData { Hint = "custom connect group" });
            }
            foreach (var item in allStrings.Except(checkedStrings)) {
                dropdown.AddItem(
                    item: item,
                    isChecked: false,
                    userData: new CustomItemUserData { Hint = "custom connect group" });
            }
            dropdown.AddItem(
                item: "<Add Custom ...>",
                isChecked: false,
                userData: new CustomItemUserData {
                    AddCutstomItem = true,
                    Hint =
                    "Add custom connect group. this can be used \n" +
                    "in combination of the vanilla connect groups.\n" +
                    "If you do use custom connect groups, please also\n" +
                    "set lane types and vehicle types for compatibility with DC mods"});
        }
        private void DropDown_eventCheckedChanged(UIComponent component, int value) {
            try {
                LogCalled(component, value);
                if (DropDown.GetItemUserData(value) is CustomItemUserData cud &&
                    cud.AddCutstomItem &&
                    DropDown.GetChecked(value)) {
                    // user clicked <Add custom ...>
                    DropDown.SetChecked(value, false);
                    var panel = MiniPanel.Display();
                    var field = panel.AddTextField();
                    field.width = 200;
                    panel.AddButton("Add Custom item", null, () => OnItemAdded(field.text));
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }

        private void OnItemAdded(string item) {
            try {
                Log.Called(item);
                Source.Selected = Source.Selected.AddToArray(item);
                Log.Info("[p1] OnItemAdded: Source.Selected=" + Source.Selected.ToSTR());
                Refresh();
                Log.Info("[p2] OnItemAdded: Source.Selected=" + Source.Selected.ToSTR());
                SetChecked(item, true);
                Log.Info("[p3] OnItemAdded: Source.Selected=" + Source.Selected.ToSTR());
            } catch (Exception ex) {
                ex.Log();
            }
        }

        protected override void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown) {
            try {
                SetValue(GetCheckedFlags());
                SetValue(GetCheckedStrings().ToArray());
                UpdateText();
            } catch (Exception ex) {
                ex.Log();
            }
        }

        void SetChecked(string item, bool isChecked) {
            DropDown.SetChecked(item, isChecked);
            OnAfterDropdownClose(DropDown);
        }

        // apply checked flags from UI to prefab
        protected void SetValue(long value) {
            if (FlagData.GetValueLong() != value) {
                FlagData.SetValueLong(value);
                OnPropertyChanged();
            }
        }

        // apply checked strings from UI to prefab
        protected void SetValue(string[] values) {
            var selected = Source.Selected;
            bool change = values.Except(selected).Any() || selected.Except(values).Any();
            if (change) {
                Source.Selected = values;
                OnPropertyChanged();
            }
        }

        // get checked flags in UI
        private long GetCheckedFlags() {
            long ret = 0;
            for (int i = 0; i < DropDown.items.Length; i++) {
                if (DropDown.GetChecked(i) && DropDown.GetItemUserData(i) is Enum item) {
                    ret |= (item as IConvertible).ToInt64();
                }
            }
            return ret;
        }

        private IEnumerable<string> GetCheckedStrings() {
            for (int i = 0; i < DropDown.items.Length; i++) {
                if (DropDown.GetChecked(i) &&
                    DropDown.GetItemUserData(i) is CustomItemUserData item &&
                    !item.AddCutstomItem) {
                    yield return DropDown.items[i];
                }
            }
        }

        private string GetText() {
            string ret = "";
            if (FlagData.GetValue().ToInt64() != 0)
                ret = FlagData.GetValueString();

            var values = Source.Selected;
            if (values.Any() && ret != "")
                ret += ", ";

            ret += values.Join(", ");

            if (ret == "") ret = "None";
            return ret;
        }

        private void UpdateText() {
            ApplyText(DropDown, GetText());
        }
    }
}
