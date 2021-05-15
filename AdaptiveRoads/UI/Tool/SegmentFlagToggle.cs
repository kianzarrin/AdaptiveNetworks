namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using AdaptiveRoads.Manager;

    public class SegmentFlagToggle : UICheckBoxExt {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        ushort segmentID_;
        NetSegmentExt.Flags flag_;
        public static SegmentFlagToggle Add(UIPanel parent, ushort segmentID, NetSegmentExt.Flags flag) {
            var toggle = parent.AddUIComponent<SegmentFlagToggle>();
            toggle.flag_ = flag;
            toggle.segmentID_ = segmentID;
            return toggle;
        }

        public override void Start() {
            base.Start();
            this.Label = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SimulationManager.instance.AddAction(delegate () {
                ref var segment = ref man_.SegmentBuffer[segmentID_];
                segment.m_flags = segment.m_flags.SetFlags(flag_, value);
                man_.UpdateSegment(segmentID_);
            });
        }
    }
}
