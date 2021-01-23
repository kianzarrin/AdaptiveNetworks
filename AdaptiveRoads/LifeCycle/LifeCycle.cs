namespace AdaptiveRoads.LifeCycle {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Patches;
    using AdaptiveRoads.UI.RoadEditor;
    using CitiesHarmony.API;
    using ICities;
    using KianCommons;
    using System;
    using System.Diagnostics;
    using UnityEngine.SceneManagement;
    using static KianCommons.ReflectionHelpers;

    public static class LifeCycle {
        public static string HARMONY_ID = "CS.Kian.AdaptiveRoads";
        public static string HARMONY_ID_MANUAL = "CS.Kian.AdaptiveRoads.Manual";

        public static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;
        public static LoadMode Mode => (LoadMode)UpdateMode;
        public static string Scene => SceneManager.GetActiveScene().name;

        public static bool Loaded;
        public static bool bHotReload = false;

        public static void Enable() {
            try {
                Log.Debug("Testing StackTrace:\n" + new StackTrace(true).ToString(), copyToGameLog: false);
                KianCommons.UI.TextureUtil.EmbededResources = false;
                HelpersExtensions.VERBOSE = false;
                Loaded = false;

                HarmonyHelper.EnsureHarmonyInstalled();
                //LoadingManager.instance.m_simulationDataReady += SimulationDataReady; // load/update data
                LoadingManager.instance.m_levelPreLoaded += Preload;
                if (LoadingManager.instance.m_loadingComplete)
                    HotReload();

#if FAST_TEST_HARMONY
                HarmonyHelper.DoOnHarmonyReady(() => {
                        HarmonyUtil.InstallHarmony(HARMONY_ID_MANUAL);
                        HarmonyUtil.InstallHarmony(HARMONY_ID);
                    });
#endif
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void HotReload() {
            LogCalled();
            bHotReload = true;
            Preload();
            //SimulationDataReady();
            Load();
            Util.RoadEditorUtils.RefreshRoadEditor();
        }

        public static void Disable() {
            //LoadingManager.instance.m_simulationDataReady -= SimulationDataReady;
            LoadingManager.instance.m_levelPreLoaded -= Preload;
            Unload(); // in case of hot unload
            Exit();
#if FAST_TEST_HARMONY
                HarmonyUtil.UninstallHarmony(HARMONY_ID);
                HarmonyUtil.UninstallHarmony(HARMONY_ID_MANUAL);
#endif

        }

        public static void Preload() {
            Log.Info("LifeCycle.Preload() called");
            PluginUtil.LogPlugins();
            if (!HideCrosswalksPatch.patched && PluginUtil.GetHideCrossings().IsActive()) {
                HarmonyUtil.ManualPatch(typeof(HideCrosswalksPatch), HARMONY_ID_MANUAL);
                HideCrosswalksPatch.patched = true;
            }
            HelpersExtensions.VERBOSE = false;
        }

        public static void Load() {
            try {
                Log.Info("LifeCycle.Load() called");
                Log.Info("testing stack trace:\n" + Environment.StackTrace, false);

                NetworkExtensionManager.Instance.OnLoad();
                HarmonyUtil.InstallHarmony(HARMONY_ID);
                NetInfoExtionsion.Ensure_EditedNetInfos();
                HintBox.Create();
                Log.Info("LifeCycle.Load() successfull!");

            } catch (Exception e) {
                Log.Error(e.ToString() + "\n --- \n");
                throw e;
            }
        }

        public static void Unload() {
            Log.Info("LifeCycle.Release() called");
            HintBox.Release();
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
            NetworkExtensionManager.Instance.OnUnload();
        }

        public static void Exit() {
            Log.Info("LifeCycle.Exit() called");
            HarmonyUtil.UninstallHarmony(HARMONY_ID_MANUAL);
            HideCrosswalksPatch.patched = false;
        }
    }
}
