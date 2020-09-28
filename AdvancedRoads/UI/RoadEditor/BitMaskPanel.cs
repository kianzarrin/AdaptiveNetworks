namespace AdvancedRoads.UI.RoadEditor {
    using AdvancedRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using JetBrains.Annotations;

    public class BitMaskPanel: UIPanel  {
        public UILabel Label;
        public UICheckboxDropDownExt DropDown;

        public delegate void SetHandlerD(uint value);
        public delegate uint GetHandlerD();
        public SetHandlerD SetHandler;
        public GetHandlerD GetHandler;
        public Type EnumType;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public static BitMaskPanel Add(
            UIComponent container,
            string label,
            Type enumType,
            SetHandlerD setHandler,
            GetHandlerD getHandler) {
            var subPanel = container.AddUIComponent<BitMaskPanel>();
            subPanel.EnumType = enumType;
            subPanel.SetHandler = setHandler;
            subPanel.GetHandler = getHandler;
            subPanel.Initialize();
            subPanel.Label.text = label;
            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            Label = AddUIComponent<UILabel>();
            Label.relativePosition = new Vector2(0,4);
            //Label.atlas = "InMapEditor";
            //Label.size = new Vector2(176, 18);

            DropDown = AddUIComponent<UICheckboxDropDownExt>();
            DropDown.relativePosition = new Vector2(158, 2);
            DropDown.size = new Vector2(206, 22);
            DropDown.zOrder = 1;
            DropDown.verticalAlignment = UIVerticalAlignment.Middle;
            DropDown.horizontalAlignment = UIHorizontalAlignment.Center;

            DropDown.atlas = TextureUtil.GetAtlas("InMapEditor");
            DropDown.normalBgSprite = "TextFieldPanel";
            DropDown.normalBgSprite = "IconDownArrow";
            DropDown.hoveredBgSprite = "IconDownArrowHovered";
            DropDown.focusedBgSprite = "IconDownArrowPressed";
            DropDown.listBackground = "GenericPanelWhite";
            DropDown.uncheckedSprite = "check-unchecked";
            DropDown.checkedSprite = "check-checked";

            DropDown.listWidth = 188;
            DropDown.listHeight = 300;
            DropDown.clampListToScreen = true;
            DropDown.listPosition = UICheckboxDropDown.PopupListPosition.Automatic;

            DropDown.itemHeight = 25;
            DropDown.itemHover = "ListItemHover";
            DropDown.itemHighlight = "ListItemHighlight";

            DropDown.popupColor = Color.black;
            DropDown.popupTextColor = Color.white;

            DropDown.triggerButton = DropDown.AddUIComponent<UIButton>();
            UIButton button = DropDown.triggerButton as UIButton;
            button.size = this.size;
            button.zOrder = 1;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.atlas = DropDown.atlas;
            button.normalBgSprite = "IconDownArrow";
            button.hoveredBgSprite = "IconDownArrowHovered";
            button.pressedBgSprite = "IconDownArrowPressed";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.relativePosition = new Vector3(0, 0);

            DropDown.listScrollbar = DropDown.AddUIComponent<UIScrollbar>();
            DropDown.listScrollbar.autoHide = true;
            DropDown.listScrollbar.size = new Vector2(12, 300);
            DropDown.listScrollbar.incrementAmount = 60;
        }

        private void Initialize() {
            Disable();
            Populate(DropDown, GetHandler(), EnumType);
            Enable();
        }

        public static void Populate(UICheckboxDropDownExt dropdown, uint flags, Type enumType) {
            var values = EnumBitMaskExtensions.GetPow2ValuesU32(enumType);
            foreach (uint flag in values) {
                bool hasFlag = (flags & flag) != 0;
                dropdown.AddItem(
                    item: Enum.GetName(enumType, flag),
                    isChecked: hasFlag,
                    userData: flag);
            }
        }

        public override void Start() {
            base.Start();
        }

        public override void OnEnable() {
            if (DropDown != null) {
                DropDown.eventAfterDropdownClose += DropdownClose;
            }
            base.OnEnable();
        }

        public override void OnDisable () {
            if (DropDown != null) {
                DropDown.eventAfterDropdownClose += DropdownClose;
            }
            base.OnDisable();
        }

        private void DropdownClose(UICheckboxDropDown checkboxdropdown) {
            SetValue(GetCheckedFlags());
            UpdateText();
        }

        // apply checked flags from UI to prefab
        protected void SetValue(uint value) {
            if(GetHandler() != value) {
                SetHandler(value);
                EventPropertyChanged?.Invoke();
            }
        }

        // get checked flags in UI
        private uint GetCheckedFlags() {
            uint ret = 0;
            for (int i = 0; i < DropDown.items.Length; i++) {
                if (DropDown.GetChecked(i)) {
                    ret |= (uint)DropDown.GetItemUserData(i);
                }
            }
            return ret;
        }


        private void UpdateText() {
            uint flags = GetCheckedFlags();
            UIButton uibutton = (UIButton)DropDown.triggerButton;
            uibutton.text = Enum.Format(enumType: EnumType, value:flags, "G");
        }
    }
}
