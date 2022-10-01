namespace AdaptiveRoads.NSInterface.UI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using AdaptiveRoads.Manager;
    using System;

    public class SegmentEndFlagToggle : UICheckBoxExt {
        NetSegmentEnd.Flags flag_;
        ANImplementation Impl => ANImplementation.Instance;
        ARCustomFlags ARCustomFlags => Impl.ARCustomFlags;
        public static void Add(UIPanel parent, NetSegmentEnd.Flags flag) {
            var toggle = parent.AddUIComponent<SegmentEndFlagToggle>();
            toggle.flag_ = flag;
        }

        public override void Start() {
            base.Start();
            string name = Impl.BasePrefab.GetSharedName(flag_);
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
            Refresh();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            try {
                Log.Called($"Flag={flag_}", "value=" + value);
                base.OnCheckChanged(component, value);
                ARCustomFlags.SegmentEnd = ARCustomFlags.SegmentEnd.SetFlags(flag_, value);
                Log.Info("ARCustomFlags.SegmentEnd became " + ARCustomFlags.SegmentEnd);
                Impl.Change();
            } catch(Exception ex) { ex.Log(); }
        }

        public void Refresh() {
            isChecked = ARCustomFlags.SegmentEnd.IsFlagSet(flag_);
            FitChildrenHorizontally();
        }
    }
}