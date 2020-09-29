namespace AdvancedRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using AdvancedRoads.Patches.RoadEditor;
    using System.Reflection;
    using HarmonyLib;

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
            GetHandlerD getHandler,
            bool dark) {
            Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label}, enumType:{enumType})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(BitMaskPanel)) as BitMaskPanel;
            subPanel.EnumType = enumType;
            subPanel.SetHandler = setHandler;
            subPanel.GetHandler = getHandler;
            subPanel.Initialize();
            subPanel.Label.text = label + ":";
            //if (dark)
            //    subPanel.opacity = 0.1f;
            //else
            //    subPanel.opacity = 0.3f;

            roadEditorPanel.m_Container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);

            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            size = new Vector2(370, 54);
            atlas = TextureUtil.Ingame;
            //backgroundSprite = "GenericPanelWhite";
            //color = Color.white;

            Label = AddUIComponent<UILabel>();
            Label.relativePosition = new Vector2(0,6);

            DropDown = AddUIComponent<UICheckboxDropDown>();
            DropDown.size = new Vector2(370, 22);
            DropDown.relativePosition = new Vector2(width- DropDown.width, 28);
            DropDown.verticalAlignment = UIVerticalAlignment.Middle;
            DropDown.horizontalAlignment = UIHorizontalAlignment.Center;

            DropDown.atlas = TextureUtil.InMapEditor;
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
            button.atlas = TextureUtil.Ingame;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.normalBgSprite = "TextFieldPanel";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.relativePosition = new Vector3(0, 0);


            // Scrollbar
            DropDown.listScrollbar = DropDown.AddUIComponent<UIScrollbar>();
            DropDown.listScrollbar.width = 12f;
            DropDown.listScrollbar.height = DropDown.listHeight;
            DropDown.listScrollbar.orientation = UIOrientation.Vertical;
            DropDown.listScrollbar.pivot = UIPivotPoint.TopRight;
            DropDown.listScrollbar.thumbPadding = new RectOffset(0, 0, 5, 5);
            DropDown.listScrollbar.minValue = 0;
            DropDown.listScrollbar.value = 0;
            DropDown.listScrollbar.AlignTo(DropDown, UIAlignAnchor.TopRight);
            DropDown.listScrollbar.autoHide = true; // false ?
            DropDown.listScrollbar.isVisible = false;

            UISlicedSprite tracSprite = DropDown.listScrollbar.AddUIComponent<UISlicedSprite>();
            tracSprite.relativePosition = Vector2.zero;
            tracSprite.autoSize = true;
            tracSprite.size = tracSprite.parent.size;
            tracSprite.fillDirection = UIFillDirection.Vertical;
            tracSprite.spriteName = "ScrollbarTrack";

            DropDown.listScrollbar.trackObject = tracSprite;

            UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width - 8;
            thumbSprite.spriteName = "ScrollbarThumb";
            DropDown.listScrollbar.thumbObject = thumbSprite;

            DropDown.eventAfterDropdownClose += DropdownClose;
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
        }

        private void DropdownClose(UICheckboxDropDown checkboxdropdown) {
            SetValue(GetCheckedFlags());
            UpdateText();
            UIButton button = DropDown.triggerButton as UIButton;
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
            string text = Enum.Format(enumType: EnumType, value: flags, "G");
            ApplyText(DropDown, text);
        }

        // private UIFontRenderer ObtainTextRenderer()
        static MethodInfo mObtainTextRenderer = AccessTools.DeclaredMethod(typeof(UIButton), "ObtainTextRenderer")
            ?? throw new Exception("mObtainTextRenderer is null");
        static UIFontRenderer ObtainTextRenderer(UIButton button) =>
            mObtainTextRenderer.Invoke(button, null) as UIFontRenderer;

        public static void ApplyText(UICheckboxDropDown dd, string text) {
            UIButton uibutton = (UIButton)dd.triggerButton;
            var r = ObtainTextRenderer(uibutton);
            float x = r.MeasureString(text).x;
            if( x > uibutton.width - 42 )
                uibutton.textHorizontalAlignment = UIHorizontalAlignment.Center;
            else
                uibutton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            if (x > uibutton.width - 21) {
                int n;
                for(n = 4; n < text.Length; ++n) {
                    string text2 = text.Substring(0, n) + "...";
                    float x2 = r.MeasureString(text2).x;
                    if (x2 > uibutton.width - 21) {
                        text = text.Substring(0, n - 1) + "...";
                        break;
                    }
                }

            }
            uibutton.text = text;
        }
    }
}
