namespace AdaptiveRoads.UI.Tool {
    using AdaptiveRoads.Util;
    using KianCommons;
    using System.Collections.Generic;

    public static class Util {
        public static IEnumerable<ushort> GetSimilarSegmentsBetweenJunctions(ushort segmentID) {
            var ret = new List<ushort>();
            ret.Add(segmentID);
            ret.AddRange(TraverseSegments(segmentID, segmentID.ToSegment().m_startNode));
            ret.AddRange(TraverseSegments(segmentID, segmentID.ToSegment().m_endNode));
            return ret;
        }

        static IEnumerable<ushort> TraverseSegments(ushort startSegmentID, ushort nodeID) {
            ushort nextSegmentID = startSegmentID;
            while(nodeID != 0 && nodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.Middle | NetNode.Flags.Bend)) {
                nextSegmentID = nodeID.ToNode().GetAnotherSegment(nextSegmentID);
                if(nextSegmentID.ToSegment().Info != startSegmentID.ToSegment().Info)
                    yield break;
                yield return nextSegmentID;
                nodeID = nextSegmentID.ToSegment().GetOtherNode(nodeID);
            }
        }

        public static IEnumerable<LaneData> GetSimilarLanes(LaneData lane, IEnumerable<ushort> segmentIDs_) =>
            LaneHelpers.GetSimilarLanes(lane.LaneIndex, segmentIDs_);
    }
}
