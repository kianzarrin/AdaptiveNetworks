namespace AdaptiveRoads.LifeCycle {
    using AdaptiveRoads.Manager;
    using ICities;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using static KianCommons.Assertion;
    using KianCommons.Serialization;
    using AdaptiveRoads.UI;

    public class AssetDataExtension : AssetDataExtensionBase {
        public const string ID_NetInfo = "AdvancedRoadEditor_NetInfoExt";

        public static AssetDataExtension Instance;
        public override void OnCreated(IAssetData assetData) {
            base.OnCreated(assetData);
            Instance = this;
        }
        public override void OnReleased() {
            Instance = null;
        }

        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) {
            try {
                if (HelpersExtensions.InAssetEditor && ModSettings.VanillaMode)
                    return;
                Log.Debug($"AssetDataExtension.OnAssetLoaded({name}, {asset}, userData) called", false);
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

        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetSaved({name}, {asset}, userData) called");
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
    }
}