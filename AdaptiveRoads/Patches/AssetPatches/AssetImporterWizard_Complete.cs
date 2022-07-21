namespace AdaptiveRoads.Patches.AssetPatches {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;

    /// <summary>
    /// Applies metadata from source to target prefab
    /// </summary>
    [HarmonyPatch(typeof(AssetImporterWizard), nameof(AssetImporterWizard.Complete))]
    static class AssetImporterWizard_Complete {
        static void Prefix() {
            Log.Debug("AssetImporterWizard.Complete().Prefix(): Creating new road based on existing road " +
                $"(ARMode={UI.ModSettings.ARMode}) ...");
            NetInfoExtionsion.Ensure_EditedNetInfos(recalculate: true);
        }
    }
}
