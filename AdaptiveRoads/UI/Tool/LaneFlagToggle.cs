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
            return toggle;
        }

        public override void Start() {
            base.Start();
            this.Label = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SimulationManager.instance.AddAction(delegate () {
                ref var lane = ref man_.LaneBuffer[laneID_];
                lane.m_flags = lane.m_flags.SetFlags(flag_, value);
                man_.UpdateSegment(laneID_.ToLane().m_segment);
            });
        }
    }
}
