namespace AdvancedRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using AdvancedRoads.Patches.RoadEditor;

    public class BitMaskPanel: UIPanel  {
        public UILabel Label;
        public UICheckboxDropDown DropDown;

        public delegate void SetHandlerD(uint value);
        public delegate uint GetHandlerD();
        public SetHandlerD SetHandler;
        public GetHandlerD GetHandler;
        public Type EnumType;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public static BitMaskPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            Type enumType,
            SetHandlerD setHandler,
            GetHandlerD getHandler) {
            Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label}, enumType:{enumType})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(BitMaskPanel)) as BitMaskPanel;
            subPanel.EnumType = enumType;
            subPanel.SetHandler = setHandler;
            subPanel.GetHandler = getHandler;
            subPanel.Initialize();
            subPanel.Label.text = label;

            roadEditorPanel.m_Container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);


            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            size = new Vector2(370, 27);

            Label = AddUIComponent<UILabel>();
            Label.relativePosition = new Vector2(0,4);

            DropDown = AddUIComponent<UICheckboxDropDown>();
            DropDown.relativePosition = new Vector2(158, 2);
            DropDown.size = new Vector2(206, 22);
            DropDown.verticalAlignment = UIVerticalAlignment.Middle;
            DropDown.horizontalAlignment = UIHorizontalAlignment.Center;

            DropDown.atlas = TextureUtil.GetAtlas("InMapEditor");
            DropDown.normalBgSprite = "TextFieldPanel";
            DropDown.uncheckedSprite = "check-unchecked";
            DropDown.checkedSprite = "check-checked";

            DropDown.listBackground = "GenericPanelWhite";
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
            button.size = DropDown.size;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.atlas = TextureUtil.GetAtlas("InGame");
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.normalBgSprite = "TextFieldPanel";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.relativePosition = new Vector3(0, 0);

            //DropDown.listScrollbar = DropDown.AddUIComponent<UIScrollbar>();
            //DropDown.listScrollbar.autoHide = true;
            //DropDown.listScrollbar.size = new Vector2(12, 300);
            //DropDown.listScrollbar.incrementAmount = 60;
        }

        private void Initialize() {
            //Disable();
            Populate(DropDown, GetHandler(), EnumType);
            UpdateText();
            Enable();
        }

        public static void Populate(UICheckboxDropDown dropdown, uint flags, Type enumType) {
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
            UIButton button = DropDown.triggerButton as UIButton;
            button.atlas = TextureUtil.GetAtlas("InGame");
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
