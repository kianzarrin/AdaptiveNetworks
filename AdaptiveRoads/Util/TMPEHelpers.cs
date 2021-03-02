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
            //if(segmentID.ToSegment().IsInvert())
            //    Helpers.Swap(ref forward, ref backward);
        }
        public static float GetMaxSpeedLimit(ushort segmentID) {
            GetMaxSpeedLimit(segmentID, out float forward, out float backward);
            return Mathf.Max(forward, backward);
        }

    }
}
