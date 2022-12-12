using AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class LoadTrackTemplatePanel : LoadTemplatePanel<SaveListBoxTrack, TrackTemplate> {
        public delegate void OnLoadedHandler(Track[] tracks);
        public event OnLoadedHandler OnLoaded;
        protected override string Title => "Load Track Template";

        public static LoadTrackTemplatePanel Display(OnLoadedHandler handler) {
            Log.Called();
            var ret = UIView.GetAView().AddUIComponent<LoadTrackTemplatePanel>();
            ret.OnLoaded = handler;
            return ret;
        }

        protected override void AddCustomUIComponents(UIPanel panel) { }

        public override void Load(TrackTemplate template) {
            var tracks = template.GetTracks();
            var info = RoadEditorUtils.GetSelectedNetInfo(out var _);
            foreach (var track in tracks) track.InitializeEmpty(info);
            OnLoaded(tracks);
        }
    }
}
