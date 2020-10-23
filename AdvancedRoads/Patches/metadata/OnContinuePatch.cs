namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    using AdaptiveRoads.Manager;

    [HarmonyPriority(100)]
    [HarmonyPatch(typeof(AssetImporterAssetTemplate), "OnContinue")]
    public static class OnContinuePatch {
        public static void Postfix() {
            if (ToolsModifierControl.toolController.m_templatePrefabInfo is NetInfo) {
                NetInfoExtionsion.EnsureEditedNetInfosExtended();
            }
        }
    }
}

