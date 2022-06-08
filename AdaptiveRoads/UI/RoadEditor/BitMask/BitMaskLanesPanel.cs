namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.Plugins;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class LaneInfoData {
        public int Index;
        public NetInfo.Lane Lane;
        public string DisplayText => $"[{Index}] {Lane.m_laneType}, {Lane.m_vehicleType}, {Lane.m_position} ";
        public ulong Mask => 1ul << Index;
    }

    public class BitMaskLanesAttribute: Attribute { }

    public class BitMaskLanesPanel : UIPanel, IDataUI {
        public UILabel Label;
        public UICheckboxDropDown DropDown;
        public object Target;
        public FieldInfo Field;
        public NetInfo ParentInfo;

        public string Hint;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public ulong GetValue() => (ulong)Field.GetValue(Target);
        public void SetValue(ulong value) {
            Field.SetValue(Target, value);
            OnPropertyChanged();
        }
        public NetInfo.Lane[] Lanes => ParentInfo.m_lanes;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }
        internal static BitMaskLanesPanel Add(
            RoadEditorPanel roadEditorPanel,
            FieldInfo field,
            NetInfo netInfo,
            UIComponent container,
            string label,
            string hint) {
            try {
                Log.Debug($"BitMaskLanesPanel.Add(container:{container}, label:{label}");
                var subPanel = UIView.GetAView().AddUIComponent(typeof(BitMaskLanesPanel)) as BitMaskLanesPanel;
                subPanel.Target = roadEditorPanel.GetTarget();
                subPanel.Field = field;
                subPanel.ParentInfo = netInfo;
                subPanel.Label.text = label + ":";
                subPanel.Hint = hint;
                subPanel.Initialize();

                container.AttachUIComponent(subPanel.gameObject);
                roadEditorPanel.FitToContainer(subPanel);
                subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;

                return subPanel;
            } catch(Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        public override void Awake() {
            try {
                base.Awake();
                size = new Vector2(370, 54);
                atlas = TextureUtil.Ingame;
                color = new Color32(87, 97, 100, 255);

                Label = AddUIComponent<UILabel>();
                Label.relativePosition = new Vector2(0, 6);

                DropDown = AddUIComponent<UICheckboxDropDown>();
                EditorMultiSelectDropDown.Init(DropDown);
                DropDown.relativePosition = new Vector2(width - DropDown.width, 28);
                DropDown.eventAfterDropdownClose += OnAfterDropdownClose;

                isInteractive = true;
            } catch(Exception ex) {
                ex.Log();
            }
        }

        public override void Start() {
            base.Start();
            FitTo(parent, LayoutDirection.Horizontal);
        }

        protected override void OnClick(UIMouseEventParameter p) {
            base.OnClick(p);
            if(!p.used && p.buttons == UIMouseButton.Left) {
                p.Use();
                DropDown.Invoke("OpenPopup", 0);
            }
        }

        protected void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown) {
            try {
                SetValue(GetCheckedIndexes());
                UpdateText();
            } catch(Exception ex) {
                ex.Log();
            }
        }
        private ulong GetCheckedIndexes() {
            ulong ret = 0;
            for(int i = 0; i < DropDown.items.Length; i++) {
                if(DropDown.GetChecked(i)) {
                    var data = DropDown.GetItemUserData(i) as LaneInfoData;
                    ret |= data.Mask;
                }
            }
            return ret;
        }

        public void OnPropertyChanged() {
            try {
                LogCalled();
                EventPropertyChanged?.Invoke();
            } catch(Exception ex) {
                ex.Log();
            }
        }

        protected void Initialize() {
            Populate(DropDown, GetValue(), Lanes);
            UpdateText();
            Enable();
        }



        public static void Populate(UICheckboxDropDown dropdown, ulong mask, NetInfo.Lane[] lanes) {
            try {
                for(int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                    var data = new LaneInfoData { Lane = lanes[laneIndex], Index = laneIndex };
                    bool hasLane = (mask & data.Mask) != 0;
                    dropdown.AddItem(
                        item: data.DisplayText,
                        isChecked: hasLane,
                        userData: data);
                }
            } catch(Exception ex) {
                ex.Log();
            }
        }

        static MethodInfo mObtainTextRenderer = GetMethod(typeof(UIButton), "ObtainTextRenderer");
        protected static UIFontRenderer ObtainTextRenderer(UIButton button) =>
            mObtainTextRenderer.Invoke(button, null) as UIFontRenderer;

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
            if(Input.GetMouseButtonDown(1))
                DropDown.ClosePopup(); // close all pop-ups on left click
        }

        public bool IsHovered() {
            try {
                if(containsMouse)
                    return true;
                if(DropDown.GetHoverIndex() >= 0)
                    return true;
            } catch(Exception ex) {
                ex.Log();
            }
            return false;
        }

        public string GetHint() {
            try {
                int i = DropDown.GetHoverIndex();
                if(i >= 0) {
                    //var userData = DropDown.GetItemUserData(i);
                    List<string> hints = new List<string>();
                    hints.Add("right-click => close drop down");
                    return hints.JoinLines();
                } else if(containsMouse) {
                    return Hint;
                } else {
                    return null;
                }
            } catch(Exception ex) {
                ex.Log();
                return null;
            }
        }

        private string ToText(ulong mask) {
            List<int> indexes = new List<int>();
            int index = 0;
            while(mask != 0) {
                if((mask&1)!= 0) {
                    indexes.Add(index);
                }
                index++;
                mask >>= 1;
            }

            return indexes.Select(item => item.ToString()).Join(",");
        }

        private void UpdateText() {
            var mask = GetValue();
            string text = ToText(mask);
            BitMaskPanelBase.ApplyText(DropDown, text);
        }
        public void Refresh() {
            try {
                DropDown.Clear();
                Initialize();
            } catch(Exception ex) { ex.Log(); }
        }
    }
}
