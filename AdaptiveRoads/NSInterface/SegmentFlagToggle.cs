namespace AdaptiveRoads.NSInterface {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using NetworkSkins.Helpers;
    using System;

    public class SegmentFlagToggle : UICheckBoxExt {
        public NetSegmentExt.Flags Flag;

        public static SegmentFlagToggle Add(UIPanel parent, NetSegmentExt.Flags flag) {
            var toggle = parent.AddUIComponent<SegmentFlagToggle>();
            toggle.Flag = flag;
            return toggle;
        }

        public override void Start() {
            base.Start();
            string name = CustomFlagAttribute.GetName(Flag, ARImplementation.Instance.Prefab);
            this.Label = name ?? Flag.ToString();
            this.tooltip = Flag.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            try {
                Log.Called($"Flag={Flag}", "value="+value);
                base.OnCheckChanged(component, value);
                ARImplementation.Instance.CustomSegmentFlags = ARImplementation.Instance.CustomSegmentFlags.SetFlags(Flag, value);
                Log.Info("CustomSegmentFlags became " + ARImplementation.Instance.CustomSegmentFlags);
                ARImplementation.Instance.OnControllerChanged();
            } catch(Exception ex) { ex.Log(); }
        }

        public void Refresh(NetSegmentExt.Flags flags) {
            isChecked = flags.IsFlagSet(Flag);
            FitChildrenHorizontally(0);
        }
    }
}
