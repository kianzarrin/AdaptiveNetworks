namespace AdaptiveRoads.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TrafficManager;
    using TrafficManager.API.Manager;

    public static class Shortcuts {
        public static IManagerFactory TMPE => Constants.ManagerFactory;
        public static IParkingRestrictionsManager ParkingMan => TMPE?.ParkingRestrictionsManager;
        public static ITrafficPriorityManager PrioMan => TMPE?.TrafficPriorityManager;
        public static IVehicleRestrictionsManager VRMan => TMPE?.VehicleRestrictionsManager;
        public static ILaneConnectionManager LCMan => TMPE?.LaneConnectionManager;
        public static IJunctionRestrictionsManager JRMan => TMPE?.JunctionRestrictionsManager;
        public static ILaneArrowManager LaneArrowMan => TMPE?.LaneArrowManager;
        public static ISpeedLimitManager SLMan => TMPE?.SpeedLimitManager;
        public static IRoutingManager RMan => TMPE?.RoutingManager;


    }
}
