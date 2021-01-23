#if DEBUG
namespace AdaptiveRoads.Patches.AssetPatches {
    using HarmonyLib;
    using KianCommons;
    using AdaptiveRoads.Manager;

    [HarmonyPatch(typeof(AssetImporterWizard), nameof(AssetImporterWizard.Complete))]
    static class AssetImporterWizard_Complete {
        static void Prefix() =>
            Log.Debug("AssetImporterWizard.Complete().Prefix(): Creating new road based on existing road " +
                $"(ARMode={UI.ModSettings.ARMode}) ...");
        static void Postfix() => NetInfoExtionsion.EnsureExtended_EditedNetInfos();
}
}
#endif
