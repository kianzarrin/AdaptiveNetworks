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
            __instance.GetMetaData()?.Recalculate(__instance);
        }
    }
}