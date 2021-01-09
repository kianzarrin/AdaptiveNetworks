namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    [HarmonyPatch(typeof(REModelCrossImportSet), "Validate")]
    public static class REModelCrossImportSet_Validate {
        public static bool Prefix(object target, ref bool __result) {
            __result = target is NetInfo.Segment || target is NetInfo.Node;
            return false;
        }
    }
}

