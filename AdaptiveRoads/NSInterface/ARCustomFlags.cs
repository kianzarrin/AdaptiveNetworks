namespace AdaptiveRoads.NSInterface {
    using AdaptiveRoads.Manager;
    using System;
    using System.Linq;
    using KianCommons;
    using AdaptiveRoads.Data.NetworkExtensions;

    public class ARCustomFlags : ICloneable {
        public NetSegmentExt.Flags Segment;
        public NetSegmentEnd.Flags SegmentEnd;
        public NetNodeExt.Flags Node;
        public NetLaneExt.Flags[] Lanes;

        public ARCustomFlags() { }
        public ARCustomFlags(int nLanes) {
            Lanes = new NetLaneExt.Flags[nLanes];
        }

        public ARCustomFlags Clone() {
            var ret = MemberwiseClone() as ARCustomFlags;
            ret.Lanes = ret.Lanes.Clone() as NetLaneExt.Flags[];
            return ret;
        }

        object ICloneable.Clone() => this.Clone();

        public override bool Equals(object obj) {
            return obj is ARCustomFlags data &&
                Segment.Equals(data.Segment) &&
                SegmentEnd.Equals(data.SegmentEnd) &&
                Node.Equals(data.Node) &&
                Lanes.SequenceEqual(data.Lanes);
        }

        public override int GetHashCode() {
            unchecked {
                int hc = Segment.GetHashCode();
                hc = unchecked(hc * 314159 + Node.GetHashCode());
                hc = unchecked(hc * 314159 + SegmentEnd.GetHashCode());
                hc = unchecked(hc * 314159 + Node.GetHashCode());
                for(int i = 0; i < Lanes.Length; ++i)
                    hc = unchecked(hc * 314159 + Lanes[i].GetHashCode());
                return hc;
            }
        }

        public bool IsDefault() {
            return
                Segment == default &&
                SegmentEnd == default &&
                Node == default &&
                Lanes.All(lane => lane == default);
        }

        public override string ToString() =>
            $"segment:{Segment} segmentEnd:{SegmentEnd} node:{Node} lanes:{Lanes.ToSTR()}";
    }
}
