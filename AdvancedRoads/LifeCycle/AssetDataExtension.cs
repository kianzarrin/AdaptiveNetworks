namespace AdvancedRoads.LifeCycle {
    using System.Collections.Generic;
    using ColossalFramework.UI;
    using ICities;
    using HarmonyLib;
    using KianCommons;
    using static KianCommons.HelpersExtensions;
    using AdvancedRoads.Manager;
    using PrefabIndeces;
    using System;

    [HarmonyPatch(typeof(LoadAssetPanel), "OnLoad")]
    public static class OnLoadPatch {
        /// <summary>
        /// when loading asset from a file, IAssetData.OnAssetLoaded() is called for all assets but the one that is loaded from the file.
        /// this postfix calls IAssetData.OnAssetLoaded() for asset loaded from file.
        /// </summary>
        public static void Postfix(LoadAssetPanel __instance, UIListBox ___m_SaveList) {
            // Taken from LoadAssetPanel.OnLoad
            var selectedIndex = ___m_SaveList.selectedIndex;
            var getListingMetaDataMethod = typeof(LoadSavePanelBase<CustomAssetMetaData>).GetMethod(
                "GetListingMetaData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var listingMetaData = (CustomAssetMetaData)getListingMetaDataMethod.Invoke(__instance, new object[] { selectedIndex });

            // Taken from LoadingManager.LoadCustomContent
            if (listingMetaData.userDataRef != null) {
                AssetDataWrapper.UserAssetData userAssetData = listingMetaData.userDataRef.Instantiate() as AssetDataWrapper.UserAssetData;
                if (userAssetData == null) {
                    userAssetData = new AssetDataWrapper.UserAssetData();
                }
                AssetDataExtension.Instance.OnAssetLoaded(listingMetaData.name, ToolsModifierControl.toolController.m_editPrefabInfo, userAssetData.Data);
            }
        }
    }

    [Serializable]
    public class AssetData {
        public NetInfoExt Ground, Elevated, Bridge, Slope, Tunnel;

        public static AssetData CreateFromEditPrefab() {
            NetInfo ground = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
            if (ground == null)
                return null;
            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);
            var ret = new AssetData();
            ret.Ground = NetInfoExt.Buffer[ground.GetIndex()];
            return ret;
        }

        public static void Load(AssetData assetData, NetInfo groundInfo) {
            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(groundInfo);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(groundInfo);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(groundInfo);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(groundInfo);

            if(groundInfo) NetInfoExt.SetNetInfoExt(groundInfo.GetIndex(), assetData.Ground);
            if(elevated) NetInfoExt.SetNetInfoExt(elevated.GetIndex(), assetData.Elevated);
            if(bridge) NetInfoExt.SetNetInfoExt(bridge.GetIndex(), assetData.Bridge);
            if(slope) NetInfoExt.SetNetInfoExt(slope.GetIndex(), assetData.Slope);
            if(tunnel) NetInfoExt.SetNetInfoExt(tunnel.GetIndex(), assetData.Tunnel);
        }

    }

    public class AssetDataExtension : AssetDataExtensionBase {
        public const string ID_NetInfo = "AdvancedRoadEditor_NetInfoExt";


        public static AssetDataExtension Instance;
        public override void OnCreated(IAssetData assetData) {
            base.OnCreated(assetData);
            Instance = this;
            NetInfoExt.Init();
        }
        public override void OnReleased() {
            NetInfoExt.Buffer = null;
            Instance = null;
        }

        // TODO serialize BuildgInfo
        // TODO clone custom flags when netinfo is cloned by asset editor. 
        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetLoaded({name}, {asset}, userData) called");
            if (asset is NetInfo prefab) {
                Log.Debug("AssetDataExtension.OnAssetLoaded():  prefab is " + prefab);
                if (userData.TryGetValue(ID_NetInfo, out byte[] data)) {
                    Log.Info("AssetDataExtension.OnAssetLoaded(): extracted data for " + ID_NetInfo);
                    var assetData = SerializationUtil.Deserialize(data) as AssetData;
                    AssertNotNull(assetData, "assetData");
                    AssetData.Load(assetData, prefab);
                    Log.Debug("AssetDataExtension.OnAssetLoaded(): Asset Data=" + assetData);
                }
            }
            else if(asset is BuildingInfo buildingInfo) {
                // load stored custom road flags for intersections or buildings.
            }
        }

        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetSaved({name}, {asset}, userData) called");
            userData = null;
            if (asset is NetInfo prefab) {
                Log.Info("AssetDataExtension.OnAssetSaved():  prefab is " + prefab);
                var assetData = AssetData.CreateFromEditPrefab();
                Log.Debug("AssetDataExtension.OnAssetSaved(): assetData=" + assetData);
                userData = new Dictionary<string, byte[]>();
                userData.Add(ID_NetInfo, SerializationUtil.Serialize(assetData));
            }
        }
    }


}
