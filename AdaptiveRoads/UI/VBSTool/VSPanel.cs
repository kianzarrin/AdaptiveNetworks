namespace AdaptiveRoads.UI.VBSTool {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI.RoadEditor;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class VSPanel : UIWindow {
        static string FileName => ModSettings.FILE_NAME;

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        public static readonly SavedFloat SavedX = new SavedFloat(
            "VSPanelX", FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "VSPanelY", FileName, 150, true);

        public override Vector2 Position {
            get => new Vector2(SavedX.value, SavedY.value);
            set {
                SavedX.value = value.x;
                SavedY.value = value.y;
            }
        }

        ushort segmentID_;

        public static VSPanel Open(ushort segmentID) {
            var panel = Create<VSPanel>();
            panel.Init(segmentID);
            return panel;
        }

        public void Close() => Destroy(Wrapper.gameObject);

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void Awake() {
            try {
                base.Awake();
                Log.Called();
                Title = "Virtual Stops";
            } catch (Exception ex) { ex.Log(); }
        }

        public void Init(ushort segmentID) {
            segmentID_ = segmentID;
            var flags = (NetSegmentFlags)segmentID_.ToSegment().m_flags & NetSegmentFlags.StopAll; ;
            foreach (var flag in new[]{
                    NetSegmentFlags.BusStopLeft,
                    NetSegmentFlags.BusStopRight,
                    NetSegmentFlags.TramStopLeft,
                    NetSegmentFlags.TramStopRight,
                }) {
                var cb = Container.AddUIComponent<AutoSizeCheckbox>();
                cb.Label = flag.ToString();
                cb.isChecked = flags.IsFlagSet(flag);
                cb.objectUserData = (NetSegment.Flags)flag;
                cb.eventCheckChanged += Cb_eventCheckChanged;
                cb.eventMouseHover += Cb_eventMouseHover;
                cb.eventMouseEnter += Cb_eventMouseHover;
                cb.eventMouseLeave += Cb_eventMouseLeave;
            }
        }
        private void Cb_eventMouseLeave(UIComponent component, UIMouseEventParameter eventParam) {
            Overlay.HoveredInstace = default;
        }

        private void Cb_eventMouseHover(UIComponent component, UIMouseEventParameter eventParam) {
            Log.Called();
            ref NetSegment segment = ref segmentID_.ToSegment();
            NetSegment.Flags flag = (NetSegment.Flags)component.objectUserData;
            bool leftFlag = flag.IsFlagSet(NetSegment.Flags.StopLeft | NetSegment.Flags.StopLeft2);
            bool rightFlag = flag.IsFlagSet(NetSegment.Flags.StopRight | NetSegment.Flags.StopRight2);
            bool busFlag = flag.IsFlagSet(NetSegment.Flags.StopBoth);
            bool tramFlag = flag.IsFlagSet(NetSegment.Flags.StopBoth2);
            foreach (var lane in new LaneDataIterator(segmentID_)) {
                bool leftLane = lane.LaneInfo.m_position < 0;
                leftLane ^= segment.IsInvert();
                var stopType = lane.LaneInfo.m_stopType;
                bool busLane = stopType.IsFlagSet(VehicleInfo.VehicleType.Car);
                bool tramLane = stopType.IsFlagSet(VehicleInfo.VehicleType.Tram);

                bool show = leftLane && leftFlag || !leftLane && rightFlag;
                show &= busFlag && busLane || tramFlag && tramLane;
                Log.Debug("show:" + show);
                if (show) {
                    Overlay.HoveredInstace = new InstanceID { NetLane = lane.LaneID };
                }
            }
        }

        private void Cb_eventCheckChanged(UIComponent component, bool value) {
            NetSegment.Flags flag = (NetSegment.Flags)component.objectUserData;
            ref NetSegment segment = ref segmentID_.ToSegment();
            segment.m_flags = segment.m_flags.SetFlags(flag, value);
            UpdateLaneFlags();
            NetworkExtensionManager.Instance.UpdateSegment(segmentID_);
        }

    private void UpdateLaneFlags() {
            ref NetSegment segment = ref segmentID_.ToSegment();
            var segmentFlags = segment.m_flags;
            foreach (var lane in new LaneDataIterator(segmentID_)) {
                var leftLane = lane.LaneInfo.m_position < 0;
                leftLane ^= segment.IsInvert();
                var stopType = lane.LaneInfo.m_stopType;
                bool busLane = stopType.IsFlagSet(VehicleInfo.VehicleType.Car);
                bool tramLane = stopType.IsFlagSet(VehicleInfo.VehicleType.Tram);

                bool stop =
                    leftLane & segmentFlags.IsFlagSet(NetSegment.Flags.StopLeft) ||
                    !leftLane & segmentFlags.IsFlagSet(NetSegment.Flags.StopRight);
                stop &= busLane;

                bool stop2 =
                    leftLane & segmentFlags.IsFlagSet(NetSegment.Flags.StopLeft2) ||
                    !leftLane & segmentFlags.IsFlagSet(NetSegment.Flags.StopRight2);
                stop2 &= tramLane;

                lane.Flags = lane.Flags.SetFlags(NetLane.Flags.Stop, stop);
                lane.Flags = lane.Flags.SetFlags(NetLane.Flags.Stop2, stop2);
            }
        }
    }
}