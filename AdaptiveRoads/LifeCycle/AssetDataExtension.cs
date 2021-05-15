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
    using ColossalFramework.Packaging;
    using ColossalFramework;

    public class AssetDataExtension : IAssetDataExtension {
        public const string ID_NetInfo = "AdvancedRoadEditor_NetInfoExt";
        public void OnCreated(IAssetData assetData) { }
        public void OnReleased() { }

        public void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) =>
            OnAssetLoadedImpl(name, asset, userData);

        // asset should be the same as ToolsModifierControl.toolController.m_editPrefabInfo
        public void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData) =>
            OnAssetSavedImpl(name, asset, out userData);

        public static void OnAssetLoadedImpl(string name, object asset, Dictionary<string, byte[]> userData) {
            try {
                if (HelpersExtensions.InAssetEditor && ModSettings.VanillaMode)
                    return;
                Log.Debug($"AssetDataExtension.OnAssetLoadedImpl({name}, {asset}, userData) called", false);
                if (asset is NetInfo prefab) {
                    Log.Debug("AssetDataExtension.OnAssetLoaded():  prefab is " + prefab, false);
                    if (userData.TryGetValue(ID_NetInfo, out byte[] data)) {
                        Log.Debug("AssetDataExtension.OnAssetLoaded(): extracted data for " + ID_NetInfo);
                        AssertNotNull(data, "data");
                        var assetData0 = SerializationUtil.Deserialize(data, default);
                        AssertNotNull(assetData0, "assetData0 | data version is too old for " + prefab);
                        var assetData = assetData0 as AssetData;
                        AssertNotNull(assetData, $"assetData: {assetData0.GetType()} is not ${typeof(AssetData)}");
                        AssetData.Load(assetData, prefab);
                        Log.Debug($"AssetDataExtension.OnAssetLoaded(): Asset Data={assetData} version={assetData.VersionString}");
                    }
                } else if (asset is BuildingInfo buildingInfo) {
                    // TODO: load stored custom road flags for intersections or buildings.
                }
            } catch (Exception e) {
                Log.Exception(e, $"asset:{asset} name:{name}");
            }
        }

        public static void OnAssetSavedImpl(string name, object asset, out Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetSavedImpl({name}, {asset}, userData) called");
            userData = null;
            if (ModSettings.VanillaMode) {
                Log.Info("MetaData not saved vanilla mode is set in the settings");
                return;
            }

            if (asset is NetInfo prefab) {
                Log.Info("AssetDataExtension.OnAssetSaved():  prefab is " + prefab);
                AssertNotNull(AssetData.Snapshot,"snapshot");
                var assetData = AssetData.Snapshot; //AssetData.CreateFromEditPrefab();
                Log.Debug("AssetDataExtension.OnAssetSaved(): assetData=" + assetData);
                userData = new Dictionary<string, byte[]>();
                userData.Add(ID_NetInfo, SerializationUtil.Serialize(assetData));
            }
        }

        public static bool InRoadEditor => ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo;

        public static void BeforeSave() {
            try {
                if (ModSettings.VanillaMode || !InRoadEditor) return;
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
            Log.Debug("AssetDataExtension.BeforeSave() was successfull");
        }

        public static void AfterSave() {
            try {
                if (ModSettings.VanillaMode || !InRoadEditor) return;
                Log.Debug($"SaveAssetPanel.SaveRoutine re extending ...");
                foreach (var info in NetInfoExtionsion.EditedNetInfos) {
                    info.UndoVanillaForbidden();
                }
                AssetData.ApplySnapshot();
                NetInfoExtionsion.InvokeEditPrefabChanged();
            } catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
        }

        public static void HotReload() {
            try {
                LogCalled();
                SteamHelper.DLC_BitMask ownedMask = SteamHelper.GetOwnedDLCMask();
                var filter = new Package.AssetType[] { UserAssetType.CustomAssetMetaData };
                foreach (Package.Asset asset in PackageManager.FilterAssets(filter)) {
                    if (asset == null || !asset.isEnabled)
                        continue;
                    if (asset.Instantiate<CustomAssetMetaData>() is not CustomAssetMetaData customAssetMetaData)
                        continue;
                    SteamHelper.DLC_BitMask assetDLCMask = AssetImporterAssetTemplate.GetAssetDLCMask(customAssetMetaData);
                    if ((assetDLCMask & ownedMask) != assetDLCMask)
                        continue;

                    if (customAssetMetaData.type != CustomAssetMetaData.Type.RoadElevation &&
                        customAssetMetaData.type != CustomAssetMetaData.Type.Road)
                        continue;

                    if (customAssetMetaData.userDataRef?.Instantiate() is not AssetDataWrapper.UserAssetData userAssetData)
                        continue;
                    OnAssetLoadedImpl(
                        customAssetMetaData.name,
                        ToolsModifierControl.toolController.m_editPrefabInfo,
                        userAssetData.Data);
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}