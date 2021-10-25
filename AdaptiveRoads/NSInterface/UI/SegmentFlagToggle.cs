namespace AdaptiveRoads.NSInterface.UI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using NetworkSkins.Helpers;
    using System;

    public class SegmentFlagToggle : UICheckBoxExt {
        private NetSegmentExt.Flags flag_;
        ARImplementation Impl => ARImplementation.Instance;
        ARCustomFlags ARCustomFlags => Impl.ARCustomFlags;

        public static SegmentFlagToggle Add(UIPanel parent, NetSegmentExt.Flags flag) {
            var toggle = parent.AddUIComponent<SegmentFlagToggle>();
            toggle.flag_ = flag;
            return toggle;
        }

        public override void Start() {
            base.Start();
            string name = CustomFlagAttribute.GetName(flag_, Impl.Prefab);
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
            FitChildrenHorizontally();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            try {
                Log.Called($"Flag={flag_}", "value="+value);
                base.OnCheckChanged(component, value);
                ARCustomFlags.Segment = ARCustomFlags.Segment.SetFlags(flag_, value);
                Log.Info("ARCustomFlags.Segment became " + ARCustomFlags.Segment);
                Impl.OnControllerChanged();
            } catch(Exception ex) { ex.Log(); }
        }

        public void Refresh(NetSegmentExt.Flags flags) {
            isChecked = flags.IsFlagSet(flag_);
            FitChildrenHorizontally(0);
        }
    }
}
