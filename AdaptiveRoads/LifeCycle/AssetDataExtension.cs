namespace AdaptiveRoads.LifeCycle {
    using AdaptiveRoads.Manager;
    using ICities;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using static KianCommons.Assertion;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Serialization;
    using AdaptiveRoads.UI;
    using KianCommons.Plugins;
    using ColossalFramework.Packaging;
    using ColossalFramework.UI;

    public class AssetDataExtension : IAssetDataExtension {
        public const string ID_NetInfo = "AdvancedRoadEditor_NetInfoExt";
        public static bool InRoadEditor => ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo;
        public static string SaveName;
        public static NetInfo CurrentBasicNetInfo;
        public static CustomAssetMetaData ListingMetaData;

        public void OnCreated(IAssetData assetData) { }
        public void OnReleased() { }

        public void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) =>
            OnAssetLoadedImpl(name, asset, userData);

        // asset should be the same as ToolsModifierControl.toolController.m_editPrefabInfo
        public void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData) =>
            OnAssetSavedImpl(name, asset, out userData);

        public static void OnAssetLoadedImpl(string name, object asset, Dictionary<string, byte[]> userData) {
            try {
                if(HelpersExtensions.InAssetEditor && ModSettings.VanillaMode)
                    return;
                Log.Debug($"AssetDataExtension.OnAssetLoadedImpl({name}, {asset}, userData) called", false);
                if(asset is NetInfo prefab) {
                    CurrentBasicNetInfo = prefab;
                    Log.Debug("AssetDataExtension.OnAssetLoadedImpl():  prefab is " + prefab, false);
                    if(userData.TryGetValue(ID_NetInfo, out byte[] data)) {
                        Log.Debug("AssetDataExtension.OnAssetLoadedImpl(): extracted data for " + ID_NetInfo);
                        AssertNotNull(data, "data");
                        var assetData0 = SerializationUtil.Deserialize(data, default);
                        AssertNotNull(assetData0, $"assetData0 | deserialization failed for '{prefab}'. data version = V{SerializationUtil.DeserializationVersion}");
                        var assetData = assetData0 as AssetData;
                        AssertNotNull(assetData, $"assetData: {assetData0.GetType()} is not ${typeof(AssetData)}");
                        AssetData.Load(assetData, prefab);
                        Log.Debug($"AssetDataExtension.OnAssetLoadedImpl(): Asset Data={assetData} version={assetData.VersionString}");
                    }
                } else if(asset is BuildingInfo buildingInfo) {
                    // TODO: load stored custom road flags for intersections or buildings.
                }
            } catch(Exception e) {
                Log.Exception(e, $"asset:{asset} name:{name}");
            } finally {
                CurrentBasicNetInfo = null; ;
            }
        }

        public static void OnAssetSavedImpl(string name, object asset, out Dictionary<string, byte[]> userData) {
            try {
                if(Log.VERBOSE) Log.Called(name, asset, "userData");
                userData = null;
                if (ModSettings.VanillaMode) {
                    Log.Info("MetaData not saved vanilla mode is set in the settings");
                    return;
                }

                if (asset is NetInfo prefab) {
                    CurrentBasicNetInfo = prefab;
                    if(Log.VERBOSE) Log.Info(ThisMethod + " :  prefab is " + prefab);
                    AssertNotNull(AssetData.Snapshot, "snapshot");
                    var assetData = AssetData.Snapshot; //AssetData.CreateFromEditPrefab();
                    if(Log.VERBOSE) Log.Debug(ThisMethod + " : assetData =" + assetData);
                    userData = new Dictionary<string, byte[]>();
                    userData.Add(ID_NetInfo, SerializationUtil.Serialize(assetData));
                }
            } catch (Exception ex) {
                ex.Log();
                userData = null;
            } finally {
                CurrentBasicNetInfo = null; ;
            }
        }

        public static void BeforeSave(string saveName) {
            try {
                SaveName = saveName;
                if(!InRoadEditor) return;
                if(ModSettings.VanillaMode) {
                    // just in case there is extended infos by mistake.
                    // in AN mode I first need to pause simulation
                    NetInfoExtionsion.Ensure_EditedNetInfos();
                    return;
                }
                AssetData.TakeSnapshot();
                Log.Debug($"AssetDataExtension.BeforeSave(): reversing ...");
                SimulationManager.instance.ForcedSimulationPaused = true;
                foreach (var info in NetInfoExtionsion.EditedNetInfos)
                    info.ApplyVanillaForbidden();
                NetInfoExtionsion.UndoExtend_EditedNetInfos();
            } catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
            Log.Debug("AssetDataExtension.BeforeSave() was successful");
        }

        public static void AfterSave() {
            try {
                if(ModSettings.VanillaMode || !InRoadEditor) return;
                Log.Debug($"AfterSave re extending ...");
                foreach(var info in NetInfoExtionsion.EditedNetInfos) {
                    info.UndoVanillaForbidden();
                }
                AssetData.ApplySnapshot();
                NetInfoExtionsion.InvokeEditPrefabChanged();
            } catch(Exception e) {
                Log.Exception(e);
                throw e;
            } finally {
                SaveName = null;
                SimulationManager.instance.ForcedSimulationPaused = false;
            }
        }

        public static void HotReload() {
            try {
                LogCalled();
                var prefabs2UserData = PluginUtil.GetLoadOrderMod()
                    ?.GetMainAssembly()
                    ?.GetType("LoadOrderMod.LOMAssetDataExtension", throwOnError: false)
                    ?.GetField("Assets2UserData")
                    ?.GetValue(null)
                    as Dictionary<PrefabInfo, Dictionary<string, byte[]>>;

                if (null == prefabs2UserData) {
                    Log.Warning("Could not hot reload assets because LoadOrderMod was not found");
                    return;
                }
                NetInfo editPrefab = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
                Log.Debug("editPrefab=" + editPrefab);

                // load all assets loaded during level load
                foreach (var prefab2UserData in prefabs2UserData) {
                    var prefab = prefab2UserData.Key;
                    var userData = prefab2UserData.Value;
                    if (prefab is NetInfo netInfo) {
                        if (editPrefab) {
                            // work around for duplications in asset editor.
                            netInfo = FindLoadedCounterPart(netInfo);
                        }
                        OnAssetLoadedImpl(netInfo.name, netInfo, userData);
                    }
                }

                // load edited prefab user data:
                if (editPrefab) {
                    bool? lastLoaded = WasLastLoaded;
                    if (!WasLastLoaded.HasValue) {
                        Log.Warning("Last loaded state not recorded");
                    } else if (!lastLoaded.Value) {
                        // edit prefab was cloned.
                        NetInfo templatePrefab = ToolsModifierControl.toolController.m_templatePrefabInfo as NetInfo;
                        if (templatePrefab) {
                            NetInfo loadedNetInfo = FindLoadedCounterPart(templatePrefab);
                            AssetData.NetInfoMetaData.CopyMetadata(loadedNetInfo, editPrefab);
                        }
                    } else {
                        // edit prefab was loaded
                        try {
                            var lastAssetMetaData = ListingMetaData = GetLastLoadedMetaData(); // last loaded asset
                            if (lastAssetMetaData?.userDataRef is Package.Asset asset) {
                                var data = asset.Instantiate<AssetDataWrapper.UserAssetData>()?.Data;
                                if (data != null) {
                                    OnAssetLoadedImpl(lastAssetMetaData.name, editPrefab, data);
                                }
                            }
                        } catch (Exception ex) {

                        } finally {
                            ListingMetaData = null;
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        /// <summary>
        // OnLoad() calls IntializePrefab() which can create duplicates.
        // so we have to match by name.
        /// </summary>
        static NetInfo FindLoadedCounterPart(NetInfo source) {
            if(Log.VERBOSE) Log.Called(source);
            if (source?.name != null) {
                int n = PrefabCollection<NetInfo>.LoadedCount();
                for (uint i = 0; i < n; ++i) {
                    var prefab = PrefabCollection<NetInfo>.GetLoaded(i);
                    if (prefab?.name == source.name) {
                        return prefab;
                    }
                }
            }
            return source;
        }

        static LoadAssetPanel GetLoadAssetPanel() => UIView.library.Get<LoadAssetPanel>(nameof(LoadAssetPanel));
        static CustomAssetMetaData GetLastLoadedMetaData() {
            var loadAssetPanel = GetLoadAssetPanel();
            UIListBox m_SaveList = GetFieldValue(loadAssetPanel, "m_SaveList") as UIListBox;
            return (CustomAssetMetaData)InvokeMethod(loadAssetPanel, "GetListingMetaData", m_SaveList.selectedIndex);
        }

        public static bool? WasLastLoaded {
            get {
                if (GetLoadAssetPanel().component.objectUserData is bool lastLoaded)
                    return lastLoaded;
                else
                    return null;
            }
            set {
                GetLoadAssetPanel().component.objectUserData = value;
            }
        }
    }
}