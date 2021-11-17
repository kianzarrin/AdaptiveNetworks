namespace AdaptiveRoads.Patches.Track {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using HarmonyLib;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [HarmonyPatch(typeof(NetManager), "InitRenderDataImpl")]
    [InGamePatch]
    public static class NetManager_InitRenderDataImpl {
        static bool Prefix(ref IEnumerator __result) {
            __result = InitRenderDataImpl();
            return false;
        }

        public static IEnumerator InitRenderDataImpl() {
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.BeginLoading("NetManager.InitRenderData");
            FastList<KeyValuePair<NetInfo, NetInfo.Segment>> segmentsDic = new FastList<KeyValuePair<NetInfo, NetInfo.Segment>>();
            FastList<KeyValuePair<NetInfo, NetInfo.Node>> nodesDict = new FastList<KeyValuePair<NetInfo, NetInfo.Node>>();
            var tracksDict = new FastList<KeyValuePair<NetInfoExtionsion.Net, NetInfoExtionsion.Track>>();
            FastList<Texture2D> rgbTextures = new FastList<Texture2D>();
            FastList<Texture2D> xysTextures = new FastList<Texture2D>();
            FastList<Texture2D> aprTextures = new FastList<Texture2D>();
            int netCount = PrefabCollection<NetInfo>.LoadedCount();
            segmentsDic.EnsureCapacity(netCount * 4);
            nodesDict.EnsureCapacity(netCount * 4);
            tracksDict.EnsureCapacity(netCount * 4);
            rgbTextures.EnsureCapacity(netCount * 4);
            xysTextures.EnsureCapacity(netCount * 4);
            aprTextures.EnsureCapacity(netCount * 4);
            var netMan = NetManager.instance;
            for(int prefabIndex = 0; prefabIndex < netCount; prefabIndex++) {
                NetInfo netInfo = PrefabCollection<NetInfo>.GetLoaded((uint)prefabIndex);
                if(netInfo != null) {
                    if(netInfo.m_segments != null) {
                        for(int i = 0; i < netInfo.m_segments.Length; i++) {
                            try {
                                NetInfo.Segment segment = netInfo.m_segments[i];
                                if(segment.m_lodMesh == null || segment.m_lodMaterial == null) {
                                    netInfo.InitMeshData(segment, new Rect(0f, 0f, 1f, 1f), null, null, null);
                                } else {
                                    Texture2D rgb = null;
                                    if(segment.m_lodMaterial.HasProperty(netMan.ID_MainTex)) {
                                        rgb = (segment.m_lodMaterial.GetTexture(netMan.ID_MainTex) as Texture2D);
                                    }
                                    Texture2D xys = null;
                                    if(segment.m_lodMaterial.HasProperty(netMan.ID_XYSMap)) {
                                        xys = (segment.m_lodMaterial.GetTexture(netMan.ID_XYSMap) as Texture2D);
                                    }
                                    Texture2D apr = null;
                                    if(segment.m_lodMaterial.HasProperty(netMan.ID_APRMap)) {
                                        apr = (segment.m_lodMaterial.GetTexture(netMan.ID_APRMap) as Texture2D);
                                    }
                                    if(rgb == null && xys == null && apr == null) {
                                        netInfo.InitMeshData(segment, new Rect(0f, 0f, 1f, 1f), null, null, null);
                                    } else {
                                        if(rgb == null) {
                                            throw new PrefabException(netInfo, "LOD diffuse null");
                                        }
                                        if(xys == null) {
                                            throw new PrefabException(netInfo, "LOD xys null");
                                        }
                                        if(apr == null) {
                                            throw new PrefabException(netInfo, "LOD apr null");
                                        }
                                        if(xys.width != rgb.width || xys.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD xys size doesnt match diffuse size");
                                        }
                                        if(apr.width != rgb.width || apr.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD aci size doesnt match diffuse size");
                                        }
                                        try {
                                            rgb.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD diffuse not readable");
                                        }
                                        try {
                                            xys.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD xys not readable");
                                        }
                                        try {
                                            apr.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD aci not readable");
                                        }
                                        segmentsDic.Add(new KeyValuePair<NetInfo, NetInfo.Segment>(netInfo, segment));
                                        rgbTextures.Add(rgb);
                                        xysTextures.Add(xys);
                                        aprTextures.Add(apr);
                                    }
                                }
                            } catch(PrefabException ex) { ex.Handle(); }
                        }
                    }
                    if(netInfo.m_nodes != null) {
                        for(int i = 0; i < netInfo.m_nodes.Length; i++) {
                            try {
                                NetInfo.Node node = netInfo.m_nodes[i];
                                if(node.m_lodMesh == null || node.m_lodMaterial == null) {
                                    netInfo.InitMeshData(node, new Rect(0f, 0f, 1f, 1f), null, null, null);
                                } else {
                                    Texture2D rgb = null;
                                    if(node.m_lodMaterial.HasProperty(netMan.ID_MainTex)) {
                                        rgb = (node.m_lodMaterial.GetTexture(netMan.ID_MainTex) as Texture2D);
                                    }
                                    Texture2D xys = null;
                                    if(node.m_lodMaterial.HasProperty(netMan.ID_XYSMap)) {
                                        xys = (node.m_lodMaterial.GetTexture(netMan.ID_XYSMap) as Texture2D);
                                    }
                                    Texture2D apr = null;
                                    if(node.m_lodMaterial.HasProperty(netMan.ID_APRMap)) {
                                        apr = (node.m_lodMaterial.GetTexture(netMan.ID_APRMap) as Texture2D);
                                    }
                                    if(rgb == null && xys == null && apr == null) {
                                        netInfo.InitMeshData(node, new Rect(0f, 0f, 1f, 1f), null, null, null);
                                    } else {
                                        if(rgb == null) {
                                            throw new PrefabException(netInfo, "LOD diffuse null");
                                        }
                                        if(xys == null) {
                                            throw new PrefabException(netInfo, "LOD xys null");
                                        }
                                        if(apr == null) {
                                            throw new PrefabException(netInfo, "LOD apr null");
                                        }
                                        if(xys.width != rgb.width || xys.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD xys size doesnt match diffuse size");
                                        }
                                        if(apr.width != rgb.width || apr.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD aci size doesnt match diffuse size");
                                        }
                                        try {
                                            rgb.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD diffuse not readable");
                                        }
                                        try {
                                            xys.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD xys not readable");
                                        }
                                        try {
                                            apr.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD aci not readable");
                                        }
                                        nodesDict.Add(new KeyValuePair<NetInfo, NetInfo.Node>(netInfo, node));
                                        rgbTextures.Add(rgb);
                                        xysTextures.Add(xys);
                                        aprTextures.Add(apr);
                                    }
                                }
                            } catch(PrefabException ex) { ex.Handle(); }
                        }
                    }
                    var netInfoExt = netInfo?.GetMetaData();
                    if(netInfoExt?.Tracks != null) {
                        for(int i = 0; i < netInfoExt.Tracks.Length; i++) {
                            try {
                                var track = netInfoExt.Tracks[i];
                                if(track.m_lodMesh == null || track.m_lodMaterial == null) {
                                    netInfoExt.InitMeshData(track, new Rect(0f, 0f, 1f, 1f), null, null, null);
                                } else {
                                    Texture2D rgb = null;
                                    if(track.m_lodMaterial.HasProperty(netMan.ID_MainTex)) {
                                        rgb = (track.m_lodMaterial.GetTexture(netMan.ID_MainTex) as Texture2D);
                                    }
                                    Texture2D xys = null;
                                    if(track.m_lodMaterial.HasProperty(netMan.ID_XYSMap)) {
                                        xys = (track.m_lodMaterial.GetTexture(netMan.ID_XYSMap) as Texture2D);
                                    }
                                    Texture2D apr = null;
                                    if(track.m_lodMaterial.HasProperty(netMan.ID_APRMap)) {
                                        apr = (track.m_lodMaterial.GetTexture(netMan.ID_APRMap) as Texture2D);
                                    }
                                    if(rgb == null && xys == null && apr == null) {
                                        netInfoExt.InitMeshData(track, new Rect(0f, 0f, 1f, 1f), null, null, null);
                                    } else {
                                        if(rgb == null) {
                                            throw new PrefabException(netInfo, "LOD diffuse null");
                                        }
                                        if(xys == null) {
                                            throw new PrefabException(netInfo, "LOD xys null");
                                        }
                                        if(apr == null) {
                                            throw new PrefabException(netInfo, "LOD apr null");
                                        }
                                        if(xys.width != rgb.width || xys.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD xys size doesnt match diffuse size");
                                        }
                                        if(apr.width != rgb.width || apr.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD aci size doesnt match diffuse size");
                                        }
                                        try {
                                            rgb.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD diffuse not readable");
                                        }
                                        try {
                                            xys.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD xys not readable");
                                        }
                                        try {
                                            apr.GetPixel(0, 0);
                                        } catch(UnityException) {
                                            throw new PrefabException(netInfo, "LOD aci not readable");
                                        }
                                        tracksDict.Add(new KeyValuePair<NetInfoExtionsion.Net, NetInfoExtionsion.Track>(netInfoExt, track));
                                        rgbTextures.Add(rgb);
                                        xysTextures.Add(xys);
                                        aprTextures.Add(apr);
                                    }
                                }
                            } catch(PrefabException ex) { ex.Handle(); }
                        }
                    }
                }
            }
            if(netMan.m_lodRgbAtlas == null) {
                netMan.m_lodRgbAtlas = new Texture2D(1024, 1024, TextureFormat.DXT1, true, false);
                netMan.m_lodRgbAtlas.filterMode = FilterMode.Trilinear;
                netMan.m_lodRgbAtlas.anisoLevel = 8;
            }
            if(netMan.m_lodXysAtlas == null) {
                netMan.m_lodXysAtlas = new Texture2D(1024, 1024, TextureFormat.DXT1, true, true);
                netMan.m_lodXysAtlas.filterMode = FilterMode.Trilinear;
                netMan.m_lodXysAtlas.anisoLevel = 8;
            }
            if(netMan.m_lodAprAtlas == null) {
                netMan.m_lodAprAtlas = new Texture2D(1024, 1024, TextureFormat.DXT1, true, true);
                netMan.m_lodAprAtlas.filterMode = FilterMode.Trilinear;
                netMan.m_lodAprAtlas.anisoLevel = 8;
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.PauseLoading();
            yield return 0;
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.ContinueLoading();
            Rect[] rect = netMan.m_lodRgbAtlas.PackTextures(rgbTextures.ToArray(), 0, 4096, false);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.PauseLoading();
            yield return 0;
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.ContinueLoading();
            netMan.m_lodXysAtlas.PackTextures(xysTextures.ToArray(), 0, 4096, false);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.PauseLoading();
            yield return 0;
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.ContinueLoading();
            netMan.m_lodAprAtlas.PackTextures(aprTextures.ToArray(), 0, 4096, false);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.PauseLoading();
            yield return 0;
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.ContinueLoading();
            for(int i = 0; i < segmentsDic.m_size; i++) {
                try {
                    var pair = segmentsDic.m_buffer[i];
                    var netInfo = pair.Key;
                    var segmentInfo = pair.Value;
                    netInfo.InitMeshData(segmentInfo, rect[i], netMan.m_lodRgbAtlas, netMan.m_lodXysAtlas, netMan.m_lodAprAtlas);
                } catch(PrefabException ex) { ex.Handle(); }

            }
            for(int i = 0; i < nodesDict.m_size; i++) {
                try {
                    var pair = nodesDict.m_buffer[i];
                    var netInfo = pair.Key;
                    var nodeInfo = pair.Value;
                    netInfo.InitMeshData(nodeInfo, rect[i], netMan.m_lodRgbAtlas, netMan.m_lodXysAtlas, netMan.m_lodAprAtlas);
                } catch(PrefabException ex) { ex.Handle(); }

            }
            for(int i = 0; i < tracksDict.m_size; i++) {
                try {
                    var pair = tracksDict.m_buffer[i];
                    var netInfoExt = pair.Key;
                    var trackInfo = pair.Value;
                    netInfoExt.InitMeshData(trackInfo, rect[i], netMan.m_lodRgbAtlas, netMan.m_lodXysAtlas, netMan.m_lodAprAtlas);
                } catch(PrefabException ex) { ex.Handle(); }
            }

            Singleton<LoadingManager>.instance.m_loadingProfilerMain.EndLoading();
            yield return 0;
            yield break;

        }

        public static void Handle(this PrefabException ex) {
            CODebugBase<LogChannel>.Error(
                LogChannel.Core,
                $"{ex.m_prefabInfo.gameObject.name}: {ex.Message}\nex.StackTrace",
                ex.m_prefabInfo.gameObject);
            LoadingManager.instance.m_brokenAssets += $"\n{ex.m_prefabInfo.gameObject.name}: {ex.Message}";
        }

    }
}
