namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.Assertion;
    using System.Linq;
    using AdaptiveRoads.Data.NetworkExtensions;

    public class FlagsPanel : UIPanel {
        static string FileName => ModSettings.FILE_NAME;

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", FileName, 150, true);


        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;

        ushort []segmentIDs_;
        ushort segmentID_;
        ushort nodeID_;

        internal uint HighlighLaneID { get; private set; }

        bool SegmentEndMode => segmentID_ != 0 && nodeID_ != 0;
        bool SegmentMode => segmentID_ != 0 && nodeID_ == 0;
        bool MultiSegmentMode => SegmentMode && !segmentIDs_.IsNullorEmpty();

        bool NodeMode => segmentID_ == 0 && nodeID_ != 0;


        public static FlagsPanel Create() =>
            UIView.GetAView().AddUIComponent(typeof(FlagsPanel)) as FlagsPanel;

        public static FlagsPanel Open(ushort nodeID, ushort segmentID, ushort []selectedSegmentIDs) {
            var panel = Create();
            panel.segmentID_ = segmentID;
            panel.segmentIDs_ = selectedSegmentIDs;
            panel.nodeID_ = nodeID;
            return panel;
        }

        public void Close() => DestroyImmediate(gameObject);

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void Awake() {
            try {
                base.Awake();
                LogCalled();
                name = "ARMainPanel";

                backgroundSprite = "MenuPanel2";
                atlas = TextureUtil.Ingame;

                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenHorizontally = true;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(3, 3, 3, 3);
                padding = new RectOffset(3, 3, 3, 3);
            } catch (Exception ex) {
                ex.Log();
            }

        }

        public override void Start() {
            try {
                base.Start();
                LogCalled();

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

                if(MultiSegmentMode)
                    AddSegmentFlags(this);
                else if (SegmentMode)
                    AddSegmentFlags(this);
                else if (SegmentEndMode)
                    AddSegmentEndFlags(this);
                else if (NodeMode)
                    AddNodeFlags(this);

                AddSpacePanel(this, 5);

                isVisible = true;
                Refresh();
            } catch (Exception ex) { ex.Log(); }
        }

        public void AddSegmentFlags(UIPanel container) {
            LogCalled();
            AssertNotNull(container, "container");
            NetUtil.AssertSegmentValid(segmentID_);
            var mask = ARTool.GetUsedFlagsSegment(segmentID_).Segment;
            foreach (var flag in mask.ExtractPow2Flags()) {
                SegmentFlagToggle.Add(container, segmentID_, segmentIDs_, flag);
            }

            foreach (var lane in NetUtil.GetSortedLanes(segmentID_)) {
                var laneMask = lane.LaneInfo.GetUsedCustomFlagsLane();
                //Log.Info($"lane:{lane} laneMask:" + laneMask);
                if (laneMask != 0)
                    AddLaneFlags(container, lane, laneMask);
            }
        }

        public void AddLaneFlags(UIPanel container, LaneData lane, NetLaneExt.Flags mask) {
            try {
                LogCalled(container, lane.LaneID, mask);
                
                AddSpacePanel(container, 6);
                var lanes = Util.GetSimilarLanes(lane, segmentIDs_).ToArray();
                var laneContainer = LanePanelCollapsable.Add(container, lane, lanes, mask);

                laneContainer.eventMouseEnter += (_, __) => HighlighLaneID = lane.LaneID;
                laneContainer.eventMouseLeave += (_, __) => {
                    if (HighlighLaneID == lane.LaneID)
                        HighlighLaneID = 0;
                };
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }


        public void AddSegmentEndFlags(UIPanel container) {
            AssertNotNull(container, "container");
            NetUtil.AssertSegmentValid(segmentID_);
            Assertion.Assert(NetUtil.IsNodeValid(nodeID_), $"IsNodeValid({nodeID_})");

            var mask = ARTool.GetUsedFlagsSegmentEnd(segmentID: segmentID_, nodeID: nodeID_);
            foreach (var flag in mask.ExtractPow2Flags()) {
                SegmentEndFlagToggle.Add(container, segmentID: segmentID_, nodeID: nodeID_, flag: flag);
            }
        }

        public void AddNodeFlags(UIPanel parent) {
            AssertNotNull(parent, "parent");
            Assertion.Assert(NetUtil.IsNodeValid(nodeID_), $"IsNodeValid({nodeID_})");

            var mask = ARTool.GetUsedFlagsNode(nodeID_);
            foreach (var flag in mask.ExtractPow2Flags()) {
                NodeFlagToggle.Add(parent, nodeID_, flag);
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
            dragHandle_.FitChildren();
            dragHandle_.width = Mathf.Max(width, dragHandle_.width);
            dragHandle_.height = 32;
            lblCaption_.anchor = UIAnchorStyle.CenterHorizontal |  UIAnchorStyle.CenterVertical;
            FitChildren();
            foreach (var lpc in GetComponentsInChildren<LanePanelCollapsable>()) {
                lpc.FitParent();
            }
            Invalidate();
        }

        static UIPanel AddSpacePanel(UIPanel parent, int space) {
            var panel = parent.AddUIComponent<UIPanel>();
            panel.height = space;
            panel.width = 1;
            return panel;
        }
    }
}