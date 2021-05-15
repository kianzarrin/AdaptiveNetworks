namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using AdaptiveRoads.Manager;

    public class SegmentEndFlagToggle : UICheckBoxExt {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        ushort segmentID_;
        ushort nodeID_;
        NetSegmentEnd.Flags flag_;
        public static void Add(UIPanel parent, ushort segmentID, ushort nodeID, NetSegmentEnd.Flags flag) {
            var toggle = parent.AddUIComponent<SegmentEndFlagToggle>();
            toggle.flag_ = flag;
            toggle.segmentID_ = segmentID;
            toggle.nodeID_ = nodeID;
        }

        public override void Start() {
            base.Start();
            this.Label = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SimulationManager.instance.AddAction(delegate () {
                ref var segmentEnd = ref man_.GetSegmentEnd(segmentID_, nodeID_);
                segmentEnd.m_flags = segmentEnd.m_flags.SetFlags(flag_, value);
                man_.UpdateNode(nodeID_);
            });
        }
    }
}
