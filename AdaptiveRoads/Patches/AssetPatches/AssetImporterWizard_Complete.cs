#if DEBUG
namespace AdaptiveRoads.Patches.AssetPatches {
    using HarmonyLib;
    using KianCommons;

    [HarmonyPatch(typeof(AssetImporterWizard), nameof(AssetImporterWizard.Complete))]
    public static class AssetImporterWizard_Complete {
        public static void Prefix() =>
            Log.Debug("AssetImporterWizard.Complete().Prefix(): Creating new road based on existing road ...");  
    }
}
#endif
