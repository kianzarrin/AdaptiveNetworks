using System;
using System.Collections.Generic;
namespace AdaptiveRoads.NSInterface {
    using AdaptiveRoads.Manager;
    using System.Linq;
    using KianCommons;
    using AdaptiveRoads.Data.NetworkExtensions;
    using TrafficManager.API.Traffic;
    using Pair = KeyValuePair<Enum, int>;

    public class ARCustomFlags : ICloneable {
        public NetSegmentExt.Flags Segment;
        public NetSegmentEnd.Flags SegmentEnd;
        public NetNodeExt.Flags Node;
        public NetLaneExt.Flags[] Lanes;

        public ARCustomFlags() { }
        public ARCustomFlags(int nLanes) {
            Lanes = new NetLaneExt.Flags[nLanes];
        }

        public static ARCustomFlags operator &(ARCustomFlags a, ARCustomFlags b) {
            int aLaneCount = a.Lanes?.Length ?? 0;
            int bLaneCount = b.Lanes?.Length ?? 0;
            int minLaneCount = Math.Min(aLaneCount, bLaneCount);
            int maxLaneCount = Math.Max(aLaneCount, bLaneCount);
            var ret = new ARCustomFlags(maxLaneCount);
            ret.Segment = a.Segment & b.Segment;
            ret.SegmentEnd = a.SegmentEnd & b.SegmentEnd;
            ret.Node = a.Node & b.Node;
            for(int laneIndex = 0; laneIndex < minLaneCount; ++laneIndex) {
                ret.Lanes[laneIndex] = a.Lanes[laneIndex] & b.Lanes[laneIndex];
            }
            return ret;
        }
        public static ARCustomFlags operator |(ARCustomFlags a, ARCustomFlags b) {
            int aLaneCount = a.Lanes?.Length ?? 0;
            int bLaneCount = b.Lanes?.Length ?? 0;
            int minLaneCount = Math.Min(aLaneCount, bLaneCount);
            int maxLaneCount = Math.Max(aLaneCount, bLaneCount);
            var ret = new ARCustomFlags(maxLaneCount);
            ret.Segment = a.Segment | b.Segment;
            ret.SegmentEnd = a.SegmentEnd | b.SegmentEnd;
            ret.Node = a.Node | b.Node;
            for (int laneIndex = 0; laneIndex < maxLaneCount; ++laneIndex) {
                var flagA = laneIndex < aLaneCount ? a.Lanes[laneIndex] : default;
                var flagB = laneIndex < bLaneCount ? b.Lanes[laneIndex] : default;
                ret.Lanes[laneIndex] = flagA | flagB;
            }
            return ret;
        }

        public IEnumerable<Enum> IterateNonLaneFlags() {
            foreach (var flag in Segment.ExtractPow2Flags()) yield return flag;
            foreach (var flag in SegmentEnd.ExtractPow2Flags()) yield return flag;
            foreach (var flag in Node.ExtractPow2Flags()) yield return flag;
        }


        public IEnumerable<Pair> IterateAll() {
            foreach (var flag in IterateNonLaneFlags()) {
                yield return new Pair(flag, -1);
            }
            for(int laneIndex = 0; laneIndex < Lanes.Length; ++laneIndex) {
                foreach (var flag in Lanes[laneIndex].ExtractPow2Flags()) {
                    yield return new Pair(flag, laneIndex);
                }
            }
        }

        public bool HasFlag(Enum flag, int laneIndex = -1) {
            if (flag is NetNodeExt.Flags nodeFlag) {
                return Node.IsFlagSet(nodeFlag);
            } else if (flag is NetSegmentExt.Flags segmentFlag) {
                return Segment.IsFlagSet(segmentFlag);
            } else if (flag is NetSegmentEnd.Flags segmentEndFlag) {
                return SegmentEnd.IsFlagSet(segmentEndFlag);
            } else if (flag is NetLaneExt.Flags laneFlag) {
                return laneIndex >= 0 && laneIndex < Lanes.Length && Lanes[laneIndex].IsFlagSet(laneFlag);
            } else {
                throw new ArgumentException("flag: " + flag);
            }
        }

        public void AddFlag(Enum flag, int laneIndex = -1) {
            if (flag is NetNodeExt.Flags nodeFlag) {
                Node |= nodeFlag;
            } else if (flag is NetSegmentExt.Flags segmentFlag) {
                Segment |= segmentFlag;
            } else if (flag is NetSegmentEnd.Flags segmentEndFlag) {
                SegmentEnd |= segmentEndFlag;
            } else if (flag is NetLaneExt.Flags laneFlag) {
                Lanes[laneIndex] |= laneFlag;
            } else {
                throw new ArgumentException("flag: " + flag);
            }
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
        public string ToString(NetInfo baseInfo) {
            string ret =
                $"segment:{ToFlagNames(Segment, baseInfo)} " +
                $"segmentEnd:{ToFlagNames(SegmentEnd, baseInfo)} " +
                $"node:{ToFlagNames(Node, baseInfo)} ";
            ret += "lanes:{";
            for (int laneIndex = 0; laneIndex < Lanes.Length; ++laneIndex) {
                if (laneIndex > 0) ret += ", ";
                ret += $"lanes:{ToFlagNames(Lanes[laneIndex], baseInfo, laneIndex)}";
            }
            ret += "}";
            return ret;
        }

        static string ToFlagNames(Enum flags, NetInfo baseInfo) {
            string ret = "";
            foreach(Enum flag in flags.ExtractPow2Flags()) {
                if (baseInfo.GetSharedName(flag) is string name) {
                    if (ret != "") ret += ", ";
                    ret += name;
                }
            }
            return ret;
        }

        static string ToFlagNames(NetLaneExt.Flags flags, NetInfo baseInfo, int laneIndex) {
            string ret = "";
            foreach (NetLaneExt.Flags flag in flags.ExtractPow2Flags()) {
                if (baseInfo.GetSharedName(flag, laneIndex) is string name) {
                    if (ret != "") ret += ", ";
                    ret += name;
                }
            }
            return ret;
        }

    }
}
