namespace AdvancedRoads.LifeCycle
{
    using KianCommons;
    using AdvancedRoads.UI.MainPanel;

    public static class LifeCycle
    {
        public static void Load()
        {
            Log.Info("LifeCycle.Load() called");
            HarmonyExtension.InstallHarmony();
            NetworkExtensionManager.Instance.OnLoad();
            MainPanel.Create();
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            MainPanel.Release();
            HarmonyExtension.UninstallHarmony();
        }
    }
}
