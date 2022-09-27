namespace AdaptiveRoads.Manager {
    using System;
    using System.Collections.Generic;
    using KianCommons;

    public struct CustomFlags {
        public NetNodeExt.Flags Node;
        public NetSegmentExt.Flags Segment;
        public NetSegmentEnd.Flags SegmentEnd;
        public NetLaneExt.Flags Lane;

        public static CustomFlags None = default;

        public static CustomFlags operator |(CustomFlags lhs, CustomFlags rhs) {
            return new CustomFlags {
                Node = lhs.Node | rhs.Node,
                Segment = lhs.Segment | rhs.Segment,
                SegmentEnd = lhs.SegmentEnd | rhs.SegmentEnd,
                Lane = lhs.Lane | rhs.Lane,
            };
        }

        public static CustomFlags Or(IEnumerable<CustomFlags> source) {
            CustomFlags ret = default;
            foreach (var item in source) ret |= item;
            return ret;
        }

        public bool IsDefault() {
            return
                Node == default &&
                Segment == default &&
                SegmentEnd == default &&
                Lane == default;
        }

        public IEnumerable<Enum> Iterate() {
            foreach (var item in new Enum[] { Node, Segment, SegmentEnd, Lane }) {
                foreach (var flag in item.ExtractPow2Flags()) {
                    yield return flag as Enum;
                }
            }
        }

        public override string ToString() =>
            $"CustomFlags{{Node: {Node} | Segment: {Segment}  | SegmentEnd: {SegmentEnd} | Lane: {Lane}}}";
    }

    public class CustomFlagAttribute : Attribute {
        public static string GetName(Enum flag, NetInfo netInfo) {
            var cfn = netInfo?.GetMetaData()?.CustomFlagNames;
            if(cfn != null && cfn.TryGetValue(flag, out string ret))
                return ret;
            return null;
        }
    }


}

