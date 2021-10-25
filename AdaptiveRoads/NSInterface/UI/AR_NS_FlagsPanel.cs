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
                autoLayoutPadding = new RectOffset(0, 0, 1, 1);
                padding = new RectOffset(0, 0, 6, 0);
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
                    AddSpacePanel(this, 6);
                }
                if(Impl.PrefabCustomFlags.SegmentEnd != default) {
                    AddSegmentEndFlags(this);
                    AddSpacePanel(this, 6);
                }
                if(Impl.PrefabCustomFlags.Node != default) {
                    AddNodeFlags(this);
                    AddSpacePanel(this, 6);
                }
                RefreshLayout();
            } catch (Exception ex) { ex.Log(); }
        }

        public void AddSegmentFlags(UIPanel container) {
            LogCalled();
            AssertNotNull(container, "container");
            var mask = Impl.PrefabCustomFlags.Segment;
            foreach (var flag in mask.ExtractPow2Flags()) {
                SegmentFlagToggle.Add(container, flag);
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
                
                AddSpacePanel(container, 6);
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

            var mask = Impl.PrefabCustomFlags.SegmentEnd;
            foreach (var flag in mask.ExtractPow2Flags()) {
                SegmentEndFlagToggle.Add(container, flag: flag);
            }
        }

        public void AddNodeFlags(UIPanel parent) {
            AssertNotNull(parent, "parent");

            var mask = Impl.PrefabCustomFlags.Node;
            foreach (var flag in mask.ExtractPow2Flags()) {
                NodeFlagToggle.Add(parent, flag);
            }
        }

        void RefreshLayout() {
            FitChildren();
            foreach (var lpc in GetComponentsInChildren<LanePanelCollapsable>()) {
                lpc.FitParent();
            }
            Invalidate();
        }

        static UIPanel AddSpacePanel(UIPanel parent, int space) {
            var panel = parent.AddUIComponent<UIPanel>();
            panel.height = space;
            panel.width = 220;
            return panel;
        }
    }
}