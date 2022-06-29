namespace AdaptiveRoads.UI.VBSTool {
    using ColossalFramework.UI;
    using System;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;
    using ColossalFramework;
    using AdaptiveRoads.Manager;

    public class VBSButton :UIButton {
        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            size = new Vector2(90f, 30f);
            textScale = 1;
            normalBgSprite = "ButtonMenu";
            hoveredBgSprite = "ButtonMenuHovered";
            pressedBgSprite = "ButtonMenuPressed";
            disabledBgSprite = "ButtonMenuDisabled";
            canFocus = false;
        }
    }

    public class VBSPanel : UIPanel {
        private UILabel seginput;
        private UITitleBar m_title;
        private UIButton segNoneButton;
        private UIButton segLeftButton;
        private UIButton segRightButton;
        private UIButton segAllButton;
        private ushort segmentID;

        public override void OnDestroy() {
            base.OnDestroy();
            this.SetAllDeclaredFieldsToNull();
        }

        public static VBSPanel Open(ushort segmentID) {
            try {
                UIView view = UIView.GetAView();
                var ret = view.AddUIComponent<VBSPanel>();
                ret.segmentID = segmentID;
                ret.seginput.text = segmentID.ToString();
                return ret;
            } catch (Exception ex) { ex.Log(); }
            return null;
        }

        public void Close() => Destroy(gameObject);
        

        public override void Awake() {
            try {
                base.Awake();
                name = "VirtualBusStopsMain";
                atlas = TextureUtil.Ingame;
                backgroundSprite = "MenuPanel2";
                relativePosition = new Vector2(1275, 0);
                canFocus = true;
                size = new Vector2(340, 140);

                m_title = AddUIComponent<UITitleBar>();
                m_title.title = "Virtual Bus Stops";
                m_title.closeButton.isVisible = false;

                UILabel seglabel = AddUIComponent<UILabel>();
                seglabel.text = "Segment ID:";
                seglabel.autoSize = false;
                seglabel.width = 105f;
                seglabel.height = 20f;
                seglabel.relativePosition = new Vector2(10, 55);

                seginput = this.AddUIComponent<UILabel>();
                seginput.text = "";
                seginput.width = 100f;
                seginput.height = 25f;
                seginput.padding = new RectOffset(6, 6, 6, 6);
                seginput.relativePosition = new Vector3(120, 50);

                segNoneButton = AddUIComponent<VBSButton>();
                segNoneButton.text = "None";
                segNoneButton.relativePosition = new Vector2(240, 50);
                segNoneButton.width = 90;
                segNoneButton.eventClick += (_, _) => ResetFlags();


                segLeftButton = AddUIComponent<VBSButton>();
                segLeftButton.text = "StopLeft";
                segLeftButton.relativePosition = new Vector2(10, 90);
                segLeftButton.width = 100;
                segLeftButton.eventClick += (_, _) => SetStopFlags(NetSegment.Flags.StopLeft);

                segRightButton = AddUIComponent<VBSButton>();
                segRightButton.text = "StopRight";
                segRightButton.relativePosition = new Vector2(115, 90);
                segRightButton.width = 105;

                segRightButton.eventClick += (_, _) => SetStopFlags(NetSegment.Flags.StopRight);

                segAllButton = AddUIComponent<VBSButton>();
                segAllButton.text = "StopBoth";
                segAllButton.textScale = 1f;
                segAllButton.relativePosition = new Vector2(225, 90);
                segAllButton.width = 110;
                segAllButton.eventClick += (_, _) => SetStopFlags(NetSegment.Flags.StopBoth);
            } catch (Exception ex) { ex.Log(); }
        }

        private void ResetFlags() {
            try {
                var flags = segmentID.ToSegment().m_flags;
                flags.ClearFlags(NetSegment.Flags.StopAll);
                flags.ClearFlags(NetSegment.Flags.StopLeft);
                flags.ClearFlags(NetSegment.Flags.StopRight);
                NetManager.instance.UpdateSegmentFlags(segmentID);
                NetworkExtensionManager.Instance.UpdateSegment(segmentID);
            } catch (Exception ex) { ex.Log(); }
        }

        private void SetStopFlags(NetSegment.Flags flags) {
            try {
                ResetFlags();
                segmentID.ToSegment().m_flags |= flags;
                NetManager.instance.UpdateSegmentFlags(segmentID);
                NetworkExtensionManager.Instance.UpdateSegment(segmentID);
            } catch (Exception ex) { ex.Log(); }
        }
    }
}
