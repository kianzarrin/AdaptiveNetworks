using ColossalFramework.UI;
using KianCommons.UI;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class ListBox : UIListBox {
        public override void Awake() {
            base.Awake();
            itemPadding = new RectOffset(15, 0, 5, 0);
            itemHeight = 27;

            color = new Color32(50, 62, 70, 255);
            // itemTextColor = new Color32(174, 197, 211,255);
            atlas = TextureUtil.Ingame;
            normalBgSprite = "GenericPanel";
            itemHover = "ListItemHover";
            itemHighlight = "ListItemHighlight";
            multilineItems = false;

            textScale = 1.25f;
        }

        /// <summary>
        /// call this after the size of the list box has been determined.
        /// </summary>
        public void AddScrollBar() {
            scrollbar = this.AddUIComponent<UIScrollbar>();
            scrollbar.width = 30f;
            scrollbar.height = height;
            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.pivot = UIPivotPoint.TopRight;
            scrollbar.thumbPadding = new RectOffset(0, 0, 5, 5);
            scrollbar.minValue = 0;
            scrollbar.value = 0;
            scrollbar.incrementAmount = 60;
            scrollbar.isVisible = true;
            scrollbar.AlignTo(this, UIAlignAnchor.TopRight);

            UISlicedSprite tracSprite = scrollbar.AddUIComponent<UISlicedSprite>();
            tracSprite.relativePosition = Vector2.zero;
            tracSprite.autoSize = true;
            tracSprite.size = scrollbar.size;
            tracSprite.fillDirection = UIFillDirection.Vertical;
            tracSprite.spriteName = "ScrollbarTrack";

            scrollbar.trackObject = tracSprite;

            UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = tracSprite.width - 8;
            thumbSprite.spriteName = "ScrollbarThumb";
            scrollbar.thumbObject = thumbSprite;

            // TODO add this to see if it enables us to add UIScrollbar before setting the size.
            //eventSizeChanged += (_, __) => {
            //    scrollbar.AlignTo(this, UIAlignAnchor.TopRight);
            //    scrollbar.pivot = UIPivotPoint.TopRight;
            //    scrollbar.anchor = UIAnchorStyle.Top | UIAnchorStyle.Bottom;
            //    scrollbar.height = height;
            //    tracSprite.size = scrollbar.size;
            //    thumbSprite.width = tracSprite.width - 8;
            //};
        }
    }
}
