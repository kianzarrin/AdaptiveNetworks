namespace AdaptiveRoads.Patches.TMPE {
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using System.Reflection;
    using TrafficManager.Manager.Impl;

    [PreloadPatch]
    [HarmonyPatch(typeof(TrafficManager.LoadingExtension))]
    [HarmonyPatch(nameof(TrafficManager.LoadingExtension.OnLevelLoaded))]
    class OnLevelLoaded {
        static void Postfix() {
            GeometryManager.Instance.Subscribe(observer_ as ARTMPEObsever);
            LifeCycle.LifeCycle.Load();
        }

        static void Prepare(MethodBase original) {
            if(original == null) // first call
                observer_ = new ARTMPEObsever();
        }
        static object observer_;
    }
}
