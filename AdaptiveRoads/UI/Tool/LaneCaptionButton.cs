namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;

    public class LaneCaptionButton : UIButtonExt, IFittable {
        LaneData lane_;
        UIPanel panel_;

        public static LaneCaptionButton Add(UIPanel parent, LaneData lane) {
            var ret = parent.AddUIComponent<LaneCaptionButton>();
            ret.lane_ = lane;
            ret.Init();
            return ret;
        }

        public void SetTarget(UIPanel panel) => panel_ = panel;

        void Init() {
            this.ParentWith = false;
            var laneType = lane_.LaneInfo.m_laneType;
            var vehicleTypes = lane_.LaneInfo.m_vehicleType;
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

        public void Fit2Children() => this.FitChildrenHorizontally();
        public void Fit2Parent() {
            autoSize = false;
            this.width = parent.width;
        }
    }

}
