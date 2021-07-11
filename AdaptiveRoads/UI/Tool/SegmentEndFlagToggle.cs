namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
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
            ref var segmentEnd = ref man_.GetSegmentEnd(segmentID, nodeID);
            toggle.isChecked = segmentEnd.m_flags.IsFlagSet(flag);
        }

        public override void Start() {
            base.Start();
            string name = CustomFlagAttribute.GetName(flag_, segmentID_.ToSegment().Info);
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SimulationManager.instance.AddAction(delegate () {
                ref var segmentEnd = ref man_.GetSegmentEnd(segmentID_, nodeID_);
                var newFlags = segmentEnd.m_flags.SetFlags(flag_, value);
                if (segmentEnd.m_flags != newFlags) {
                    segmentEnd.m_flags = newFlags;
                    man_.UpdateNode(nodeID_);
                }
            });
        }
    }
}