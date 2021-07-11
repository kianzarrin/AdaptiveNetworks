namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using AdaptiveRoads.Manager;
    using KianCommons;

    public class LaneFlagToggle : UICheckBoxExt {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        uint laneID_;
        NetLaneExt.Flags flag_;
        public static LaneFlagToggle Add(UIPanel parent, uint laneID, NetLaneExt.Flags flag) {
            var toggle = parent.AddUIComponent<LaneFlagToggle>();
            toggle.flag_ = flag;
            toggle.laneID_ = laneID;
            ref var lane = ref man_.LaneBuffer[laneID];
            toggle.isChecked = lane.m_flags.IsFlagSet(flag);
            return toggle;
        }

        public override void Start() {
            base.Start();
            var segment = laneID_.ToLane().m_segment;
            string name = CustomFlagAttribute.GetName(flag_, segment.ToSegment().Info);
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SimulationManager.instance.AddAction(delegate () {
                ref var lane = ref man_.LaneBuffer[laneID_];
                var newFlags = lane.m_flags.SetFlags(flag_, value);
                if (lane.m_flags != newFlags) {
                    lane.m_flags = newFlags;
                    man_.UpdateSegment(laneID_.ToLane().m_segment);
                }
            });
        }
    }
}
