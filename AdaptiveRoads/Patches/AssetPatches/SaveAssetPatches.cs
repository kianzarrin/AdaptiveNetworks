namespace AdaptiveRoads.Patches.AssetPatches {
    using HarmonyLib;
    using KianCommons;
    using LifeCycle;
    using System;

    /// <summary>
    /// package/metadata names are generated based on ground road name.
    /// </summary>
    [HarmonyPatch(typeof(SaveAssetPanel), "SaveAsset")]
    public static class SaveAssetPatch {
        static void Prefix(string saveName, ref string assetName) {
            AssetDataExtension.BeforeSave();
            if (ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo netInfo) {
                assetName = PackageHelper.StripName(netInfo.name);
                assetName = assetName.Remove("_Data");
            }
        }

        public static void Finalizer(Exception __exception) {
            SimulationManager.instance.ForcedSimulationPaused = false;
            if (__exception != null)
                Log.Exception(__exception);
        }
    }
}