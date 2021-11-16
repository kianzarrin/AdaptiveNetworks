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
            AssetDataExtension.BeforeSave(saveName);
            if (ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo netInfo) {
                Assertion.Assert(netInfo is not null);
                Assertion.Assert(netInfo.name is not null);
                Log.Debug("SaveAssetPatch.Prefix: netInfo.name= " + netInfo.name.ToSTR());
                assetName = PackageHelper.StripName(netInfo.name);
                assetName = assetName.Remove("_Data");
            }
        }

        public static void Finalizer(Exception __exception) {
            if(__exception != null) {
                SimulationManager.instance.ForcedSimulationPaused = false;
                Log.Exception(__exception);
            }

        }
    }
}