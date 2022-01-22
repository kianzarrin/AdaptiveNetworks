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
                    Log.Debug("AssetDataExtension.OnAssetLoaded():  prefab is " + prefab, false);
                    if(userData.TryGetValue(ID_NetInfo, out byte[] data)) {
                        Log.Debug("AssetDataExtension.OnAssetLoaded(): extracted data for " + ID_NetInfo);
                        AssertNotNull(data, "data");
                        var assetData0 = SerializationUtil.Deserialize(data, default);
                        AssertNotNull(assetData0, "assetData0 | data version is too old for " + prefab);
                        var assetData = assetData0 as AssetData;
                        AssertNotNull(assetData, $"assetData: {assetData0.GetType()} is not ${typeof(AssetData)}");
                        AssetData.Load(assetData, prefab);
                        Log.Debug($"AssetDataExtension.OnAssetLoaded(): Asset Data={assetData} version={assetData.VersionString}");
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
                if(ToolsModifierControl.toolController.m_editPrefabInfo != null) {
                    Log.Info("Skipping hot reload of asset data in asset editor");
                    return;
                    /* I don't know why it does not work and some elevations are returning null.
                     * maybe it only fails if it loads the original copy of the loaded asset.
                     */
                }
                LogCalled();
                var assets2UserData = PluginUtil.GetLoadOrderMod()
                    ?.GetMainAssembly()
                    ?.GetType("LoadOrderMod.LOMAssetDataExtension", throwOnError: false)
                    ?.GetField("Assets2UserData")
                    ?.GetValue(null) 
                    as Dictionary<PrefabInfo, Dictionary<string, byte[]>>;

                if (null == assets2UserData) {
                    Log.Warning("Could not hot reload assets because LoadOrderMod was not found");
                    return;
                }

                foreach (var asset2UserData in assets2UserData) {
                    var asset = asset2UserData.Key;
                    var userData = asset2UserData.Value;
                    if(asset)
                        OnAssetLoadedImpl(asset.name, asset, userData);
                }

            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}