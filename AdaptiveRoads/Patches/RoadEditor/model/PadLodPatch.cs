namespace AdaptiveRoads.Patches.RoadEditor.model {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;

    [HarmonyPatch(typeof(AssetImporterTextureLoader), "LoadTextures")]
    public static class PadLodPatch {
        [UsedImplicitly]
        static void Prefix(ref bool generatePadding) {
            Log.Called();
            if (generatePadding)
                generatePadding = PaddingTogglePatch.PadLod.value;
        }
    }
}
