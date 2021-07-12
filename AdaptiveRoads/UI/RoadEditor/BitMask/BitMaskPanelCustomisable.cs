namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using ColossalFramework.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using System;
    using AdaptiveRoads.Util;
    using System.Linq;
    using HarmonyLib;
    using System.Collections.Generic;

    internal class ItemSource {
        static Dictionary<Type, ItemSource> sources_ = new Dictionary<Type, ItemSource>();
        public static ItemSource GetOrCreate(Type type) {
            if (!sources_.ContainsKey(type))
                sources_[type] = new ItemSource();
            return sources_[type];
        }

        public event Action eventItemSourceUpdated;
        string[] items_ = new string[0];
        public string[] Items => items_;
        

        public bool Add(string item) {
            LogCalled(item);
            if (!item.IsNullorEmpty() && !items_.Contains(item)) {
                items_ = items_.AddToArray(item);
                eventItemSourceUpdated?.Invoke();
                return true;
            }
            return false;
        }
    }

    internal struct CustomFlagDataT {
        public readonly ItemSource ItemSource;
        readonly Traverse selected_;

        public CustomFlagDataT(ItemSource itemSource, Traverse selected) {
            ItemSource = itemSource;
            selected_ = selected;
        }

        public string[] Selected {
            get => selected_.GetValue<string[]>() ?? new string[0];
            set {
                if (value == null || value.Length == 0)
                    selected_.SetValue(null);
                else
                    selected_.SetValue(value);
            }
        }
    }


    public class BitMaskPanelCustomisable : BitMaskPanelBase {
        class CustomItemUserData {
            public bool AddCutstomItem;
            public string Hint;
            public override string ToString() => Hint;
        }


        internal FlagDataT FlagData;
        internal CustomFlagDataT CustomFlagData;

        public override void OnDestroy() {
            CustomFlagData.ItemSource.eventItemSourceUpdated -= Refresh;
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static BitMaskPanelCustomisable Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            FlagDataT flagData,
            CustomFlagDataT customFlagData) {
            Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(BitMaskPanelCustomisable)) as BitMaskPanelCustomisable;
            subPanel.Target = roadEditorPanel.GetTarget();
            subPanel.FlagData = flagData;
            subPanel.CustomFlagData = customFlagData;
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
            Populate(DropDown, FlagData, CustomFlagData.ItemSource.Items, CustomFlagData.Selected);
            UpdateText();
            DropDown.eventCheckedChanged -= DropDown_eventCheckedChanged;
            DropDown.eventCheckedChanged += DropDown_eventCheckedChanged;
            CustomFlagData.ItemSource.eventItemSourceUpdated -= Refresh;
            CustomFlagData.ItemSource.eventItemSourceUpdated += Refresh;
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
                    "set lane types and vehicle types for compatibilty with DC mods"});
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
                LogCalled(item);
                CustomFlagData.ItemSource.Add(item); // adds item to drop down
                SetChecked(item, true);
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
            var selected = CustomFlagData.Selected;
            bool change = values.Except(selected).Any() || selected.Except(values).Any();
            if (change) {
                CustomFlagData.Selected = values;
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

        private string ToText(IConvertible value, string [] values) {
            value = Convert2RawInteger(value, FlagData.UnderlyingType);
            string ret = "";
            if (value.ToInt64() != 0)
                ret = Enum.Format(enumType: FlagData.EnumType, value: value, format: "G");

            if (values.Any())
                if (ret != "") ret += ", ";
                ret += values.Join(", ");

            if (ret == "") ret = "None";
            return ret;
        }

        private void UpdateText() {
            var enumFlags = FlagData.GetValue();
            var strings = CustomFlagData.Selected;
            string text = ToText(enumFlags, strings);
            ApplyText(DropDown, text);
        }
    }
}
