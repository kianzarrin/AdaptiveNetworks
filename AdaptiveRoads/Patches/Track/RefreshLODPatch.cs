namespace AdaptiveRoads.Patches.Track {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;

    [HarmonyPatch(typeof(NetInfo), "RefreshLevelOfDetail")]
    [HarmonyAfter("boformer.TrueLodToggler")]
    class RefreshLODPatch {
        public static void Postfix(NetInfo __instance) {
            Log.Called();
            __instance?.GetMetaData()?.RefreshLevelOfDetail(__instance);
        }
    }
}
