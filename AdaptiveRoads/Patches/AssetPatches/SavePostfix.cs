namespace AdaptiveRoads.Patches.AssetPatches {
    using ColossalFramework.Packaging;
    using HarmonyLib;
    using LifeCycle;

    //ColossalFramework.Packaging.Package.Save(string, bool)
    [HarmonyPatch(typeof(Package), nameof(Package.Save),
        new[] { typeof(string), typeof(bool) })]
    public static class SavePostfix {
        public static void Postfix() =>
            SimulationManager.instance.m_ThreadingWrapper.QueueMainThread(AssetDataExtension.AfterSave);
    }
}
