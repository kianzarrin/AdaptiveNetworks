namespace AdaptiveRoads.UI.Tool {
    using AdaptiveRoads.Util;
    using KianCommons;
    using System.Collections.Generic;
    using System.Linq;

    public static class Util {
        public static IEnumerable<ushort> GetSimilarSegmentsBetweenJunctions(ushort segmentID) {
            var ret = new List<ushort>();
            ret.Add(segmentID);
            ret.AddRange(TraverseSegments(segmentID, segmentID.ToSegment().m_startNode));
            ret.AddRange(TraverseSegments(segmentID, segmentID.ToSegment().m_endNode));
            return ret;
        }

        private static HashSet<ushort> hashset = new ();
        static IEnumerable<ushort> TraverseSegments(ushort startSegmentID, ushort nodeID) {
            NetInfo startInfo = startSegmentID.ToSegment().Info;
            ushort nextSegmentID = startSegmentID;
            int watchdog = 0;
            while(nodeID != 0 && nodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.Middle | NetNode.Flags.Bend)) {
                nextSegmentID = nodeID.ToNode().GetAnotherSegment(nextSegmentID);

                if (watchdog++ > 10000)
                    break;
                if (nextSegmentID.ToSegment().Info != startInfo)
                    break;
                if(nextSegmentID == startSegmentID)
                    break; // circled around
                if (hashset.Contains(nextSegmentID)) {
                    Log.Error("unexpected Loop detected. send screenshot of networks to kian.");
                    break;
                }

                hashset.Add(nextSegmentID);
                nodeID = nextSegmentID.ToSegment().GetOtherNode(nodeID);
            }
            return hashset;
        }

        public static IEnumerable<LaneData> GetSimilarLanes(LaneData lane, IEnumerable<ushort> segmentIDs_) =>
            LaneHelpers.GetSimilarLanes(lane.LaneIndex, segmentIDs_);
    }
}
