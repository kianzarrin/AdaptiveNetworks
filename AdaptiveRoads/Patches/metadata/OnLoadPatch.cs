namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    using AdaptiveRoads.Manager;

    [HarmonyPatch(typeof(LoadAssetPanel), "OnLoad")]
    [HarmonyPriority(100)]
    public static class OnLoadPatch {
        public static void Postfix() {
            if (ToolsModifierControl.toolController.m_templatePrefabInfo is NetInfo) {
                // don't know why but the lods do not load the first time i load an asset.
                NetManager.instance.RebuildLods(); 
            }
        }
    }
}

