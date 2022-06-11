namespace AdaptiveRoads.Util {
    using JetBrains.Annotations;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TrafficManager.API.Traffic.Enums;
    using static Shortcuts;

    internal static class TrackUtils {
        internal const NetInfo.LaneType TRACK_LANE_TYPES =
            NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;

        internal const VehicleInfo.VehicleType TRACK_VEHICLE_TYPES =
            VehicleInfo.VehicleType.Metro |
            VehicleInfo.VehicleType.Train |
            VehicleInfo.VehicleType.Tram |
            VehicleInfo.VehicleType.Monorail;

        internal const VehicleInfo.VehicleType TILTABLE_VEHICLE_TYPES =
            VehicleInfo.VehicleType.Train |
            VehicleInfo.VehicleType.Metro |
            VehicleInfo.VehicleType.Monorail;

        internal static NetInfo.LaneType RoadLaneTypes => LaneArrowMan.LaneTypes;

        internal static VehicleInfo.VehicleType RoadVehicleTypes = LaneArrowMan.VehicleTypes | VehicleInfo.VehicleType.Trolleybus;

        internal static bool MatchesTrack([NotNull] this NetInfo.Lane laneInfo) =>
            laneInfo.Matches(TRACK_LANE_TYPES, TRACK_VEHICLE_TYPES);

        internal static bool MatchesRoad([NotNull] this NetInfo.Lane laneInfo) =>
            laneInfo.Matches(RoadLaneTypes, RoadVehicleTypes);

        internal static bool Matches([NotNull] this NetInfo.Lane laneInfo, NetInfo.LaneType laneType, VehicleInfo.VehicleType vehicleType) {
            return (laneType & laneInfo.m_laneType) != 0 && (vehicleType & laneInfo.m_vehicleType) != 0;
        }

        internal static bool IsTrackOnly(this NetInfo.Lane laneInfo) {
            return
                laneInfo.MatchesTrack() &&
                !laneInfo.m_laneType.IsFlagSet(~TRACK_LANE_TYPES) &&
                !laneInfo.m_vehicleType.IsFlagSet(~TRACK_VEHICLE_TYPES);
        }

        public static LaneEndTransitionGroup GetLaneEndTransitionGroup(VehicleInfo.VehicleType vehicleType) {
            LaneEndTransitionGroup ret = 0;
            if (vehicleType.IsFlagSet(RoadVehicleTypes))
                ret |= LaneEndTransitionGroup.Road;
            if (vehicleType.IsFlagSet(TRACK_VEHICLE_TYPES))
                ret |= LaneEndTransitionGroup.Track;
            return ret;
        }

        public static LaneEndTransitionGroup GetLaneEndTransitionGroup(this NetInfo.Lane laneInfo) {
            LaneEndTransitionGroup ret = 0;
            if (laneInfo.MatchesRoad())
                ret |= LaneEndTransitionGroup.Road;
            if (laneInfo.MatchesTrack())
                ret |= LaneEndTransitionGroup.Track;
            return ret;
        }

        public static bool IsFlagSet(this LaneEndTransitionGroup value, LaneEndTransitionGroup group) =>
            (value & group) != 0;
    }
}
