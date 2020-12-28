namespace AdaptiveRoads.Patches.AssetPatches {
    using HarmonyLib;
    using KianCommons;
    using System;
    using LifeCycle;

    [HarmonyPatch(typeof(SaveAssetPanel), "SaveAsset")]
    public static class SaveAssetPrefix {
        public static void Prefix() => AssetDataExtension.BeforeSave();
        public static void Finalizer(Exception __exception) {
            Log.Debug("SaveAssetPanel.Finalizer() called with " + __exception.STR());
            SimulationManager.instance.ForcedSimulationPaused = false;
            if (__exception != null)
                Log.Exception(__exception);
        }
    }
}
