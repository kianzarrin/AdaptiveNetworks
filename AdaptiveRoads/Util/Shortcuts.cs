namespace AdaptiveRoads.Util {
    using TrafficManager.API;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Notifier;

    public static class Shortcuts {
        public static IManagerFactory TMPE => Implementations.ManagerFactory;
        public static IParkingRestrictionsManager ParkingMan => TMPE?.ParkingRestrictionsManager;
        public static ITrafficPriorityManager PrioMan => TMPE?.TrafficPriorityManager;
        public static IVehicleRestrictionsManager VRMan => TMPE?.VehicleRestrictionsManager;
        public static ILaneConnectionManager LCMan => TMPE?.LaneConnectionManager;
        public static IJunctionRestrictionsManager JRMan => TMPE?.JunctionRestrictionsManager;
        public static ILaneArrowManager LaneArrowMan => TMPE?.LaneArrowManager;
        public static ISpeedLimitManager SLMan => TMPE?.SpeedLimitManager;
        public static IRoutingManager RMan => TMPE?.RoutingManager;
        public static INotifier TMPENotifier => Implementations.Notifier;
    }
}
