namespace AdaptiveRoads.NSInterface.UI {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using NetworkSkins.Helpers;
    using System;

    public class NodeFlagToggle : UICheckBoxExt {
        NetNodeExt.Flags flag_;
        ARImplementation Impl => ARImplementation.Instance;
        ARCustomFlags ARCustomFlags => Impl.ARCustomFlags;

        public static NodeFlagToggle Add(UIPanel parent, NetNodeExt.Flags flag) {
            var toggle = parent.AddUIComponent<NodeFlagToggle>();
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
                Log.Called($"Flag={flag_}", "value=" + value);
                base.OnCheckChanged(component, value);
                ARCustomFlags.Node = ARCustomFlags.Node.SetFlags(flag_, value);
                Log.Info("ARCustomFlags.Node became " + ARCustomFlags.Node);
                Impl.OnControllerChanged();
            } catch(Exception ex) { ex.Log(); }
        }

        public void Refresh(NetNodeExt.Flags flags) {
            isChecked = flags.IsFlagSet(flag_);
            FitChildrenHorizontally(0);
        }
    }
}
