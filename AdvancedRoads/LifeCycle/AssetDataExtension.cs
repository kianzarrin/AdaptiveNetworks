namespace AdaptiveRoads.LifeCycle {
    using AdaptiveRoads.Manager;
    using ColossalFramework.UI;
    using HarmonyLib;
    using ICities;
    using KianCommons;
    using PrefabIndeces;
    using System;
    using System.Collections.Generic;
    using static KianCommons.Assertion;

    // TODO move to prefab indeces.
    [HarmonyPatch(typeof(SaveAssetPanel), "SaveAsset")]
    public static class SaveRoutinePatch {
        public static void Prefix() {
            Log.Debug($"SaveAssetPanel.SaveRoutine reversing ...");
            foreach (var info in NetInfoExt.EditNetInfos)
                info.ReversePrefab();
        }
        public static void PostFix() {
            Log.Debug($"SaveAssetPanel.SaveRoutine re extending ...");
            foreach (var info in NetInfoExt.EditNetInfos)
                info.ExtendPrefab();
        }
    }

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

    //private void AssetImporterAssetTemplate::OnContinue(UIComponent comp, UIMouseEventParameter p)
    [HarmonyPatch(typeof(AssetImporterAssetTemplate), "OnContinue")]
    public static class OnContinuePatch {
        /// <summary>
        /// copy NetInfoExt when road editor is create new asset based on another road.
        /// </summary>
        public static void Postfix() {
            if (ToolsModifierControl.toolController.m_templatePrefabInfo is NetInfo source) {
                NetInfo target = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
                NetInfoExt.CopyAll(source: source, target: target, forceCreate:true);
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

            var ret = new AssetData {
                Ground = ground.GetExt(),
                Elevated = elevated?.GetExt(),
                Bridge = bridge?.GetExt(),
                Slope = slope?.GetExt(),
                Tunnel = tunnel?.GetExt(),
            };

            return ret;
        }

        public static void Load(AssetData assetData, NetInfo groundInfo) {
            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(groundInfo);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(groundInfo);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(groundInfo);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(groundInfo);

            groundInfo?.SetExt(assetData.Ground);
            elevated?.SetExt(assetData.Elevated);
            bridge?.SetExt(assetData.Bridge);
            slope?.SetExt(assetData.Slope);
            tunnel?.SetExt(assetData.Tunnel);
        }
    }

    public class AssetDataExtension : AssetDataExtensionBase {
        public const string ID_NetInfo = "AdvancedRoadEditor_NetInfoExt";

        public static AssetDataExtension Instance;
        public override void OnCreated(IAssetData assetData) {
            base.OnCreated(assetData);
            Instance = this;
            // initiliazes buffer and extend prefab indeces if necessary (ie not hot reload)
            NetInfoExt.EnsureBuffer();
            NetInfoExt.DataDict = new Dictionary<NetInfo, NetInfoExt>();
        }
        public override void OnReleased() {
            Log.Debug("NetInfoExt.Buffer = null;\n"+ Environment.StackTrace);
            NetInfoExt.Buffer = null;
            Instance = null;
        }

        // TODO serialize BuildgInfo
        // TODO clone custom flags when netinfo is cloned by asset editor. 
        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) {
            try {
                Log.Info($"AssetDataExtension.OnAssetLoaded({name}, {asset}, userData) called");
                if (asset is NetInfo prefab) {
                    Log.Debug("AssetDataExtension.OnAssetLoaded():  prefab is " + prefab);
                    if (userData.TryGetValue(ID_NetInfo, out byte[] data)) {
                        Log.Info("AssetDataExtension.OnAssetLoaded(): extracted data for " + ID_NetInfo);
                        var assetData0 = SerializationUtil.Deserialize(data, default);
                        AssertNotNull(assetData0, "assetData0");
                        var assetData = assetData0 as AssetData;
                        AssertNotNull(assetData, $"assetData: {assetData0.GetType()} is not ${typeof(AssetData)}");
                        AssetData.Load(assetData, prefab);
                        Log.Debug("AssetDataExtension.OnAssetLoaded(): Asset Data=" + assetData);
                    }
                } else if (asset is BuildingInfo buildingInfo) {
                    // load stored custom road flags for intersections or buildings.
                }
            }catch(Exception e) {
                Log.Exception(e);
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
