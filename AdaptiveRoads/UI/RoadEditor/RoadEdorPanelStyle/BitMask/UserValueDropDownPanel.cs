namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using ColossalFramework.UI;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using System;
    using AdaptiveRoads.Util;
    using System.Linq;
    using HarmonyLib;
    using KianCommons.UI;
    using AdaptiveRoads.Data;
    using AdaptiveRoads.Manager;
    using UnityEngine;
    using KianCommons.Plugins;
    using KianCommons.UI.Helpers;

    public class UserValueDropDownPanel: UIPanel, IDataUI {
        public class DataT {
            public UserValueNames Names;
            public UserDataInfo Target;
            public int Index;
            public event Action<UserValueNames> eventNamesUpdated;
            public event Action<int> eventEntryRemoved;
            public int Value {
                get {
                    try {
                        return Target.UserValues[Index].Value;
                    } catch (Exception ex) {
                        ex.Log();
                        return -1;
                    }
                }
                set {
                    try {
                        Target.UserValues[Index].Value = value;
                    } catch (Exception ex) {
                        ex.Log();
                    }
                }
            }

            public string ToText() {
                Log.Called(Value);
                if (Value < 0)
                    return "<Not Applicable>";
                else
                    return Names.Items[Value];
            }
            public void OnNamesUpdated() => eventNamesUpdated?.Invoke(Names);
            public void OnEntryRemoved() => eventEntryRemoved?.Invoke(Index);
        }

        class CustomItemUserData {
            public bool AddCutstomItem;
            public string Hint;
            public override string ToString() => Hint;
        }

        const string MODIFY_ITEM = "<Modify ...>";
        const string NA_ITEM = "<Not Applicable>";

        public DataT Data;
        public UIButton RemoveButton;
        public UILabel Label;
        public UIDropDown DropDown;
        public object Target;

        public string Hint;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        #region lifecycle
        public override void Awake() {
            try {
                base.Awake();
                size = new Vector2(370, 54);
                atlas = TextureUtil.Ingame;
                color = new Color32(87, 97, 100, 255);

                Label = AddUIComponent<UILabel>();
                Label.relativePosition = new Vector2(0, 6);

                RemoveButton = AddUIComponent<EditorArrayRemoveButton>();
                RemoveButton.isVisible = true;
                RemoveButton.eventClicked += Remove;

                DropDown = AddUIComponent<UIDropDown>();
                EditorDropDown.Init(DropDown);
                DropDown.relativePosition = new Vector2(width - DropDown.width, 28);
                DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;
                NetInfoExtionsion.Net.OnCustomFlagRenamed += Refresh;


                isInteractive = true;
            } catch (Exception ex) {
                ex.Log();
            }
        }

        public override void Start() {
            base.Start();
            size = new Vector2(370, 54);
        }

        public void Remove(UIComponent _, UIMouseEventParameter __) {
            try {
                Log.Called(Data.Index);
                Data.OnEntryRemoved();
                parent.RemoveUIComponent(this);
                Destroy(gameObject);
                OnPropertyChanged();
            } catch(Exception ex) {
                ex.Log();
            }
        }

        public void OnDestroy() {
            Log.Called();
            //ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
        }

        internal static UserValueDropDownPanel Add(
            RoadEditorPanel roadEditorPanel,
            RoadEditorCollapsiblePanel groupPanel,
            string hint,
            DataT data) {
            var subPanel = UIView.GetAView().AddUIComponent<UserValueDropDownPanel>();
            subPanel.Data = data;
            subPanel.Initialize();
            subPanel.Label.text = data.Names.Title + ":";
            subPanel.Hint = hint;

            groupPanel.AddComponent(subPanel, false);
            roadEditorPanel.FitToContainer(subPanel);

            subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;
            return subPanel;
        }

        [FPSBoosterSkipOptimizations]
        public override void Update() {
            try {
                base.Update();
                if (IsHovered())
                    backgroundSprite = "GenericPanelWhite";
                else
                    backgroundSprite = "";
            } catch (Exception ex) {
                ex.Log();
            }
            if (Input.GetMouseButtonDown(1))
                DropDown.ClosePopup(); // close all popups on left click
        }

        protected void Initialize() {
            try {
                Log.Called();
                //Disable();
                DropDown.items = new string[0];
                Populate(DropDown, Data.Names.Items, Data.Value);
                UpdateText();
                DropDown.eventSelectedIndexChanged -= DropDown_eventSelectedIndexChanged;
                DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;
                Enable();
            } catch (Exception ex) { ex.Log(); }
        }

        #endregion

        public void Refresh() => Initialize(); 

        internal static void Populate(UIDropDown dropdown, string []names, int value) {
            dropdown.AddItem(NA_ITEM);
            for (int i = 0; i < names.Length; ++i) {
                dropdown.AddItem(names[i]);
            }
            dropdown.AddItem(MODIFY_ITEM);
            dropdown.selectedIndex = value + 1;
        }

        private void DropDown_eventSelectedIndexChanged(UIComponent component, int selectedIndex) {
            try {
                LogCalled(component, selectedIndex);
                if(DropDown.items[selectedIndex] == MODIFY_ITEM) {
                    // user clicked <modify ...>
                    ModifyEntryMiniPanel();
                    Data.Value = -1;
                } else {
                    SetValue(selectedIndex - 1);
                }
            } catch (Exception ex) {
                ex.Log();
            }
            UpdateText();
        }

        public MiniPanel ModifyEntryMiniPanel() {
            var panel = MiniPanel.Display();

            panel.AddUIComponent<UILabel>().text = "Title:";
            var field = panel.AddTextField();
            field.text = Data.Names.Title;
            field.width = 200;

            panel.AddUIComponent<UILabel>().text = "items (delimiter is ,):";
            var field2 = panel.AddTextField();
            field2.text = Data.Names.Items.Join(" , ");
            field2.width = 400;
            panel.AddButton("Modify", null, () => {
                var entry = AddNewEntry(field.text, field2.text);
                OnItemsModified(entry);
            });
            return panel;
        }

        private void OnItemsModified(UserValueNames entry) {
            try {
                Data.Value = -1;
                Log.Called(entry);
                Data.Names = entry;
                Data.OnNamesUpdated();
                Refresh();
            } catch (Exception ex) {
                ex.Log();
            }
        }

        public static MiniPanel AddNewEntryMiniPanel(Action<UserValueNames> callBack) {
            var panel = MiniPanel.Display();

            panel.AddUIComponent<UILabel>().text = "Title:";
            var field = panel.AddTextField();
            field.text = "Title";
            field.width = 200;

            panel.AddUIComponent<UILabel>().text = "items (delimiter is ,):";
            var field2 = panel.AddTextField();
            field2.text = "Item0, Item1, Item2, Item3";
            field2.width = 400;
            panel.AddButton("Add", null, delegate() {
                var entry = AddNewEntry(field.text, field2.text);
                if (!entry.Items.IsNullorEmpty()) {
                    callBack(entry);
                }
            });
            return panel;
        }

        public static UserValueNames AddNewEntry(string title, string items) {
            try {
                Log.Called(title, items);
                var list = items.Split(",").
                    Select(item => item.Trim()).
                    Where(item => !string.IsNullOrEmpty(item)).
                    ToArray();

                return new UserValueNames {
                    Title = title,
                    Items = list,
                };
            } catch (Exception ex) {
                ex.Log();
                return default;
            }
        }

        public void OnPropertyChanged() {
            try {
                LogCalled();
                EventPropertyChanged?.Invoke();
            } catch (Exception ex) {
                ex.Log();
            }
        }


        // apply checked flags from UI to prefab
        void SetValue(int value) {
            Log.Called(value);
            if (Data.Value != value) {
                Data.Value = value;
                //OnPropertyChanged();
            }
        }

        private void UpdateText() {
            string text = Data.ToText();
            (DropDown.triggerButton as UIButton).text = text;
        }

        public bool IsHovered() {
            try {
                if (containsMouse)
                    return true;
                if (DropDown.GetHoverIndex() >= 0)
                    return true;
            } catch (Exception ex) {
                ex.Log();
            }
            return false;
        }

        public string GetHint() {
            try {
                int i = DropDown.GetHoverIndex();
                if (i >= 0) {
                    string item = DropDown.items[i];
                    if (item == NA_ITEM) {
                        return "ignore";
                    } else if (item == MODIFY_ITEM) {
                        return "modify item list";
                    } else if (containsMouse) {
                        return Hint;
                    }
                }
            } catch (Exception ex) {
                ex.Log();
            }
            return null;
        }
    }
}
