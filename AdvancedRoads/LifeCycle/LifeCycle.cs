namespace AdvancedRoads.LifeCycle
{
    using KianCommons;
    using AdvancedRoads.UI.MainPanel;
    using System;
    using AdvancedRoads.Manager;
    using KianCommons.Patches;
    using System.Reflection;

    public static class LifeCycle
    {
        //public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        //public static string HARMONY_ID = "CS.Kian." + AssemblyName;
        public static string HARMONY_ID = "CS.Kian.AdaptiveRoads" ;

        public static void Load()
        {
            try {
                Log.Info("LifeCycle.Load() called");
                // ensure buffer is large enough after everything has been loaded.
                // also extends loaded prefabs with indeces.
                NetInfoExt.ExpandBuffer();
                HarmonyUtil.InstallHarmony(HARMONY_ID);
                NetworkExtensionManager.Instance.OnLoad();
                //MainPanel.Create();
            }catch (Exception e) {
                Log.Error(e.ToString()+"\n --- \n");
                throw e;
            }
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            MainPanel.Release();
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
        }
    }
}
