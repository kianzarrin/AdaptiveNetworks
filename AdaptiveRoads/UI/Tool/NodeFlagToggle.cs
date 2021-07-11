namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using AdaptiveRoads.Manager;

    public class NodeFlagToggle : UICheckBoxExt {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        ushort nodeID_;
        NetNodeExt.Flags flag_;
        public static NodeFlagToggle Add(UIPanel parent, ushort nodeID, NetNodeExt.Flags flag) {
            var toggle = parent.AddUIComponent<NodeFlagToggle>();
            toggle.flag_ = flag;
            toggle.nodeID_ = nodeID;
            ref var node = ref man_.NodeBuffer[nodeID];
            toggle.isChecked = node.m_flags.IsFlagSet(flag);
            return toggle;
        }

        public override void Start() {
            base.Start();
            string name = CustomFlagAttribute.GetName(flag_, nodeID_.ToNode().Info);
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SimulationManager.instance.AddAction(delegate () {
                ref var node = ref man_.NodeBuffer[nodeID_];
                var newFlags = node.m_flags.SetFlags(flag_, value);
                if (node.m_flags != newFlags) {
                    node.m_flags = newFlags;
                    man_.UpdateNode(nodeID_);
                }
            });
        }
    }
}
