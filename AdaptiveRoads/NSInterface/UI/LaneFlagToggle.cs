namespace AdaptiveRoads.NSInterface.UI {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using AdaptiveRoads.Data.NetworkExtensions;
    public class LaneFlagToggle : UICheckBoxExt {
        int laneIndex_;
        NetLaneExt.Flags flag_;
        static NetInfo Prefab => ARImplementation.Instance.Prefab;
        ARCustomFlags ARCustomFlags => ARImplementation.Instance.ARCustomFlags;

        public static LaneFlagToggle Add(UIPanel parent, int laneIndex, NetLaneExt.Flags flag) {
            var toggle = parent.AddUIComponent<LaneFlagToggle>();
            toggle.flag_ = flag;
            toggle.laneIndex_ = laneIndex;
            return toggle;
        }

        public void Refresh() {
            isChecked = ARCustomFlags.Lanes[laneIndex_].IsFlagSet(flag_);
            FitChildrenHorizontally(0);
        }

        public override void Start() {
            base.Start();
            var metadata = Prefab?.GetMetaData();
            string name = metadata.GetCustomLaneFlagName(flag_, laneIndex_);
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
            Refresh();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            try {
                base.OnCheckChanged(component, value);
                Log.Called($"Flag={flag_}", "value=" + value);
                base.OnCheckChanged(component, value);
                ARCustomFlags.Lanes[laneIndex_] = ARCustomFlags.Lanes[laneIndex_].SetFlags(flag_, value);
                Log.Info($"ARCustomFlags.Lanes[{laneIndex_}] became " + ARCustomFlags.Lanes[laneIndex_]);
                ARImplementation.Instance.Change();
            } catch(Exception ex) { ex.Log(); }
        }
    }
}
