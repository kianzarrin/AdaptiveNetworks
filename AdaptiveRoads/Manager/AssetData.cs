using KianCommons;
using PrefabMetadata.API;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;

namespace AdaptiveRoads.Manager {
    [Serializable]
    public class AssetData {
        [Serializable]
        public class NetInfoMetaData {
            public List<NetInfoExtionsion.Node> Nodes = new List<NetInfoExtionsion.Node>();
            public List<NetInfoExtionsion.Segment> Segments = new List<NetInfoExtionsion.Segment>();

            /// <summary>
            /// props for all lanes is stored here in order.
            /// </summary>
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
                try {
                    info.EnsureExtended();
                    for (int i = 0; i < Nodes.Count; ++i)
                        (info.m_nodes[i] as IInfoExtended).SetMetaData(Nodes[i]);
                    for (int i = 0; i < Segments.Count; ++i)
                        (info.m_segments[i] as IInfoExtended).SetMetaData(Segments[i]);
                    ApplyProps(info);
                    info.SetMeteData(NetData?.Clone());
                    Log.Debug("Net Metadata restored.");
                }catch(Exception ex) {
                    Log.Exception(ex);
                }
            }
            void ApplyProps(NetInfo info) {
                // this.Props stores props for all lanes in order.
                // so we need to extract them in order.
                int i = 0;
                foreach (var lane in info.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    if (props == null)
                        continue;
                    foreach (var item in props)
                        (item as IInfoExtended).SetMetaData(Props[i++]);
                }
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
}
