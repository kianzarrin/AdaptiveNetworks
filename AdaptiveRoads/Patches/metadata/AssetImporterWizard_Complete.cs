namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;

    [HarmonyPatch(typeof(AssetImporterWizard), nameof(AssetImporterWizard.Complete))]
    [HarmonyPriority(100)]
    public static class AssetImporterWizard_Complete {
        public static void Postfix() => OnLoadPatch.Postfix();
    }
}

