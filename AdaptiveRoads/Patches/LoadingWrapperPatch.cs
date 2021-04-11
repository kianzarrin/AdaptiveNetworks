namespace AdaptiveRoads.Patches {
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using KianCommons;
    using System;
    using TrafficManager.Manager.Impl;
    using System.Reflection;

    [HarmonyPatch(typeof(LoadingWrapper), "OnLevelLoaded")]
    public static class LoadingWrapperPatch {
        static void Finalizer(Exception __exception) {
            try {
                OnPostLevelLoaded();
                if (__exception == null) return;
                Log.Exception(__exception);
            }catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void OnPostLevelLoaded() {
            LifeCycle.LifeCycle.Load();
        }
    }
}