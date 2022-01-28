namespace AdaptiveRoads.Patches {
    using HarmonyLib;
    using KianCommons;
    using System;

    [HarmonyPatch(typeof(LoadingWrapper), "OnLevelLoaded")]
    [PreloadPatch]
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