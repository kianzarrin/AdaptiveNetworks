namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using KianCommons;
    using HarmonyLib;

    /// <summary>
    /// recalculate cahced values.
    /// </summary>
    [HarmonyPatch(typeof(NetInfo), "InitializePrefab")]
    static class NetInfo_InitializePrefabPatch {
        static void Postfix(NetInfo __instance) {
            Log.Called(__instance);
            if(ToolsModifierControl.toolController.m_editPrefabInfo && // in game optimisation.
                UI.ModSettings.ARMode &&
                __instance.IsEditing()) {
                __instance.GetOrCreateMetaData();
            }
            __instance.GetMetaData()?.Recalculate(__instance);
        }
    }
}