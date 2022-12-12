using AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System.Collections.Generic;
using System.Linq;
using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SaveTrackTemplatePanel : SaveTemplatePanel<SaveListBoxTrack, TrackTemplate> {
        public List<Track> tracks;

        public static SaveTrackTemplatePanel Display(IEnumerable<Track> tracks) {
            Log.Called();
            if (tracks.IsNullorEmpty()) {
                return null;
            }
            var ret = UIView.GetAView().AddUIComponent<SaveTrackTemplatePanel>();
            ret.tracks = tracks.ToList();
            return ret;
        }

        public override string Title => "Save Track Template";
        public override string GetItemsSummary() => tracks?.Summary();
        public override ISerialziableDTO CreateTemplate() {
            return TrackTemplate.Create(
                NameField.text,
                tracks.ToArray(),
                DescriptionField.text);
        }
    }
}
