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

    public static class TMPEHelpers {
        static IManagerFactory TMPE => Constants.ManagerFactory;
        static ISpeedLimitManager SLMan => TMPE?.SpeedLimitManager as SpeedLimitManager;

        public static float GetLaneSpeedLimit(uint laneID) =>
            (SLMan as SpeedLimitManager).GetGameSpeedLimit(laneID);

        public static float GetMaxSpeedLimit(ushort segmentID, NetInfo.Direction direction) {
            return NetUtil.IterateSegmentLanes(segmentID)
                .Where(lane => lane.LaneInfo.m_finalDirection == direction)
                .Max(lane => GetLaneSpeedLimit(lane.LaneID));
        }

        public static void GetMaxSpeedLimit(ushort segmentID, out float forward, out float backward) {
            forward = TMPEHelpers.GetMaxSpeedLimit(segmentID, NetInfo.Direction.Forward);
            backward = TMPEHelpers.GetMaxSpeedLimit(segmentID, NetInfo.Direction.Backward);
            if(segmentID.ToSegment().IsInvert())
                Helpers.Swap(ref forward, ref backward);
        }
        public static float GetMaxSpeedLimit(ushort segmentID) {
            GetMaxSpeedLimit(segmentID, out float forward, out float backward);
            return Mathf.Max(forward, backward);
        }

    }
}
