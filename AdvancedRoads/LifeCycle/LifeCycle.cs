namespace AdvancedRoads.LifeCycle
{
    using KianCommons;
    using System;
    using AdvancedRoads.Manager;
    using UnityEngine;

    public static class LifeCycle
    {
        //public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        //public static string HARMONY_ID = "CS.Kian." + AssemblyName;
        public static string HARMONY_ID = "CS.Kian.AdaptiveRoads" ;

        public static void Load()
        {
            try {
                Log.Info("LifeCycle.Load() called");
                #region TODO: DELETE
                var info = PrefabCollection<NetInfo>.FindLoaded("Basic Road Decoration Trees");
                var go = info.gameObject;
                Log.Debug($"{info} {go.GetComponents<Component>().ToSTR()}");
                #endregion

                // ensure buffer is large enough after everything has been loaded.
                // also extends loaded prefabs with indeces.
                NetInfoExt.ExpandBuffer();
                NetworkExtensionManager.Instance.OnLoad();
                NetInfoExt.EnsureEditNetInfoExt(); // useful for asset editor hot reload.
                HarmonyUtil.InstallHarmony(HARMONY_ID);
            }
            catch (Exception e) {
                Log.Error(e.ToString()+"\n --- \n");
                throw e;
            }
        }

        public static void Release()
        {
            Log.Info("LifeCycle.Release() called");
            //MainPanel.Release();
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
            Log.Info("setting NetInfoExt.Buffer = null");
            NetInfoExt.Buffer = null;
            NetworkExtensionManager.Instance.OnUnload();
        }
    }
}
