namespace AdaptiveRoads.Patches.RoadEditor.model {
    using AdaptiveRoads.Manager;
    using HarmonyLib;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), nameof(AssetEditorRoadUtils.ReleaseModel), typeof(NetInfo))]
    static class ReleaseModelPatch {
        static void Postfix(NetInfo info) =>
            info?.GetMetaData()?.ReleaseModels();
    }
}
