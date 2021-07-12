namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons;
    using AdaptiveRoads.Manager;
    using System.Collections.Generic;

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

            // this should appear first
            string name = CustomFlagAttribute.GetName(flag_, nodeID_.ToNode().Info);
            List<string> names = new List<string>();
            if(name!=null)
            foreach (ushort segmentID in nodeID_.ToNode().IterateSegments()) {
                string name0 = CustomFlagAttribute.GetName(flag_, segmentID.ToSegment().Info);
                if(name0!=null && !names.Contains(name0))
                    names.Add(name0);
            }

            if (names.Count == 0)
                this.Label = flag_.ToString();
            else
                this.Label = names.Join(" | ");

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
