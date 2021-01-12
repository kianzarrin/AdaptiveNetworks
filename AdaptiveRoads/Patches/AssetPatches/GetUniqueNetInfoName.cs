namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    using AdaptiveRoads.Util;

    /// <summary>
    /// prevents roads from being renamed while loading.
    /// it adds prefix instead of postfix to create unique name
    /// so that they would be stripped away.
    /// </summary>
    [HarmonyPatch(typeof(AssetEditorRoadUtils), "GetUniqueNetInfoName")]
    public static class GetUniqueNetInfoName {
        static bool Prefix(string name, ref string __result) {
            __result = RoadUtils.GetUniqueNetInfoName(name);
            return false;
        }
    }
}


