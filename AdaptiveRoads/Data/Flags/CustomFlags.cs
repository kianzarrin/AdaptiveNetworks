namespace AdaptiveRoads.Manager {
    using System;

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

        public bool IsDefault() {
            return
                Node == default &&
                Segment == default &&
                SegmentEnd == default &&
                Lane == default;
        }
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

