namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using UnityEngine;
    using System.Linq;

    public class LaneFlagToggle : UICheckBoxExt {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;
        UISprite[] Sprites => GetComponentsInChildren<UISprite>();

        uint laneID_;
        uint []laneIDs_;
        NetLaneExt.Flags flag_;
        public static LaneFlagToggle Add(UIPanel parent, uint laneID, uint[] laneIDs, NetLaneExt.Flags flag) {
            var toggle = parent.AddUIComponent<LaneFlagToggle>();
            toggle.flag_ = flag;
            toggle.laneID_ = laneID;
            toggle.laneIDs_ = laneIDs ?? new uint[0];
            ref var lane = ref man_.LaneBuffer[laneID];
            toggle.isChecked = lane.m_flags.IsFlagSet(flag);
            if(toggle.laneIDs_.Any(item => man_.LaneBuffer[item].m_flags.IsFlagSet(flag) != toggle.isChecked)) {
                toggle.SetSpritesColor(Color.yellow);
            }
            return toggle;
        }

        public override void Start() {
            base.Start();
            var segment = laneID_.ToLane().m_segment;
            var metadata = segment.ToSegment().Info?.GetMetaData();
            string name = metadata.GetCustomLaneFlagName(flag_, NetUtil.GetLaneIndex(laneID_));
            this.Label = name ?? flag_.ToString();
            this.tooltip = flag_.ToString();
        }

        public override void OnCheckChanged(UIComponent component, bool value) {
            base.OnCheckChanged(component, value);
            SetSpritesColor(Color.white);
            SimulationManager.instance.AddAction(delegate () {
                UpdateLaneFlags(laneID_, flag_, value);
                foreach(var segmentID in laneIDs_) {
                    UpdateLaneFlags(segmentID, flag_, value);
                }
            });
        }
        public static void UpdateLaneFlags(uint laneID, NetLaneExt.Flags flag, bool value) {
            ref var lane = ref man_.LaneBuffer[laneID];
            var newFlags = lane.m_flags.SetFlags(flag, value);
            if(lane.m_flags != newFlags) {
                lane.m_flags = newFlags;
                man_.UpdateSegment(laneID.ToLane().m_segment);
            }
        }

        public void SetSpritesColor(Color color) {
            foreach(var sprite in Sprites)
                sprite.color = color;
        }
    }
}
