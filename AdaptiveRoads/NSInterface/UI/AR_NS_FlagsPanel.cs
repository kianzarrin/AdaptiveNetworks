namespace AdaptiveRoads.NSInterface.UI{
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using static KianCommons.Assertion;
    using System.Linq;

    public class AR_NS_FlagsPanel : UIPanel {
        internal int HoveredLaneIndex { get; private set; } = -1;
        static ARImplementation Impl => ARImplementation.Instance;
        static NetInfo Prefab => Impl.Prefab;

        public override void Awake() {
            try {
                base.Awake();
                LogCalled();
                name = "AR_NS_MainPanel";
                atlas = TextureUtil.Ingame;

                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenHorizontally = true;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(0, 0, 0, 5);
                padding = default;
            } catch (Exception ex) {
                ex.Log();
            }
        }

        public override void Start() {
            try {
                base.Start();
                LogCalled();

                if(Impl.PrefabCustomFlags.Segment != default) {
                    AddSegmentFlags(this);
                }
                if(Impl.PrefabCustomFlags.SegmentEnd != default) {
                    AddSegmentEndFlags(this);
                }
                if(Impl.PrefabCustomFlags.Node != default) {
                    AddNodeFlags(this);
                }
                SizeChanged(default,default);
            } catch (Exception ex) { ex.Log(); }
        }

        public void AddSegmentFlags(UIPanel container) {
            LogCalled();
            AssertNotNull(container, "container");

            var subPanel = AddPanel(container, 1);
            var mask = Impl.PrefabCustomFlags.Segment;
            foreach (var flag in mask.ExtractPow2Flags()) {
                SegmentFlagToggle.Add(subPanel, flag);
            }

            foreach(int laneIndex in Prefab.m_sortedLanes) {
                var laneInfo = Prefab.m_lanes[laneIndex];
                var laneMask = laneInfo.GetUsedCustomFlagsLane();
                //Log.Info($"lane:{lane} laneMask:" + laneMask);
                if (laneMask != 0)
                    AddLaneFlags(container, laneIndex, laneMask);
            }
        }

        public void AddLaneFlags(UIPanel container, int laneIndex, NetLaneExt.Flags mask) {
            try {
                LogCalled(container, laneIndex, mask);
                
                var lanes = LaneHelpers.GetSimilarLanes(laneIndex, Prefab).ToArray();
                var laneContainer = LanePanelCollapsable.Add(container, laneIndex, mask);

                laneContainer.eventMouseEnter += (_, __) => HoveredLaneIndex = laneIndex;
                laneContainer.eventMouseLeave += (_, __) => {
                    if (HoveredLaneIndex == laneIndex)
                        HoveredLaneIndex = -1;
                };
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }


        public void AddSegmentEndFlags(UIPanel container) {
            AssertNotNull(container, "container");
            var subPanel = AddPanel(container, 1);

            var mask = Impl.PrefabCustomFlags.SegmentEnd;
            foreach (var flag in mask.ExtractPow2Flags()) {
                SegmentEndFlagToggle.Add(subPanel, flag: flag);
            }
        }

        public void AddNodeFlags(UIPanel container) {
            AssertNotNull(container, "container");
            var subPanel = AddPanel(container, 1);

            var mask = Impl.PrefabCustomFlags.Node;
            foreach (var flag in mask.ExtractPow2Flags()) {
                NodeFlagToggle.Add(subPanel, flag);
            }
        }

        private void SizeChanged(UIComponent _, Vector2 __) {
            eventSizeChanged -= SizeChanged;
            var lpcs = GetComponentsInChildren<LanePanelCollapsable>();
            if(lpcs.Length > 0) {
                foreach(var lpc in lpcs) lpc.Shrink();
                FitChildren();
                foreach(var lpc in lpcs) lpc.FitParent();
            }
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            eventSizeChanged += SizeChanged;
        }

        public static UIPanel AddPanel(UIPanel parent, int layoutPadding) {
            UIPanel panel = parent.AddUIComponent<UIPanel>();
            panel.autoLayout = true;
            panel.autoFitChildrenHorizontally = panel.autoFitChildrenVertically = true;
            panel.autoLayoutDirection = LayoutDirection.Vertical;
            panel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            panel.padding = default;
            return panel;
        }

        static UIPanel AddSpacePanel(UIPanel parent, int space) {
            var panel = parent.AddUIComponent<UIPanel>();
            panel.height = space;
            panel.width = 220;
            return panel;
        }
    }
}