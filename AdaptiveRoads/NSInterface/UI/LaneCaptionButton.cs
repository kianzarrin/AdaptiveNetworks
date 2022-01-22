namespace AdaptiveRoads.NSInterface.UI {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.Assertion;

    public class LaneCaptionButton : UIButtonExt {
        UIPanel panel_;

        int laneIndex_;
        static NetInfo Prefab => ANImplementation.Instance.Prefab;
        NetInfo.Lane LaneInfo => Prefab.m_lanes[laneIndex_];

        public static LaneCaptionButton Add(UIPanel parent,  int laneIndex) {
            var ret = parent.AddUIComponent<LaneCaptionButton>();
            ret.laneIndex_ = laneIndex;
            ret.Init();
            return ret;
        }

        public void SetTarget(UIPanel panel) => panel_ = panel;

        void Init() {
            this.ParentWith = false;
            var laneType = LaneInfo.m_laneType;
            var vehicleTypes = LaneInfo.m_vehicleType;
            text = $"▲ [{laneType}] : {vehicleTypes}";

            var padding = spritePadding;
            padding.bottom = -3;
            spritePadding = padding;
            autoSize = true;
        }

        protected override void OnClick(UIMouseEventParameter p) {
            base.OnClick(p);
            if (panel_.isVisible) {
                panel_.Hide();
                text = text.Replace("▲", "▼");
            } else {
                panel_.Show();
                text = text.Replace("▼", "▲");
            }
        }
    }

}
