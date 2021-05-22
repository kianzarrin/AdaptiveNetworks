using KianCommons;
using KianCommons.Serialization;
using PrefabMetadata.API;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AdaptiveRoads.Manager {
    [Serializable]
    public class AssetData : ISerializable{
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
                    Assertion.Assert(info, "info)");
                    info.EnsureExtended();
                    for (int i = 0; i < Nodes.Count; ++i)
                        (info.m_nodes[i] as IInfoExtended).SetMetaData(Nodes[i]);
                    for (int i = 0; i < Segments.Count; ++i)
                        (info.m_segments[i] as IInfoExtended).SetMetaData(Segments[i]);
                    ApplyProps(info);
                    info.SetMetedata(NetData?.Clone());
                    info.UpdateMetaData();
                    Log.Debug("Net Metadata restored.");
                }catch(Exception ex) {
                    Log.Exception(ex);
                }
            }

            public static void CopyMetadata(NetInfo source, NetInfo target) {
                NetInfoExtionsion.EnsureExtended(target);

                for (int i = 0; i < source.m_nodes.Length; ++i) {
                    var metadata = source.m_nodes[i].GetMetaData()?.Clone();
                    (target.m_nodes[i] as IInfoExtended).SetMetaData(metadata);
                }
                for (int i = 0; i < source.m_segments.Length; ++i) {
                    var metadata = source.m_segments[i].GetMetaData()?.Clone();
                    (target.m_segments[i] as IInfoExtended).SetMetaData(metadata);
                }

                for (int laneIndex = 0; laneIndex < source.m_lanes.Length; ++laneIndex) {
                    var m_propsTarget = target.m_lanes[laneIndex]?.m_laneProps?.m_props;
                    var m_propsTemplate = source.m_lanes[laneIndex]?.m_laneProps?.m_props;
                    if (m_propsTemplate == null) continue;
                    for (int i = 0; i < m_propsTemplate.Length; ++i) {
                        var metadata = m_propsTemplate[i].GetMetaData()?.Clone();
                        (m_propsTarget[i] as IInfoExtended).SetMetaData(metadata);
                    }
                }

                source.SetMetedata(target.GetMetaData()?.Clone());
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

        #region serialization
        public AssetData() { } 

        //serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context) =>
            SerializationUtil.GetObjectFields(info, this);

        // deserialization
        public AssetData(SerializationInfo info, StreamingContext context) {
            try {
                VersionString = info.GetString("VersionString");
            } catch {
                VersionString = default(Version).ToString(3);
            }
            try {
                var version = SerializationUtil.DeserializationVersion = new Version(VersionString);
                if(version < new Version("1,8")) {
                    Log.Warning($"old asset data (version:{version})");
                }
            } catch { }
            SerializationUtil.SetObjectFields(info, this);
        }
        #endregion

        public string VersionString = typeof(AssetData).VersionOf().ToString(3);

        public NetInfoMetaData Ground, Elevated, Bridge, Slope, Tunnel;

        public static AssetData CreateFromEditPrefab() =>
            Create(NetInfoExtionsion.EditedNetInfo);
        
        public static AssetData Create(NetInfo ground) {
            if (ground == null) return null;
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
