using ColossalFramework.UI;
using KianCommons.UI;
using KianCommons;
using UnityEngine;
using static KianCommons.ReflectionHelpers;
using System;

namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    class EditorMultiSelectDropDown : UICheckboxDropDown {
        //public UIScrollablePanel Popup => GetFieldValue(this, "m_Popup") as UIScrollablePanel;

        public override void Awake() {
            try {
                base.Awake();
                LogCalled();

                Init(this); 

                LogSucceeded();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        internal static void Init(UICheckboxDropDown dd) {
            try {
                LogCalled();
                dd.size = new Vector2(370, 22);
                dd.verticalAlignment = UIVerticalAlignment.Middle;
                dd.horizontalAlignment = UIHorizontalAlignment.Center;
                dd.builtinKeyNavigation = true;

                dd.atlas = TextureUtil.InMapEditor;
                dd.normalBgSprite = "TextFieldPanel";
                dd.uncheckedSprite = "check-unchecked";
                dd.checkedSprite = "check-checked";

                dd.listBackground = "GenericPanelWhite";
                dd.listWidth = 188;
                dd.listHeight = 1000;
                dd.clampListToScreen = true;
                dd.listPosition = UICheckboxDropDown.PopupListPosition.Automatic;

                dd.itemHeight = 25;
                dd.itemHover = "ListItemHover";
                dd.itemHighlight = "ListItemHighlight";

                dd.popupColor = Color.black;
                dd.popupTextColor = Color.white;

                dd.triggerButton = dd.AddUIComponent<UIButton>();
                UIButton triggerBtn = dd.triggerButton as UIButton;
                triggerBtn.size = dd.size;
                triggerBtn.textVerticalAlignment = UIVerticalAlignment.Middle;
                triggerBtn.textHorizontalAlignment = UIHorizontalAlignment.Left;
                triggerBtn.atlas = TextureUtil.Ingame;
                triggerBtn.normalFgSprite = "IconDownArrow";
                triggerBtn.hoveredFgSprite = "IconDownArrowHovered";
                triggerBtn.pressedFgSprite = "IconDownArrowPressed";
                triggerBtn.normalBgSprite = "TextFieldPanel";
                triggerBtn.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                triggerBtn.horizontalAlignment = UIHorizontalAlignment.Right;
                triggerBtn.verticalAlignment = UIVerticalAlignment.Middle;
                triggerBtn.relativePosition = new Vector3(0, 0);

                // Scrollbar
                dd.listScrollbar = dd.AddUIComponent<UIScrollbar>();
                dd.listScrollbar.width = 12f;
                dd.listScrollbar.height = dd.listHeight;
                dd.listScrollbar.orientation = UIOrientation.Vertical;
                dd.listScrollbar.pivot = UIPivotPoint.TopRight;
                dd.listScrollbar.thumbPadding = new RectOffset(0, 0, 5, 5);
                dd.listScrollbar.minValue = 0;
                dd.listScrollbar.value = 0;
                dd.listScrollbar.incrementAmount = 90;
                dd.listScrollbar.AlignTo(dd, UIAlignAnchor.TopRight);
                dd.listScrollbar.autoHide = true; // false ?
                dd.listScrollbar.isVisible = false;

                UISlicedSprite tracSprite = dd.listScrollbar.AddUIComponent<UISlicedSprite>();
                tracSprite.relativePosition = Vector2.zero;
                tracSprite.autoSize = true;
                tracSprite.size = tracSprite.parent.size;
                tracSprite.fillDirection = UIFillDirection.Vertical;
                tracSprite.spriteName = "ScrollbarTrack";

                dd.listScrollbar.trackObject = tracSprite;

                UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
                thumbSprite.relativePosition = Vector2.zero;
                thumbSprite.fillDirection = UIFillDirection.Vertical;
                thumbSprite.autoSize = true;
                thumbSprite.width = thumbSprite.parent.width - 8;
                thumbSprite.spriteName = "ScrollbarThumb";
                dd.listScrollbar.thumbObject = thumbSprite;

                dd.eventDropdownOpen += OnDropDownOpen;
                triggerBtn.buttonsMask |= UIMouseButton.Right;
                dd.eventMouseDown += (_, p) => HandleMouseDown(dd, p);

                LogSucceeded();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        static  void OnDropDownOpen(
            UICheckboxDropDown checkboxdropdown, UIScrollablePanel popup, ref bool overridden)
            => popup.eventMouseDown += (_, p) => HandleMouseDown(checkboxdropdown, p);

        static void HandleMouseDown(UICheckboxDropDown c, UIMouseEventParameter p) {
            if(p.buttons == UIMouseButton.Right) {
                c.ClosePopup();
                p.Use();
            }
        }
    }
}
