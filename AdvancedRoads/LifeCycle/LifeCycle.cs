namespace AdvancedRoads.LifeCycle
{
    using KianCommons;
    using AdvancedRoads.UI.MainPanel;
    using System;
    using AdvancedRoads.Manager;

    public static class LifeCycle
    {
        public static void Load()
        {
            try {
                Log.Info("LifeCycle.Load() called");
                // ensure buffer is large enough after everything has been loaded.
                // also extends loaded prefabs with indeces.
                NetInfoExt.ExpandBuffer(); 
                HarmonyExtension.InstallHarmony();
                NetworkExtensionManager.Instance.OnLoad();
                MainPanel.Create();
            }catch (Exception e) {
                Log.Error(e.ToString()+"\n --- \n");
                throw e;
            }
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            MainPanel.Release();
            HarmonyExtension.UninstallHarmony();
        }
    }
}
