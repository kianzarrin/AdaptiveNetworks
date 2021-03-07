namespace AdaptiveRoads.Patches.metadata {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons.Plugins;


    [HarmonyPatch(typeof(LoadAssetPanel), "OnLoad")]
    [HarmonyPriority(100)]
    public static class OnLoadPatch {
        public static void Postfix() {
            if(ToolsModifierControl.toolController.m_templatePrefabInfo is NetInfo) {
                if(PluginUtil.GetNetworkSkins().IsActive()) {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                            "Incompatible mod",
                            "NS2 is incompatible with AR in Road editor.",
                            false);
                }

                // don't know why but the lods do not load the first time i load an asset.
                NetManager.instance.RebuildLods();
            }
        }
    }
}

