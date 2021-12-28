namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System.Linq;
    using UnityEngine;
    using AdaptiveRoads.Data.NetworkExtensions;

    public class SegmentFlagToggle : AutoSizeCheckbox {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;
        UISprite[] Sprites => GetComponentsInChildren<UISprite>();

        ushort segmentID_;
        ushort []segmentIDs_;
        NetSegmentExt.Flags flag_;

        public static SegmentFlagToggle Add(UIPanel parent, ushort segmentID, ushort []segmentIDs, NetSegmentExt.Flags flag) {
            var toggle = parent.AddUIComponent<SegmentFlagToggle>();
            toggle.flag_ = flag;
            toggle.segmentID_ = segmentID;
            toggle.segmentIDs_ = segmentIDs ?? new ushort[0];
            ref var segment = ref man_.SegmentBuffer[segmentID];
            toggle.isChecked = segment.m_flags.IsFlagSet(flag);
            if(toggle.segmentIDs_.Any(item => man_.SegmentBuffer[item].m_flags.IsFlagSet(flag) != toggle.isChecked)) {
                toggle.SetSpritesColor(Color.yellow);
            }

            return toggle;
        }


        public override void Start() {
            base.Start();
            string name = CustomFlagAttribute.GetName(flag_, segmentID_.ToSegment().Info);
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SetSpritesColor(Color.white);
            SimulationManager.instance.AddAction(delegate () {
                UpdateSegmentFlags(segmentID_, flag_, value);
                foreach(var segmentID in segmentIDs_) {
                    UpdateSegmentFlags(segmentID, flag_, value);
                }
            }); 
        }

        public static void UpdateSegmentFlags(ushort segmentID, NetSegmentExt.Flags flag,  bool value) {
            ref var segment = ref man_.SegmentBuffer[segmentID];
            var newFlags = segment.m_flags.SetFlags(flag, value);
            if(segment.m_flags != newFlags) {
                segment.m_flags = newFlags;
                man_.UpdateSegment(segmentID);
            }
        }

        public void SetSpritesColor(Color color) {
            foreach(var sprite in Sprites)
                sprite.color = color;
        }
    }
}
