namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;
    using KianCommons;
    using AdaptiveRoads.Manager;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), "GetUniqueNetInfoName")]
    public static class GetUniqueNetInfoName {
        static bool Unique => false;
        static bool Prefix(string name, ref string __result) {
            if (Unique)return true; //default
            __result = PackageHelper.StripName(name);
            return false;
        }
    }
}


