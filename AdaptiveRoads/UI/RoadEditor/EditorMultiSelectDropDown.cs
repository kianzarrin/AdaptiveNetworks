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
                this.size = new Vector2(370, 22);
                this.relativePosition = new Vector2(width - this.width, 28);
                this.verticalAlignment = UIVerticalAlignment.Middle;
                this.horizontalAlignment = UIHorizontalAlignment.Center;
                this.builtinKeyNavigation = true;

                this.atlas = TextureUtil.InMapEditor;
                this.normalBgSprite = "TextFieldPanel";
                this.uncheckedSprite = "check-unchecked";
                this.checkedSprite = "check-checked";

                this.listBackground = "GenericPanelWhite";
                this.listWidth = 188;
                this.listHeight = 300;
                this.clampListToScreen = true;
                this.listPosition = UICheckboxDropDown.PopupListPosition.Automatic;

                this.itemHeight = 25;
                this.itemHover = "ListItemHover";
                this.itemHighlight = "ListItemHighlight";

                this.popupColor = Color.black;
                this.popupTextColor = Color.white;

                UIButton triggerBtn = this.AddUIComponent<UIButton>();
                this.triggerButton = triggerBtn;
                triggerBtn.size = this.size;
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
                this.listScrollbar = this.AddUIComponent<UIScrollbar>();
                this.listScrollbar.width = 12f;
                this.listScrollbar.height = this.listHeight;
                this.listScrollbar.orientation = UIOrientation.Vertical;
                this.listScrollbar.pivot = UIPivotPoint.TopRight;
                this.listScrollbar.thumbPadding = new RectOffset(0, 0, 5, 5);
                this.listScrollbar.minValue = 0;
                this.listScrollbar.value = 0;
                this.listScrollbar.incrementAmount = 60;
                this.listScrollbar.AlignTo(this, UIAlignAnchor.TopRight);
                this.listScrollbar.autoHide = true; // false ?
                this.listScrollbar.isVisible = false;

                UISlicedSprite tracSprite = this.listScrollbar.AddUIComponent<UISlicedSprite>();
                tracSprite.relativePosition = Vector2.zero;
                tracSprite.autoSize = true;
                tracSprite.size = tracSprite.parent.size;
                tracSprite.fillDirection = UIFillDirection.Vertical;
                tracSprite.spriteName = "ScrollbarTrack";

                this.listScrollbar.trackObject = tracSprite;

                UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
                thumbSprite.relativePosition = Vector2.zero;
                thumbSprite.fillDirection = UIFillDirection.Vertical;
                thumbSprite.autoSize = true;
                thumbSprite.width = thumbSprite.parent.width - 8;
                thumbSprite.spriteName = "ScrollbarThumb";
                this.listScrollbar.thumbObject = thumbSprite;

                this.eventDropdownOpen += OnDropDownOpen;
                triggerBtn.buttonsMask |= UIMouseButton.Right;
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        protected override void OnMouseDown(UIMouseEventParameter p) {
            base.OnMouseDown(p);
            HandleMouseDown(p);
        }

        public virtual void OnDropDownOpen(
            UICheckboxDropDown checkboxdropdown, UIScrollablePanel popup, ref bool overridden)
            => popup.eventMouseDown += (_, p) => HandleMouseDown(p);

        void HandleMouseDown(UIMouseEventParameter p) {
            if(p.buttons == UIMouseButton.Right)
                ClosePopup();
        }
    }
}
