namespace AdaptiveRoads.Manager {
    using KianCommons;
    using System;
    using AdaptiveRoads.Data.NetworkExtensions;

    public static partial class NetInfoExtionsion {
        [Serializable]
        public class Range {
            public float Lower, Upper;
            public bool InRange(float value) => Lower <= value && value < Upper;
            public override string ToString() => $"[{Lower}:{Upper})";
        }

        [Serializable]
        [FlagPair]
        public struct VanillaSegmentInfoFlags {
            [BitMask]
            public NetSegment.Flags Required, Forbidden;
            public bool CheckFlags(NetSegment.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Obsolete("use VanillaNodeInfoFlagsLong instead")]
        //[FlagPair]
        [Serializable]
        public struct VanillaNodeInfoFlags {
            [BitMask]
            public NetNode.Flags Required, Forbidden;
            //public bool CheckFlags(NetNode.Flags flags) => flags.CheckFlags(Required, Forbidden);

            public static explicit operator VanillaNodeInfoFlagsLong(VanillaNodeInfoFlags flags) {
                return new VanillaNodeInfoFlagsLong {
                    Required = (NetNode.FlagsLong)flags.Required,
                    Forbidden = (NetNode.FlagsLong)flags.Forbidden,
                };
            }
        }

        [FlagPair]
        [Serializable]
        public struct VanillaNodeInfoFlagsLong {
            [BitMask]
            public NetNode.FlagsLong Required, Forbidden;
            public bool CheckFlags(NetNode.FlagsLong flags) => flags.CheckFlags(Required, Forbidden);
        }


        [Serializable]
        [FlagPair]
        public struct VanillaLaneInfoFlags {
            [BitMask]
            public NetLane.Flags Required, Forbidden;
            public bool CheckFlags(NetLane.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetSegment.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetSegmentFlags))]
        [Serializable]
        public struct SegmentInfoFlags {
            [BitMask]
            public NetSegmentExt.Flags Required, Forbidden;
            internal NetSegmentExt.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentExt.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        [Hint("segment specific node flags")]
        public struct SegmentEndInfoFlags {
            [BitMask]
            public NetSegmentEnd.Flags Required, Forbidden;
            internal NetSegmentEnd.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentEnd.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetNode.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlags))]
        [Serializable]
        public struct NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            internal NetNodeExt.Flags UsedCustomFlags => (Required | Forbidden) & NetNodeExt.Flags.CustomsMask;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [FlagPair(MergeWithEnum = typeof(NetLane.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetLaneFlags))]
        [Serializable]
        public struct LaneInfoFlags {
            [BitMask]
            public NetLaneExt.Flags Required, Forbidden;
            internal NetLaneExt.Flags UsedCustomFlags => (Required | Forbidden) & NetLaneExt.Flags.CustomsMask;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [FlagPair(MergeWithEnum = typeof(NetLane.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetLaneFlags))]
        [Serializable]
        public struct LaneTransitionInfoFlags {
            [BitMask]
            public LaneTransition.Flags Required, Forbidden;
            public bool CheckFlags(LaneTransition.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }
    }
}


