namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    using AdaptiveRoads.Manager;
    [HarmonyPatch(typeof(REModelCrossImportSet), "Validate")]
    public static class REModelCrossImportSet_Validate {
        public static void Postfix(object target, ref bool __result) {
            __result |= target is NetInfo.Segment || target is NetInfo.Node;
        }
    }
}

