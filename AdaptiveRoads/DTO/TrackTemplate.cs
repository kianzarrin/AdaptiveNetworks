namespace AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using KianCommons;
using System.Linq;
using static AdaptiveRoads.Manager.NetInfoExtionsion;

public class TrackTemplate : TemplateBase<TrackTemplate> {
    public Track[] Tracks { get; private set; }
    public Track[] GetTracks() => Tracks.ToArray();

    public override string Summary {
        get {
            string ret = Name + $"({Date})";
            if (!string.IsNullOrEmpty(Description))
                ret += "\n" + Description;
            var summaries = Tracks.Select(track => track.Summary());
            ret += "\n" + summaries.JoinLines();
            return ret;
        }
    }

    public static TrackTemplate Create(
        string name,
        Track[] tracks,
        string description) {
        return new TrackTemplate {
            Name = name,
            Tracks = tracks.ToArray(),
            Description = description,
        };
    }
}
