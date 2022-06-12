namespace AdaptiveRoads.Patches.AssetPatches {
    using ColossalFramework.UI;
    using HarmonyLib;
    using LifeCycle;
    using KianCommons;
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using System.Reflection;
    using ColossalFramework.Packaging;
    using System;
    using static KianCommons.ReflectionHelpers;

    [HarmonyPatch(typeof(LoadAssetPanel), "OnLoad")]
    public static class OnLoadPatch {
#if DEBUG
        public static void Prefix() =>
            Log.Debug("LoadAssetPanel.OnLoad().Prefix(): Loading Road asset " +
                     $"(ARMode={UI.ModSettings.ARMode}) ...");

#endif
        static MethodInfo mListingMetaData =
            typeof(LoadSavePanelBase<CustomAssetMetaData>)
            .GetMethod("GetListingMetaData", BindingFlags.Instance | BindingFlags.NonPublic);


        /// <summary>
        /// when loading asset from a file, IAssetData.OnAssetLoaded() is called for all assets but the one that is loaded from the file.
        /// this postfix calls IAssetData.OnAssetLoaded() for asset loaded from file.
        /// Note: even if new road instantiated based on an AN Road, we still need to do this because
        ///       NetInfo metadata is stored in external array
        ///       also we cannot be sure that it is always instantiated properly.
        /// </summary>
        public static void Postfix(LoadAssetPanel __instance, UIListBox ___m_SaveList) {
            try {
                // Taken from LoadAssetPanel.OnLoad
                var selectedIndex = ___m_SaveList.selectedIndex;
                var listingMetaData = (CustomAssetMetaData)mListingMetaData
                    .Invoke(__instance, new object[] { selectedIndex });
                AssetDataExtension.ListingMetaData = listingMetaData;

                // Taken from LoadingManager.LoadCustomContent
                if (listingMetaData.userDataRef != null) {
                    AssetDataWrapper.UserAssetData userAssetData =
                        listingMetaData.userDataRef.Instantiate() as AssetDataWrapper.UserAssetData;
                    if (userAssetData == null) {
                        userAssetData = new AssetDataWrapper.UserAssetData();
                    }
                    Log.Info($"LoadAssetPanel.OnLoad().Postfix(): Loading asset from load asset panel");
                    AssetDataExtension.OnAssetLoadedImpl(
                        listingMetaData.name,
                        ToolsModifierControl.toolController.m_editPrefabInfo,
                        userAssetData.Data);

                    var originalInfo = GetOriginalNetInfo(listingMetaData);
                    if (originalInfo) {
                        // OnLoad() calls IntializePrefab() which reverses metadata
                        // and can't be patched because its generic.
                        // so we restore asset metadata here
                        Log.Info($"restoring original metadata.");
                        AssetDataExtension.OnAssetLoadedImpl(
                            listingMetaData.name,
                            originalInfo,
                            userAssetData.Data);
                    }
                }
                NetInfoExtionsion.Ensure_EditedNetInfos(recalculate:true);
                Log.Debug($"LoadAssetPanel.OnLoad().Postfix() succeeded!");
            } catch (Exception ex) {
                Log.Exception(ex);
            } finally {
                AssetDataExtension.ListingMetaData = null;
            }

        }

        static NetInfo GetOriginalNetInfo(CustomAssetMetaData listingMetaData) {
            LogCalled();
            var roadName = listingMetaData.name;
            if (!roadName.EndsWith("_Data"))
                roadName = roadName + "_Data";
            var packageName =  listingMetaData.assetRef.package.packageName;
            var originalName = packageName + "."+ PackageHelper.StripName(roadName) ;
            Log.Debug("searching for raod:" + originalName);
            var ret =  PrefabCollection<NetInfo>.FindLoaded(originalName);
            Log.Debug("GetOriginalNetInfo() returns " + ret.ToSTR());
            return ret;

        }
    }
}
