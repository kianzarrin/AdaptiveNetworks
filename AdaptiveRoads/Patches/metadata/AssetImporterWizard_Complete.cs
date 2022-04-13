namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;

    [HarmonyPatch(typeof(AssetImporterWizard), nameof(AssetImporterWizard.Complete))]
    [HarmonyPriority(100)]
    public static class AssetImporterWizard_Complete {

        public static void Postfix() {
            OnLoadPatch.Postfix();
        }

        static void FixSkippedCode() {
/*
            // This code is not run for NetInfo. The consequences are:
            this.m_CurrentAsset.FinalizeImport();
            if (this.m_PreviousAsset != null) {
                this.m_PreviousAsset.DestroyAsset(); // memory leak. not a big deal i guess
            }
            this.m_PreviousAsset = this.m_CurrentAsset; // will not remember last asset. so it can't be destroyed in the line above
            if (ToolsModifierControl.toolController.m_editPrefabInfo != null && !(ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo)) {
                PrefabInfo editPrefabInfo = ToolsModifierControl.toolController.m_editPrefabInfo;
                ToolsModifierControl.toolController.m_editPrefabInfo = null;
                UnityEngine.Object.DestroyImmediate(editPrefabInfo.gameObject); // will not destroy last NetInfo : memory leak.
            }
            ToolsModifierControl.toolController.m_editPrefabInfo = this.m_CurrentAsset.Object.GetComponent<PrefabInfo>(); // they remembered to copy this line over to the NetInfo specific code
            ToolsModifierControl.toolController.m_templatePrefabInfo = this.m_CurrentAsset.TemplateObject.GetComponent<PrefabInfo>(); // they remembered to copy this line over to the NetInfo specific code
            Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier = Guid.NewGuid().ToString(); // might cause problem with snapshots. not sure what other problem this causes.
            Singleton<SimulationManager>.instance.m_metaData.m_WorkshopPublishedFileId = PublishedFileId.invalid; // caveat: If I load a WS asset -> create new road -> save -> it will show steam icon beside it even though its not WS item.
            base.owner.Complete(); // they remembered to copy this line over to the NetInfo specific code
*/
        }
    }
}

