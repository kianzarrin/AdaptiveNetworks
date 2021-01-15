using ColossalFramework.IO;
using System.IO;
using AdaptiveRoads.Util;
using ColossalFramework.IO;
using KianCommons;
using KianCommons.Math;
using KianCommons.Serialization;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AdaptiveRoads.DTO {
    //code from RoadImporter: https://github.com/citiesskylines-csur/RoadImporter
    public class RoadAssetInfo : IDTO<NetInfo>, ISerialziableDTO {
        public NetInfoDTO basic;
        public NetInfoDTO elevated;
        public NetInfoDTO bridge;
        public NetInfoDTO slope;
        public NetInfoDTO tunnel;

        public RoadAIDTO basicAI;
        public BridgeAIDTO elevatedAI;
        public BridgeAIDTO bridgeAI;
        public TunnelAIDTO slopeAI;
        public TunnelAIDTO tunnelAI;

        //public NetModelInfo basicModel = new NetModelInfo();
        //public NetModelInfo elevatedModel = new NetModelInfo();
        //public NetModelInfo bridgeModel = new NetModelInfo();
        //public NetModelInfo slopeModel = new NetModelInfo();
        //public NetModelInfo tunnelModel = new NetModelInfo();

        public string Name { get; set; }
        public string Description { get; set; }

        public void ReadFromGame(NetInfo game_basic) {
            NetInfo game_elevated = AssetEditorRoadUtils.TryGetElevated(game_basic);
            NetInfo game_bridge = AssetEditorRoadUtils.TryGetBridge(game_basic);
            NetInfo game_slope = AssetEditorRoadUtils.TryGetSlope(game_basic);
            NetInfo game_tunnel = AssetEditorRoadUtils.TryGetTunnel(game_basic);

            basic = new NetInfoDTO();
            basicAI = new RoadAIDTO();
            basic.ReadFromGame(game_basic);
            DTOUtil.CopyAllMatchingFields<RoadAIDTO>(basicAI, game_basic.m_netAI);

            if (game_elevated) {
                elevated = new NetInfoDTO();
                elevated.ReadFromGame(game_elevated);
                elevatedAI = new BridgeAIDTO();
                DTOUtil.CopyAllMatchingFields<BridgeAIDTO>(elevatedAI, game_elevated.m_netAI);
            }
            if (game_bridge) {
                bridge = new NetInfoDTO();
                bridge.ReadFromGame(game_bridge);
                bridgeAI = new BridgeAIDTO();
                DTOUtil.CopyAllMatchingFields<BridgeAIDTO>(bridgeAI, game_bridge.m_netAI);
            }
            if (game_slope) {
                slope = new NetInfoDTO();
                slope.ReadFromGame(game_slope);
                slopeAI = new TunnelAIDTO();
                DTOUtil.CopyAllMatchingFields<TunnelAIDTO>(slopeAI, game_slope.m_netAI);
            }
            if (game_tunnel) {
                tunnel = new NetInfoDTO();
                tunnel.ReadFromGame(game_tunnel);
                tunnelAI = new TunnelAIDTO();
                DTOUtil.CopyAllMatchingFields<TunnelAIDTO>(tunnelAI, game_tunnel.m_netAI);
            }

            //basicModel.Read(game_basic, "Basic");
            //elevatedModel.Read(gameRoadAI.m_elevatedInfo, "Elevated");
            //bridgeModel.Read(gameRoadAI.m_bridgeInfo, "Bridge");
            //slopeModel.Read(gameRoadAI.m_slopeInfo, "Slope");
            //tunnelModel.Read(gameRoadAI.m_tunnelInfo, "Tunnel");
        }

        public void WriteToGame(NetInfo game_basic) {
            NetInfo game_elevated = AssetEditorRoadUtils.TryGetElevated(game_basic);
            NetInfo game_bridge = AssetEditorRoadUtils.TryGetBridge(game_basic);
            NetInfo game_slope = AssetEditorRoadUtils.TryGetSlope(game_basic);
            NetInfo game_tunnel = AssetEditorRoadUtils.TryGetTunnel(game_basic);

            basic.WriteToGame(game_basic);
            elevated?.WriteToGame(game_elevated);
            bridge?.WriteToGame(game_bridge);
            slope?.WriteToGame(game_slope);
            tunnel?.WriteToGame(game_tunnel);

            DTOUtil.CopyAllMatchingFields<RoadAIDTO>(game_basic.m_netAI, basicAI);
            DTOUtil.CopyAllMatchingFields<BridgeAIDTO>(game_elevated.m_netAI, elevatedAI);
            DTOUtil.CopyAllMatchingFields<BridgeAIDTO>(game_bridge.m_netAI, bridgeAI);
            DTOUtil.CopyAllMatchingFields<TunnelAIDTO>(game_slope.m_netAI, slopeAI);
            DTOUtil.CopyAllMatchingFields<TunnelAIDTO>(game_tunnel.m_netAI, tunnelAI);
        }

        private static MultiSerializer<RoadAssetInfo> Serializer = new MultiSerializer<RoadAssetInfo>("RoadAssetInfo");
        public void Save() => Serializer.Save(Name, this);
        public void OnLoaded() { }
        public static IEnumerable<RoadAssetInfo> LoadAllFiles() => Serializer.LoadAllFiles();
    }
}

