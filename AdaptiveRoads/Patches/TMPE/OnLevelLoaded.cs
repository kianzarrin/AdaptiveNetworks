namespace AdaptiveRoads.Patches.TMPE {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using System.Reflection;
    using TrafficManager.API.Geometry;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Util;
    using TrafficManager.Manager.Impl;
    using AdaptiveRoads.Util;

    [InGamePatch]
    [HarmonyPatch(typeof(TrafficManager.LoadingExtension))]
    [HarmonyPatch(nameof(TrafficManager.LoadingExtension.OnLevelLoaded))]
    class OnLevelLoaded {
        static void Postfix() {
            GeometryManager.Instance.Subscribe(observer_ as ARTMPEObsever);
        }

        static void Prepare(MethodBase original) {
            if (original == null) // first call
                observer_ = new ARTMPEObsever();
        }
        static object observer_;
    }
}
