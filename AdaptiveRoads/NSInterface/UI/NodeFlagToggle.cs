namespace AdaptiveRoads.NSInterface.UI {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;

    public class NodeFlagToggle : UICheckBoxExt {
        NetNodeExt.Flags flag_;
        ANImplementation Impl => ANImplementation.Instance;
        ARCustomFlags ARCustomFlags => Impl.ARCustomFlags;

        public static NodeFlagToggle Add(UIPanel parent, NetNodeExt.Flags flag) {
            var toggle = parent.AddUIComponent<NodeFlagToggle>();
            toggle.flag_ = flag;
            return toggle;
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
                ARCustomFlags.Node = ARCustomFlags.Node.SetFlags(flag_, value);
                Log.Info("ARCustomFlags.Node became " + ARCustomFlags.Node);
                Impl.Change();
            } catch(Exception ex) { ex.Log(); }
        }

        public void Refresh() {
            isChecked = ARCustomFlags.Node.IsFlagSet(flag_);
            FitChildrenHorizontally(0);
        }
    }
}
