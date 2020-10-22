namespace AdaptiveRoads.Patches.metadata {
    using AdaptiveRoads.Patches.RoadEditor;
    using HarmonyLib;
    using AdaptiveRoads.Manager;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), "Instantiate")]
    public static class OnContinuePatch {
        /// <summary>
        /// clone list and metadata that
        /// AssetEditorRoadUtils has copied using CopyProperties()
        /// </summary>
        public static void Postfix() {
            if (ToolsModifierControl.toolController.m_templatePrefabInfo is NetInfo source) {
                NetInfo target = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
                foreach(var item in target.m_nodes) {
                    if (item is IInfoExtended item2)
                        item2.MetaData = item2.MetaData.Clone();
                }
                foreach (var item in target.m_segments) {
                    if (item is IInfoExtended item2)
                        item2.MetaData = item2.MetaData.Clone();
                }
                foreach (var lane in target.m_lanes) {
                    foreach (var item in lane.m_laneProps.m_props) {
                        if (item is IInfoExtended item2)
                            item2.MetaData = item2.MetaData.Clone();
                    }
                }
            }
        }
    }
}

