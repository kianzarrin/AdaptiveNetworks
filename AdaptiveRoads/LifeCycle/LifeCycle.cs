namespace AdaptiveRoads.LifeCycle {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Patches;
    using AdaptiveRoads.UI.RoadEditor;
    using CitiesHarmony.API;
    using ICities;
    using KianCommons;
    using KianCommons.Plugins;
    using System;
    using System.Diagnostics;
    using UnityEngine.SceneManagement;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.Util;
    using AdaptiveRoads.UI.Tool;
    using AdaptiveRoads.NSInterface;
    using KianCommons.Serialization;
    using UnityEngine;
    using AdaptiveRoads.Patches.RoadEditor.model;
    using static AdaptiveRoads.Util.Shortcuts;
    using ColossalFramework.Plugins;
    using AdaptiveRoads.UI.VBSTool;
    using AdaptiveRoads.UI;
    using System.Linq;

    public static class LifeCycle {
        public static string HARMONY_ID = "CS.Kian.AdaptiveRoads";
        public static string HARMONY_ID_MANUAL = "CS.Kian.AdaptiveRoads.Manual";

        public static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;
        public static LoadMode Mode => (LoadMode)UpdateMode;
        public static string Scene => SceneManager.GetActiveScene().name;

        public static bool bHotReload = false;

        public static void Enable() {
            try {
#if DEBUG
                Log.Debug("Testing StackTrace:\n" + new StackTrace(true).ToString(), copyToGameLog: false);
                var ANPlugin = PluginUtil.GetAdaptiveRoads();
#endif

                KianCommons.UI.TextureUtil.EmbededResources = false;
                Log.ReadCommandLineArgs();

                HarmonyHelper.EnsureHarmonyInstalled();
                ANImplementation.Install();

                //LoadingManager.instance.m_simulationDataReady += SimulationDataReady; // load/update data
                LoadingManager.instance.m_levelPreLoaded += Preload;

                if (LoadingManager.instance.m_loadingComplete)
                    HotReload();

                PluginManager.instance.eventPluginsStateChanged += HotReload3rdparty;
                PluginManager.instance.eventPluginsChanged += HotReload3rdparty;
#if FAST_TEST_HARMONY
                HarmonyHelper.DoOnHarmonyReady(() => {
                    HarmonyUtil.InstallHarmony(HARMONY_ID);
                    Process.GetCurrentProcess().Kill();
                });
#endif
                //Test();
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

#if DEBUG
        static void Test() {
            return;
            LogCalled();
            byte[] data;
            {
                var man = NetworkExtensionManager.CreateNew();
                man.LaneBuffer[1].m_flags = NetLaneExt.Flags.Custom0;
                Log.Debug("Serialize lane flags:" + man.LaneBuffer[1].m_flags);
                var s = SimpleDataSerializer.Writer(new Version(1, 1), 100);
                man.Serialize(s);
                data = s.GetBytes();
            }

            {
                var man = NetworkExtensionManager.CreateNew();
                Log.Debug("Before Deserialize lane flags :" + man.LaneBuffer[1].m_flags);
                var s = SimpleDataSerializer.Reader(data);
                man.DeserializeImp(s);
                Log.Debug("After Deserialize lane flags :" + man.LaneBuffer[1].m_flags);
            }
        }
#endif

        public static void HotReload3rdparty() {
            if (loaded_) {
                // check for loaded_ to avoid detecting AN hotreload as 3rdparty hotreload.
                Log.Called();
                if (PluginUtil.GetTrafficManager().IsActive()) {
                    var notifier = TMPENotifier;
                    if (notifier != null) {
                        notifier.EventLevelLoaded -= OnTMPELoaded;
                        notifier.EventLevelLoaded += OnTMPELoaded;
                        OnTMPELoaded();
                    }
                }
            }
        }

        public static void HotReload() {
            LogCalled();
            bHotReload = true;
            Preload();
            AssetDataExtension.HotReload();
            //SimulationDataReady();
            NetworkExtensionManager.Instance.HotReload();
            Load();
            RoadEditorUtils.RefreshRoadEditor();
        }

        public static void Disable() {
            Log.Buffered = false;
            ANImplementation.Uninstall();

            //LoadingManager.instance.m_simulationDataReady -= SimulationDataReady;
            LoadingManager.instance.m_levelPreLoaded -= Preload;
            Unload(); // in case of hot unload
            Exit();
#if FAST_TEST_HARMONY
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
#endif
        }

        static bool preloadPatchesApplied_ = false;
        public static void Preload() {
            try {
                Log.Info("LifeCycle.Preload() called");
                PluginUtil.LogPlugins();
                TrackManager.Ensure();
                if (!preloadPatchesApplied_) {
                    HarmonyUtil.InstallHarmony<PreloadPatchAttribute>(HARMONY_ID_MANUAL);
                    preloadPatchesApplied_ = true;
                }
                TMPENotifier.EventLevelLoaded -= OnTMPELoaded;
                TMPENotifier.EventLevelLoaded += OnTMPELoaded;
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static bool tmpeLoaded_ = false;
        static bool loaded_ = false;
        public static void OnTMPELoaded() {
            tmpeLoaded_ = true;
            if (loaded_) {
                // if TMPE was loaded last, then update everything here.
                NetworkExtensionManager.OnTMPELoaded();
            }
        }

        public static void Load() {
            try {
                Log.Info("LifeCycle.Load() called");
                Log.Debug("testing stack trace:\n" + Environment.StackTrace, false);
                Log.Info($"Scene={Scene} LoadMode={Mode}");

                loaded_ = true;
                _ = NetworkExtensionManager.Instance;

                Log.Info($"Scene={Scene} LoadMode={Mode}");
                if(Scene != "AssetEditor") {
                    Log.Info("Applying in game patches");
                    HarmonyUtil.InstallHarmony(HARMONY_ID, required: typeof(InGamePatchAttribute), forbidden: typeof(PreloadPatchAttribute));
                } else {
                    Log.Info("Applying all patches");
                    HarmonyUtil.InstallHarmony(HARMONY_ID, forbidden:typeof(PreloadPatchAttribute));
                    HintBox.Create();
                    VBSTool.Create().enabled = false;
                    ANWhatsNew.Instance.Regsiter();
                }

                NetInfoExtionsion.Ensure_EditedNetInfos(recalculate:true);


                ARTool.Create().enabled = false;

#if DEBUG
                const bool testPWValues = false;
                if (testPWValues) {
                    UI.Debug.PWSelector.Create();
                    UI.Debug.PWModifier.Create();
                }
#endif

                if (tmpeLoaded_) {
                    // wait to load TMPE
                    NetworkExtensionManager.OnTMPELoaded();
                }

                Log.Flush();
                Log.Succeeded();
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void Unload() {
            try {
                LogCalled();
                loaded_ = tmpeLoaded_ = false;
#if DEBUG
                UI.Debug.PWSelector.Release();
                UI.Debug.PWModifier.Release();
#endif
                VBSTool.Release();
                ARTool.Release();
                HintBox.Release();
                HarmonyUtil.UninstallHarmony(HARMONY_ID);
                ImportScalePatch.Release();
                NetworkExtensionManager.RawInstance?.OnUnload();
            }catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void Exit() {
            Log.Buffered = false;
            Log.Info("LifeCycle.Exit() called");
            PluginManager.instance.eventPluginsStateChanged -= HotReload3rdparty;
            PluginManager.instance.eventPluginsChanged -= HotReload3rdparty;
            TMPENotifier.EventLevelLoaded -= NetworkExtensionManager.OnTMPELoaded;
            HarmonyUtil.UninstallHarmony(HARMONY_ID_MANUAL);
            preloadPatchesApplied_ = false;
            if(TrackManager.exists) GameObject.Destroy(TrackManager.instance.gameObject);
        }

    }
}
