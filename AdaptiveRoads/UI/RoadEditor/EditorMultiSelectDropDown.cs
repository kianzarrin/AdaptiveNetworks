using ColossalFramework.UI;
using KianCommons.UI;
using KianCommons;
using UnityEngine;
using static KianCommons.ReflectionHelpers;
using System;

namespace AdaptiveRoads.UI.RoadEditor {
    class EditorMultiSelectDropDown : UICheckboxDropDown {
        public UIScrollablePanel Popup => GetFieldValue(this, "m_Popup") as UIScrollablePanel;

        public override void Awake() {
            try {
                base.Awake();
                LogCalled();
                Init(this);
                Show();

                //dd.eventDropdownOpen += OnDropDownOpen;
                //triggerBtn.buttonsMask |= UIMouseButton.Right;
                LogSucceeded();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        //public override void Start() {
        //    try {

        //        this.relativePosition = new Vector2(parent.width - this.width, 28);
        //        base.Start();
        //    } catch(Exception ex) {
        //        Log.Exception(ex);
        //    }
        //}

        internal static void Init(UICheckboxDropDown dd) {
            try {
                LogCalled();
                dd.size = new Vector2(370, 22);
                dd.relativePosition = new Vector2(0, 28);
                dd.pivot = UIPivotPoint.TopCenter;
                dd.anchor = UIAnchorStyle.CenterHorizontal;
                dd.verticalAlignment = UIVerticalAlignment.Middle;
                dd.horizontalAlignment = UIHorizontalAlignment.Center;
                dd.builtinKeyNavigation = true;

                dd.atlas = TextureUtil.InMapEditor;
                dd.normalBgSprite = "TextFieldPanel";
                dd.uncheckedSprite = "check-unchecked";
                dd.checkedSprite = "check-checked";

                dd.listBackground = "GenericPanelWhite";
                dd.listWidth = 188;
                dd.listHeight = 300;
                dd.clampListToScreen = true;
                dd.listPosition = UICheckboxDropDown.PopupListPosition.Automatic;

                dd.itemHeight = 25;
                dd.itemHover = "ListItemHover";
                dd.itemHighlight = "ListItemHighlight";

                dd.popupColor = Color.black;
                dd.popupTextColor = Color.white;

                dd.triggerButton = dd.AddUIComponent<UIButton>();
                UIButton button = dd.triggerButton as UIButton;
                button.size = dd.size;
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
                dd.listScrollbar = dd.AddUIComponent<UIScrollbar>();
                dd.listScrollbar.width = 12f;
                dd.listScrollbar.height = dd.listHeight;
                dd.listScrollbar.orientation = UIOrientation.Vertical;
                dd.listScrollbar.pivot = UIPivotPoint.TopRight;
                dd.listScrollbar.thumbPadding = new RectOffset(0, 0, 5, 5);
                dd.listScrollbar.minValue = 0;
                dd.listScrollbar.value = 0;
                dd.listScrollbar.incrementAmount = 60;
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

                //dd.eventDropdownOpen += OnDropDownOpen;
                //triggerBtn.buttonsMask |= UIMouseButton.Right;
                LogSucceeded();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        //protected override void OnMouseDown(UIMouseEventParameter p) {
        //    base.OnMouseDown(p);
        //    HandleMouseDown(p);
        //}

        //public virtual void OnDropDownOpen(
        //    UICheckboxDropDown checkboxdropdown, UIScrollablePanel popup, ref bool overridden)
        //    => popup.eventMouseDown += (_, p) => HandleMouseDown(p);

        //void HandleMouseDown(UIMouseEventParameter p) {
        //    if(p.buttons == UIMouseButton.Right)
        //        ClosePopup();
        //}
    }
}
