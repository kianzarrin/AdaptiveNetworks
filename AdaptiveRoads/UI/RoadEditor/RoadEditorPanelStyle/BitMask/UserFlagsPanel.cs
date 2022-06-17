namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using AdaptiveRoads.Data;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Linq;
    using static KianCommons.ReflectionHelpers;
    public class UserFlagsPanel : BitMaskPanelBase {
        public class DataT {
            public UserFlagsNames Names;
            public UserDataInfo Target;
            public int Index;
            public bool ForRequiredField;
            public event Action<UserFlagsNames> eventNamesUpdated;
            public int Flags {
                get {
                    if (ForRequiredField) {
                        return Target.UserFlags[Index].Required;
                    } else {
                        return Target.UserFlags[Index].Forbidden;
                    }
                }
                set {
                    if (ForRequiredField) {
                        Target.UserFlags[Index].Required = value;
                    } else {
                        Target.UserFlags[Index].Forbidden = value;
                    }
                }
            }

            public string ToText() {
                if (Flags == 0)
                    return "None";
                else
                    return Names.ToNames(Flags).Join(", ");
            }
            public void OnNamesUpdated() => eventNamesUpdated?.Invoke(Names);
        }

        public DataT Data;

        class CustomItemUserData {
            public bool AddCutstomItem;
            public string Hint;
            public override string ToString() => Hint;
        }


        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static UserFlagsPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string hint,
            DataT data) {
            Log.Debug($"BitMaskPanel.Add(container:{container})");
            var subPanel = UIView.GetAView().AddUIComponent<UserFlagsPanel>();
            subPanel.Target = roadEditorPanel.GetTarget();
            subPanel.Data = data;
            subPanel.Initialize();
            subPanel.Label.text = data.Names.Title + ":";
            subPanel.Hint = hint;

            container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);
            subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;

            return subPanel;
        }

        protected override void Initialize() {
            //Disable();
            DropDown.Clear();
            Populate(DropDown, Data.Names.Items, Data.Flags);
            UpdateText();
            DropDown.eventCheckedChanged -= DropDown_eventCheckedChanged;
            DropDown.eventCheckedChanged += DropDown_eventCheckedChanged;
            Data.eventNamesUpdated -= Refresh;
            Data.eventNamesUpdated += Refresh;
            Enable();
        }
        private void Refresh(UserFlagsNames _) => base.Refresh();

        internal static void Populate(UICheckboxDropDown dropdown, string[] names, int flags) {
            for (int i = 0; i < names.Length; ++i) {
                dropdown.AddItem(
                    item: names[i],
                    isChecked: flags.IsAnyFlagSet(1 << i));
            }
            dropdown.AddItem(
                item: "<modify ...>",
                isChecked: false,
                userData: new CustomItemUserData { AddCutstomItem = true });
        }

        private void DropDown_eventCheckedChanged(UIComponent component, int value) {
            try {
                LogCalled(component, value);
                if (DropDown.GetItemUserData(value) is CustomItemUserData cud &&
                    cud.AddCutstomItem &&
                    DropDown.GetChecked(value)) {
                    // user clicked <modify ...>
                    DropDown.SetChecked(value, false);
                    var panel = MiniPanel.Display();

                    panel.AddUIComponent<UILabel>().text = "Title:";
                    var field = panel.AddTextField();
                    field.text = Data.Names.Title;
                    field.width = 200;

                    panel.AddUIComponent<UILabel>().text = "items (delimiter is ,):";
                    var field2 = panel.AddTextField();
                    field.text = Data.Names.Items.Join(" , ");
                    field.width = 400;
                    panel.AddButton("Modify", null, () => OnItemsModified(field.text, field2.text));
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }

        private void OnItemsModified(string title, string items) {
            try {
                Log.Called(title, items);
                var list = items.Split(",").
                    Select(item => item.Trim()).
                    Where(item => !string.IsNullOrEmpty(item)).
                    ToArray();

                Data.Names = new UserFlagsNames {
                    Title = title,
                    Items = list,
                };
                Data.OnNamesUpdated();
                OnPropertyChanged();
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

        void SetChecked(string item, bool isChecked) {
            DropDown.SetChecked(item, isChecked);
            OnAfterDropdownClose(DropDown);
        }

        // apply checked flags from UI to prefab
        protected void SetValue(int value) {
            if (Data.Flags != value) {
                Data.Flags = value;
                OnPropertyChanged();
            }
        }

        // get checked flags in UI
        private int GetCheckedFlags() {
            int ret = 0;
            for (int i = 0; i < DropDown.items.Length; i++) {
                ret |= 1 << i;
            }
            return ret;
        }

        private void UpdateText() {
            string text = Data.ToText();
            ApplyText(DropDown, text);
        }
    }
}
