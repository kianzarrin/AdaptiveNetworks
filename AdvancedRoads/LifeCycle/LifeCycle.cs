namespace AdvancedRoads.LifeCycle
{
    using AdvancedRoads.Tool;
    using AdvancedRoads.Util;

    public static class LifeCycle
    {
        public static void Load()
        {
            Log.Info("LifeCycle.Load() called");
            PluginUtil.Init();
            HarmonyExtension.InstallHarmony();
            AdvancedRoadsTool.Create();
            NetworkExtensionManager.Instance.OnLoad();
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            HarmonyExtension.UninstallHarmony();
            AdvancedRoadsTool.Remove();
        }
    }
}
