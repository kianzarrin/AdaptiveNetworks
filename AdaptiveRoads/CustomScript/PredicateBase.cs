namespace AdaptiveRoads.CustomScript {
    using AdaptiveRoads.Data.NetworkExtensions;
    using AdaptiveRoads.Manager;
    using KianCommons;

    public static class Extensions {
        public static bool Has(ref this NetLaneExt lane, NetLaneExt.Flags flag) => lane.m_flags.IsFlagSet(flag);
        public static bool Has(ref this NetLaneExt lane, NetLane.Flags flag) => lane.LaneData.Lane.Flags().IsFlagSet(flag);
        public static bool Has(ref this LaneTransition laneTransition, NetNode.Flags flag) => laneTransition.m_flags.IsFlagSet(flag);
        public static bool Has(ref this NetSegmentExt segment, NetSegmentExt.Flags flag) => segment.m_flags.IsFlagSet(flag);
        public static bool Has(ref this NetSegmentExt segment, NetSegment.Flags flag) => segment.VanillaSegment.m_flags.IsFlagSet(flag);

        public static bool Has(ref this NetNodeExt node, NetNodeExt.Flags flag) => node.m_flags.IsFlagSet(flag);
        public static bool Has(ref this NetNodeExt node, NetNode.Flags flag) => node.VanillaNode.m_flags.IsFlagSet(flag);
        public static bool Has(ref this NetNodeExt node, NetNode.FlagsLong flag) => node.VanillaNode.flags.IsFlagSet(flag);

        public static bool Has(ref this NetSegmentEnd segmentEnd, NetSegmentEnd.Flags flag) => segmentEnd.m_flags.IsFlagSet(flag);

        public static bool Has(ref this NetSegmentEnd segmentEnd, NetSegmentExt.Flags flag) => segmentEnd.Segment.Has(flag);
        public static bool Has(ref this NetSegmentEnd segmentEnd, NetSegment.Flags flag) => segmentEnd.Segment.Has(flag);
        public static bool Has(ref this NetSegmentEnd segmentEnd, NetNodeExt.Flags flag) => segmentEnd.Node.Has(flag);
        public static bool Has(ref this NetSegmentEnd segmentEnd, NetNode.Flags flag) => segmentEnd.Node.Has(flag);

    }

    public abstract class PredicateBase {
        public abstract bool Condition();
        public ushort SegmentID;
        public ushort NodeID;
        public int LaneIndex;
        internal protected int LaneTransitionIndex;

        public override string ToString() => $"{this.GetType()}(SegmentID={SegmentID}, NodeID={NodeID}, LaneIndex={LaneIndex}, LaneTransitionIndex={LaneTransitionIndex})";

        public ref NetSegmentExt Segment => ref SegmentID.ToSegmentExt();
        public ref NetNodeExt Node => ref NodeID.ToNodeExt();
        public ref NetSegmentEnd SegmentEnd => ref Segment.GetEnd(NodeID);
        public uint LaneId => LaneIndex >= 0 ? Segment.LaneIDs[LaneIndex] : 0;
        public ref NetLaneExt Lane => ref LaneId.ToLaneExt();
        public ref NetLaneExt Lanes(int laneIndex) => ref Segment.LaneIDs[laneIndex].ToLaneExt();
        public ref LaneTransition LaneTransition => ref Node.GetLaneTransition(LaneTransitionIndex);

    }
}

