namespace AdaptiveRoads.UI.Tool {
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
        LaneData lane_;
        UIPanel panel_;

        public static LaneCaptionButton Add(UIPanel panel, LaneData lane) {
            var ret = panel.AddUIComponent<LaneCaptionButton>();
            ret.lane_ = lane;
            ret.panel_ = panel;
            return ret;
        }

        public override void Start() {
            base.Start();
            FitTo(panel_, LayoutDirection.Horizontal);
            var laneType = lane_.LaneInfo.m_laneType;
            var vehicleTypes = lane_.LaneInfo.m_vehicleType;
            text = $"▲ [{laneType}] : {vehicleTypes} + ";
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
