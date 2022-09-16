namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CustomStringDataT  {
        static string[] EMPTY = new string[0];
        readonly RefChain<string[]> selected_;
        readonly Dictionary<string, int> source_;

        public CustomStringDataT(RefChain<string[]> selected, Dictionary<string, int> source_) {
            selected_ = selected;
            this.source_ = source_;
        }

        public string[] Selected {
            get => selected_.Value ?? EMPTY;
            set => selected_.Value = value ?? EMPTY;
        }

        public string[] AllItems => source_.Keys.ToArray();
    }

    internal class CustomTagsDataT : CustomStringDataT {
        public CustomTagsDataT(RefChain<string[]> selected) : base(selected, NetUtil.kTags) { }
        public CustomTagsDataT(object target, string field) :
            this(RefChain.Create(target).Field<string[]>(field)) { }
    }

    // MSDD = multi select drop down
    public class StringListMSDD : BitMaskPanelBase {
        class CustomItemUserData {
            public bool AddCutstomItem;
            public string Hint;
            public override string ToString() => Hint;
        }

        CustomStringDataT CustomStringData;


        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static StringListMSDD Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            CustomStringDataT customStringData) {
            Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(StringListMSDD)) as StringListMSDD;
            subPanel.Target = roadEditorPanel.GetTarget();
            subPanel.CustomStringData = customStringData;
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
            Populate(DropDown, CustomStringData.AllItems, CustomStringData.Selected);
            UpdateText();
            DropDown.eventCheckedChanged -= DropDown_eventCheckedChanged;
            DropDown.eventCheckedChanged += DropDown_eventCheckedChanged;
            Enable();
        }

        internal static void Populate(UICheckboxDropDown dropdown, string[] allStrings, string[] checkedStrings) {
            foreach (var item in checkedStrings) {
                dropdown.AddItem(
                    item: item,
                    isChecked: true,
                    userData: null);
            }
            foreach (var item in allStrings.Except(checkedStrings)) {
                dropdown.AddItem(
                    item: item,
                    isChecked: false,
                    userData: null);
            }
            dropdown.AddItem(
                item: "<Add ...>",
                isChecked: false,
                userData: new CustomItemUserData {
                    AddCutstomItem = true,
                    Hint = "Add new tags"
                });
        }
        private void DropDown_eventCheckedChanged(UIComponent component, int value) {
            try {
                Log.Called(component, value);
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
                DropDown.items = DropDown.items.AddToArray(item);
                SetChecked(item, true);
                Refresh();
            } catch (Exception ex) {
                ex.Log();
            }
        }

        protected override void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown) {
            try {
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


        // apply checked strings from UI to prefab
        protected void SetValue(string[] values) {
            var selected = CustomStringData.Selected;
            bool change = values.Except(selected).Any() || selected.Except(values).Any();
            if (change) {
                CustomStringData.Selected = values;
                OnPropertyChanged();
            }
        }

        private IEnumerable<string> GetCheckedStrings() {
            for (int i = 0; i < DropDown.items.Length; i++) {
                bool addItem = DropDown.GetItemUserData(i) is CustomItemUserData item && item.AddCutstomItem;
                if (DropDown.GetChecked(i) && !addItem) {
                    yield return DropDown.items[i];
                }
            }
        }

        private string ToText(string[] values) {
            string ret = "";
            if (values.Any())
                if (ret != "") ret += ", ";
            ret += values.Join(", ");

            if (ret == "") ret = "None";
            return ret;
        }

        private void UpdateText() {
            string text = ToText(CustomStringData.Selected);
            ApplyText(DropDown, text);
        }
    }
}