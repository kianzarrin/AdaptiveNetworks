using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using AdaptiveRoads.DTO;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class LoadSegmentTemplatePanel : LoadTemplatePanel<SaveListBoxSegment, SegmentTemplate> {
        public delegate void OnLoadedHandler(NetInfo.Segment[] props);
        public event OnLoadedHandler OnLoaded;
        protected override string Title => "Load Segment Template";

        public static LoadSegmentTemplatePanel Display(OnLoadedHandler handler) {
            Log.Called();
            var ret = UIView.GetAView().AddUIComponent<LoadSegmentTemplatePanel>();
            ret.OnLoaded = handler;
            return ret;
        }

        protected override void AddCustomUIComponents(UIPanel panel) { }

        public override void Load(SegmentTemplate template) {
            var nodes = template.GetSegments();
            OnLoaded(nodes);
        }
    }
}
