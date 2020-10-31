namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    using AdaptiveRoads.Manager;

    [HarmonyPatch(typeof(AssetImporterWizard), nameof(AssetImporterWizard.Complete))]
    [HarmonyPriority(100)]
    public static class AssetImporterWizard_Complete {
        public static void Postfix() {
            if (ToolsModifierControl.toolController.m_templatePrefabInfo is NetInfo) {
                NetInfoExtionsion.EnsureExtended_EditedNetInfos();
            }
        }
    }
}

