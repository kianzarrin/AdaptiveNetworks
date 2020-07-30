namespace AdvancedRoads.LifeCycle {
    using System.Collections.Generic;
    using ColossalFramework.UI;
    using ICities;
    using HarmonyLib;
    using KianCommons;
    using static KianCommons.HelpersExtensions;
    using AdvancedRoads.Manager;
    using PrefabIndeces;

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

    public class AssetDataExtension : AssetDataExtensionBase {
        public const string ID = "AdvancedRoadEditor_Records";

        public static AssetDataExtension Instance;
        public override void OnCreated(IAssetData assetData) {
            base.OnCreated(assetData);
            Instance = this;
        }
        public override void OnReleased() {
            Instance = null;
        }

        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetLoaded({name}, {asset}, userData) called");
            if (asset is NetInfo prefab) {
                Log.Debug("AssetDataExtension.OnAssetLoaded():  prefab is " + prefab);
                if (userData.TryGetValue(ID, out byte[] data)) {
                    Log.Info("AssetDataExtension.OnAssetLoaded(): extracted data for " + ID);
                    var assetData = SerializationUtil.Deserialize(data) as NetInfoExt;
                    AssertNotNull(assetData, "assetData");
                    NetInfoExt.Buffer[prefab.GetIndex()] = assetData;
                    Log.Debug("AssetDataExtension.OnAssetLoaded(): Asset Data=" + assetData);
                }
            }
        }

        // asset should be the same as ToolsModifierControl.toolController.m_editPrefabInfo
        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetSaved({name}, {asset}, userData) called");
            userData = null;
            if (asset is NetInfo prefab) {
                Log.Info("AssetDataExtension.OnAssetSaved():  prefab is " + prefab);
                var assetData = NetInfoExt.Edited;
                Log.Debug("AssetDataExtension.OnAssetSaved(): assetData=" + assetData);
                userData = new Dictionary<string, byte[]>();
                userData.Add(ID, SerializationUtil.Serialize(assetData));
            }
        }
    }


}
