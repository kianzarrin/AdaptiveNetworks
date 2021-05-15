namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.Assertion;

    public class FlagsPanel : UIPanel {
        const string SPRITES_FILE_NAME = "MainPanel.png";
        static string FileName => ModSettings.FILE_NAME;

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", FileName, 150, true);


        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;

        ushort segmentID_;
        ushort nodeID_;

        bool SegmentEndMode => segmentID_ != 0 && nodeID_ != 0;
        bool SegmentMode => segmentID_ != 0 && nodeID_ == 0;
        bool NodeMode => segmentID_ == 0 && nodeID_ != 0;


        public static FlagsPanel Create() =>
            UIView.GetAView().AddUIComponent(typeof(FlagsPanel)) as FlagsPanel;

        public static FlagsPanel Open(ushort segmentID, ushort nodeID) {
            var panel = Create();
            panel.segmentID_ = segmentID;
            panel.nodeID_ = nodeID;
            return panel;
        }

        public void Close() => DestroyImmediate(gameObject);

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void Start() {
            try {
                base.Start();
                LogCalled();
                backgroundSprite = "MenuPanel2";

                atlas = TextureUtil.Ingame;
                name = "ARMainPanel";

                absolutePosition = new Vector3(SavedX, SavedY);

                {
                    dragHandle_ = AddUIComponent<UIDragHandle>();
                    dragHandle_.height = 20;
                    dragHandle_.relativePosition = Vector3.zero;
                    dragHandle_.target = parent;

                    lblCaption_ = dragHandle_.AddUIComponent<UILabel>();
                    if (SegmentMode)
                        lblCaption_.text = "AR Custom Segment Flags";
                    else if (NodeMode)
                        lblCaption_.text = "AR Custom Node Flags";
                    else if (SegmentEndMode)
                        lblCaption_.text = "AR Custom SegmentEnd Flags";

                    lblCaption_.name = "AR_caption";
                }

                var body = AddPanel(this);
                body.name = "ContianerPanel";
                body.autoLayoutPadding = new RectOffset(3, 3, 3, 3);

                if (SegmentMode)
                    AddSegmentFlags(body);

                isVisible = true;
                Refresh();
            } catch (Exception ex) { ex.Log(); }
        }

        public void AddSegmentFlags(UIPanel parent) {
            AssertNotNull(parent,"parent");
            NetUtil.AssertSegmentValid(segmentID_);
            var info = segmentID_.ToSegment().Info;
            var net = info?.GetMetaData();
            AssertNotNull(net);

            foreach(var flag in net.UsedCustomFlags.Segment.GetFlags()) {
                SegmentFlagToggle.Add(parent, segmentID_, flag);
            }

            foreach (var lane in NetUtil.GetSortedLanes(segmentID_)) {
                var mask = GetLaneUsedCustomFlags(lane);
                if (mask != 0)
                    AddLaneFlags(parent, lane, mask);
            }
        }

        static NetLaneExt.Flags GetLaneUsedCustomFlags(LaneData lane) {
            NetLaneExt.Flags mask = 0;
            var props = (lane.LaneInfo.m_laneProps?.m_props).EmptyIfNull();
            foreach (var prop in props) {
                var metadata = prop.GetMetaData();
                if (metadata != null)
                    mask |= (metadata.LaneFlags.Required | metadata.LaneFlags.Forbidden);
            }
            return mask & NetLaneExt.Flags.CustomsMask;
        }

        public void AddLaneFlags(UIPanel parent, LaneData lane, NetLaneExt.Flags mask) {
            var lanePanel = AddPanel(parent);
            LaneCaptionButton.Add(lanePanel, lane);
            foreach(var flag in mask.GetFlags()) {
                LaneFlagToggle.Add(lanePanel, lane.LaneID, flag);
            }
        }



        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            Log.DebugWait("OnPositionChanged called", id: "OnPositionChanged called".GetHashCode(), seconds: 0.2f, copyToGameLog: false);

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.DebugWait("absolutePosition: " + absolutePosition, id: "absolutePosition: ".GetHashCode(), seconds: 0.2f, copyToGameLog: false);
        }

        void Refresh() {
            lblCaption_.relativePosition = new Vector2((width - lblCaption_.width) * 0.5f, 3);
            FitChildrenHorizontally(3);
            FitChildrenVertically(3);
            dragHandle_.width = width;
            Invalidate();
        }

        static UIPanel AddPanel(UIPanel parent) {
            Assertion.AssertNotNull(parent, "parent");
            int pad_horizontal = 0;
            int pad_vertical = 3;
            UIPanel newPanel = parent.AddUIComponent<UIPanel>();
            Assertion.AssertNotNull(newPanel, "newPanel");
            newPanel.autoLayoutDirection = LayoutDirection.Vertical;
            newPanel.autoFitChildrenHorizontally = true;
            newPanel.autoFitChildrenVertically = true;
            newPanel.autoLayoutPadding = new RectOffset(pad_horizontal, pad_horizontal, pad_vertical, pad_vertical);
            newPanel.padding = new RectOffset(pad_horizontal, pad_horizontal, pad_vertical, pad_vertical);

            return newPanel;
        }

        static UIPanel AddSpacePanel(UIPanel parent, int space) {
            var panel = parent.AddUIComponent<UIPanel>();
            panel.height = space;
            return panel;
        }
    }
}