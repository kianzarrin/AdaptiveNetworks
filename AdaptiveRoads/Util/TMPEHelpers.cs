namespace AdaptiveRoads.Util {
    using ColossalFramework;
    using ColossalFramework.IO;
    using ColossalFramework.Math;
    using CSUtil.Commons;
    using KianCommons;
    using System;
    using TrafficManager;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using UnityEngine;
    using Log = KianCommons.Log;
    using System.Linq;

    internal static class TMPEHelpers {
        static IManagerFactory TMPE => Constants.ManagerFactory;
        static ISpeedLimitManager SLMan => TMPE?.SpeedLimitManager as SpeedLimitManager;
        static IRoutingManager RMan = RoutingManager.Instance;
        public static float GetLaneSpeedLimit(this LaneData lane) {
            if(SLMan != null)
                return (SLMan as SpeedLimitManager).GetGameSpeedLimit(lane.LaneID);
            else
                return lane.LaneInfo.m_speedLimit;
        }

        public static bool IsSpeedLane(this LaneData lane) =>
            lane.LaneInfo.m_laneType.IsFlagSet(SpeedLimitManager.LANE_TYPES) &&
            lane.LaneInfo.m_vehicleType.IsFlagSet(SpeedLimitManager.VEHICLE_TYPES);

        public static float GetMaxSpeedLimit(ushort segmentID, NetInfo.Direction direction) {
            float ret = -1;
            foreach(var lane in NetUtil.IterateSegmentLanes(segmentID)) {
                if(lane.IsSpeedLane() && lane.LaneInfo.m_finalDirection == direction) {
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
            if (segmentIDs.Length == 2) {
                ushort segmentID = segmentIDs[0];
                ushort segmentID2 = segmentIDs[1];
                bool startNode = segmentID.ToSegment().IsStartNode(nodeID);
                bool startNode2 = segmentID2.ToSegment().IsStartNode(nodeID);
                bool segmentInvert = segmentID.ToSegment().IsInvert();
                bool segmentInvert2 = segmentID2.ToSegment().IsInvert();
                bool reverse = (startNode2 == startNode) ^ (segmentInvert != segmentInvert2);
                TMPEHelpers.GetMaxSpeedLimit(segmentID, out float forward, out float backward);
                TMPEHelpers.GetMaxSpeedLimit(segmentID2, out float forward2, out float backward2);
                if (!reverse) {
                    speedChange = (forward != forward2) || (backward != backward2);
                } else {
                    speedChange = (forward != backward2) || (backward != forward2);
                }
            } else {
                speedChange = !segmentIDs.AllEqual(_segmentID => TMPEHelpers.GetMaxSpeedLimit(_segmentID));
            }
            return speedChange;
        }


        public static LaneTransitionData []GetForwardRoutings(uint laneID, ushort nodeID) {
            bool startNode = laneID.ToLane().m_segment.ToSegment().IsStartNode(nodeID);
            uint routingIndex = RMan.GetLaneEndRoutingIndex(laneID, startNode);
            return RMan.LaneEndForwardRoutings[routingIndex].transitions;
        }
    }
}
