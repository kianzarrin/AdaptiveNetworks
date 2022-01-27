namespace AdaptiveRoads.Util {
    using ColossalFramework;
    using KianCommons;
    using System.Linq;
    using TrafficManager.API.Manager;
    using UnityEngine;

    public static class TMPEHelpers {
        static IManagerFactory TMPE => TrafficManager.Constants.ManagerFactory;
        static ISpeedLimitManager SLMan => TMPE?.SpeedLimitManager;
        static IRoutingManager RMan = TMPE?.RoutingManager;
        public static float GetLaneSpeedLimit(this LaneData lane) {
            if(SLMan != null)
                return SLMan.GetGameSpeedLimit(lane.LaneID, lane.LaneInfo);
            else
                return lane.LaneInfo.m_speedLimit;
        }

        public static bool IsSpeedLane(this LaneData lane) =>
            lane.LaneInfo.m_laneType.IsFlagSet(SLMan.LaneTypes) &&
            lane.LaneInfo.m_vehicleType.IsFlagSet(SLMan.VehicleTypes);

        public static float GetMaxSpeedLimit(ushort segmentID, NetInfo.Direction direction) {
            float ret = -1;
            foreach (var lane in NetUtil.IterateSegmentLanes(segmentID)) {
                if (lane.IsSpeedLane() && lane.LaneInfo.m_finalDirection == direction) {
                    ret = Mathf.Max(ret, lane.GetLaneSpeedLimit());
                }
            }
            return ret;
        }

        public static void GetMaxSpeedLimit(ushort segmentID, out float forward, out float backward) {
            forward = GetMaxSpeedLimit(segmentID, NetInfo.Direction.Forward);
            backward = GetMaxSpeedLimit(segmentID, NetInfo.Direction.Backward);
        }
        public static float GetMaxSpeedLimit(ushort segmentID) {
            GetMaxSpeedLimit(segmentID, out float forward, out float backward);
            return Mathf.Max(forward, backward);
        }

        public static bool SpeedChanges(ushort nodeID) {
            var segmentIDs = nodeID.ToNode().IterateSegments().ToArray();
            bool speedChange;
            // recalculate speed limits to avoid update order issues.
            if(segmentIDs.Length == 2) {
                ushort segmentID = segmentIDs[0];
                ushort segmentID2 = segmentIDs[1];
                bool startNode = segmentID.ToSegment().IsStartNode(nodeID);
                bool startNode2 = segmentID2.ToSegment().IsStartNode(nodeID);
                bool segmentInvert = segmentID.ToSegment().IsInvert();
                bool segmentInvert2 = segmentID2.ToSegment().IsInvert();
                bool reverse = (startNode2 == startNode) ^ (segmentInvert != segmentInvert2);
                TMPEHelpers.GetMaxSpeedLimit(segmentID, out float forward, out float backward);
                TMPEHelpers.GetMaxSpeedLimit(segmentID2, out float forward2, out float backward2);
                if(!reverse) {
                    speedChange = (forward != forward2) || (backward != backward2);
                } else {
                    speedChange = (forward != backward2) || (backward != forward2);
                }
            } else {
                speedChange = !segmentIDs.AllEqual(_segmentID => TMPEHelpers.GetMaxSpeedLimit(_segmentID));
            }
            return speedChange;
        }


        public static LaneTransitionData[] GetForwardRoutings(uint laneID, ushort nodeID) {
            bool startNode = laneID.ToLane().m_segment.ToSegment().IsStartNode(nodeID);
            uint routingIndex = RMan.GetLaneEndRoutingIndex(laneID, startNode);
            return RMan.LaneEndForwardRoutings[routingIndex].transitions;
        }
    }
}
