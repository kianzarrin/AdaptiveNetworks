using AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SaveSegmentTemplatePanel : SaveTemplatePanel<SaveListBoxSegment, SegmentTemplate> {
        public List<NetInfo.Segment> Segments;

        public static SaveSegmentTemplatePanel Display(IEnumerable<NetInfo.Segment> segments) {
            Log.Called();
            if (segments.IsNullorEmpty()) {
                return null;
            }
            var ret = UIView.GetAView().AddUIComponent<SaveSegmentTemplatePanel>();
            ret.Segments = segments.ToList();
            return ret;
        }

        public override string Title => "Save Segment Template";
        public override string GetItemsSummary() => Segments?.Summary();
        public override ISerialziableDTO CreateTemplate() {
            return SegmentTemplate.Create(
                NameField.text,
                Segments.ToArray(),
                DescriptionField.text);
        }
    }
}
