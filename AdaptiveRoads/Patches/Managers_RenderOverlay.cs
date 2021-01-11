namespace AdaptiveRoads.Patches {
    using AdaptiveRoads.UI.RoadEditor;
    using HarmonyLib;

    [HarmonyPatch(typeof(RenderManager), "Managers_RenderOverlay")]
    public static class Managers_RenderOverlay {
        public static void Postfix(RenderManager.CameraInfo cameraInfo) =>
            Overlay.RenderOverlay(cameraInfo);
    }
}

