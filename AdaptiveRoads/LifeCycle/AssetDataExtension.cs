namespace AdaptiveRoads.LifeCycle {
    using AdaptiveRoads.Manager;
    using ColossalFramework.UI;
    using HarmonyLib;
    using ICities;
    using KianCommons;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using static KianCommons.Assertion;

    [HarmonyPatch(typeof(SaveAssetPanel), "SaveAsset")]
    public static class SaveRoutinePatch {
        public static void Prefix() {
            try {
                Log.Debug($"SaveAssetPanel.SaveRoutine(): reversing ...");
                SimulationManager.instance.ForcedSimulationPaused = true;
                AssetData.TakeSnapshot();
                foreach (var info in NetInfoExtionsion.EditedNetInfos)
                    info.ApplyVanillaForbidden();
                NetInfoExtionsion.UndoExtend_EditedNetInfos();

                // test if reversal was successful.
                //foreach (var netInfo in NetInfoExtionsion.EditedNetInfos) {
                //    for (int i = 0; i < netInfo.m_nodes.Length; ++i) {
                //        if (!(netInfo.m_nodes[i].GetType() == typeof(NetInfo.Node)))
                //            throw new Exception($"reversal unsuccessfull. nodes[{i}]={netInfo.m_nodes[i]}");
                //    }
                //    for (int i = 0; i < netInfo.m_segments.Length; ++i) {
                //        if (!(netInfo.m_segments[i].GetType() == typeof(NetInfo.Segment)))
                //            throw new Exception($"reversal unsuccessfull. segments[{i}]={netInfo.m_segments[i]}");
                //    }
                //    foreach (var lane in netInfo.m_lanes) {
                //        var props = lane.m_laneProps?.m_props;
                //        int n = props?.Length ?? 0;
                //        for (int i = 0; i < n; ++i) {
                //            if (!(props[i].GetType() == typeof(NetLaneProps.Prop)))
                //                throw new Exception($"reversal unsuccessfull. props[{i}]={props[i]}");
                //        }
                //    }
                //}
            }
            catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
            Log.Debug("SaveAssetPanel.SaveRoutine() was successfull");
        }

        public static void Postfix() {
            try {

                Log.Debug($"SaveAssetPanel.SaveRoutine re extending ...");
                foreach (var info in NetInfoExtionsion.EditedNetInfos) {
                    info.UndoVanillaForbidden();
                }
                AssetData.ApplySnapshot();
            }
            catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
        }

        public static void Finalizer(Exception __exception) {
            Log.Debug("SaveAssetPanel.Finalizer() called with " + __exception.STR() ); 
            SimulationManager.instance.ForcedSimulationPaused = false;
            if(__exception!=null)
                Log.Exception(__exception);
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

    [Serializable]
    public class AssetData {
        [Serializable]
        public class NetInfoMetaData {
            public List<NetInfoExtionsion.Node> Nodes = new List<NetInfoExtionsion.Node>();
            public List<NetInfoExtionsion.Segment> Segments = new List<NetInfoExtionsion.Segment>();
            public List<NetInfoExtionsion.LaneProp> Props = new List<NetInfoExtionsion.LaneProp>();
            public NetInfoExtionsion.Net NetData;

            public static NetInfoMetaData Create(NetInfo info) {
                if (info == null)
                    return null;
                return new NetInfoMetaData(info);
            }

            public NetInfoMetaData(NetInfo info) {
                foreach (var item in info.m_nodes)
                    Nodes.Add(item.GetMetaData());
                foreach (var item in info.m_segments)
                    Segments.Add(item.GetMetaData());
                foreach (var lane in info.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    if (props == null)
                        continue;
                    foreach (var item in props)
                        Props.Add(item.GetMetaData());
                }
                NetData = info.GetMetaData();
            }

            public void Apply(NetInfo info) {
                info.EnsureExtended();
                for (int i = 0; i < Nodes.Count; ++i)
                    (info.m_nodes[i] as IInfoExtended).SetMetaData(Nodes[i]);
                for (int i = 0; i < Segments.Count; ++i)
                    (info.m_segments[i] as IInfoExtended).SetMetaData(Segments[i]);

                foreach (var lane in info.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    if (props == null)
                        continue;
                    int i = 0;
                    foreach (var item in props)
                        (item as IInfoExtended).SetMetaData(Props[i++]);
                }
                info.SetMeteData(NetData?.Clone());
                Log.Debug("Net Metadata restored.");
            }
        }

        public NetInfoMetaData Ground, Elevated, Bridge, Slope, Tunnel;

        public static AssetData CreateFromEditPrefab() {
            NetInfo ground = NetInfoExtionsion.EditedNetInfo;
            if (ground == null)
                return null;
            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

            var ret = new AssetData {
                Ground = NetInfoMetaData.Create(ground),
                Elevated = NetInfoMetaData.Create(elevated),
                Bridge = NetInfoMetaData.Create(bridge),
                Slope = NetInfoMetaData.Create(slope),
                Tunnel = NetInfoMetaData.Create(tunnel),
            };

            return ret;
        }

        public static void Load(AssetData assetData, NetInfo groundInfo) {
            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(groundInfo);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(groundInfo);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(groundInfo);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(groundInfo);

            foreach (var info in NetInfoExtionsion.AllElevations(groundInfo))
                info.UndoVanillaForbidden();

            assetData.Ground?.Apply(groundInfo);
            assetData.Elevated?.Apply(elevated);
            assetData.Bridge?.Apply(bridge);
            assetData.Slope?.Apply(slope);
            assetData.Tunnel?.Apply(tunnel);
        }

        #region Snapshot
        public static AssetData Snapshot;

        public static void TakeSnapshot() =>
            Snapshot = CreateFromEditPrefab();

        public static void ApplySnapshot() =>
            Load(Snapshot, NetInfoExtionsion.EditedNetInfo);
        #endregion
    }

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
                Log.Info($"AssetDataExtension.OnAssetLoaded({name}, {asset}, userData) called");
                if (asset is NetInfo prefab) {
                    Log.Debug("AssetDataExtension.OnAssetLoaded():  prefab is " + prefab);
                    if (userData.TryGetValue(ID_NetInfo, out byte[] data)) {
                        Log.Info("AssetDataExtension.OnAssetLoaded(): extracted data for " + ID_NetInfo);
                        AssertNotNull(data, "data");
                        var assetData0 = SerializationUtil.Deserialize(data, default);
                        AssertNotNull(assetData0, "assetData0 | data version is too old");
                        var assetData = assetData0 as AssetData;
                        AssertNotNull(assetData, $"assetData: {assetData0.GetType()} is not ${typeof(AssetData)}");
                        AssetData.Load(assetData, prefab);
                        Log.Debug("AssetDataExtension.OnAssetLoaded(): Asset Data=" + assetData);
                    }
                } else if (asset is BuildingInfo buildingInfo) {
                    // TODO: load stored custom road flags for intersections or buildings.
                }
            }
            catch (Exception e) {
                Log.Exception(e);
            }
        }

        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetSaved({name}, {asset}, userData) called");
            userData = null;
            if (asset is NetInfo prefab) {
                Log.Info("AssetDataExtension.OnAssetSaved():  prefab is " + prefab);
                var assetData = AssetData.Snapshot; //AssetData.CreateFromEditPrefab();
                Log.Debug("AssetDataExtension.OnAssetSaved(): assetData=" + assetData);
                userData = new Dictionary<string, byte[]>();
                userData.Add(ID_NetInfo, SerializationUtil.Serialize(assetData));
            }
        }

    }


}
