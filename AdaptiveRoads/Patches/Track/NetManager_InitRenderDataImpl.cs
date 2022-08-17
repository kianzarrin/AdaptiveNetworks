namespace AdaptiveRoads.Patches.Track {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using HarmonyLib;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using KianCommons;
    using System;
    using AdaptiveRoads.Util;

    [HarmonyPatch(typeof(NetManager), "InitRenderDataImpl")]
    [PreloadPatch]
    public static class NetManager_InitRenderDataImpl {
        static bool Prefix(ref IEnumerator __result) {
            Log.Called();
            __result = InitRenderDataImpl();
            return false;
        }

        static bool LSM => LSMRevisited.LastActive || LSMUtil.LastSharing != null;

        public static IEnumerator InitRenderDataImpl() {
            yield return 0;
            Log.Debug("NetManager_InitRenderDataImpl.InitRenderDataImpl() started ...");
            Log.Debug("LSM=" + LSM);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.BeginLoading("NetManager.InitRenderData");
            int netCount = PrefabCollection<NetInfo>.LoadedCount();
            var subInfos = new List<KeyValuePair<NetInfo, object>>(netCount * 10);
            FastList<Texture2D> rgbTextures = new FastList<Texture2D>();
            FastList<Texture2D> xysTextures = new FastList<Texture2D>();
            FastList<Texture2D> aprTextures = new FastList<Texture2D>();
            FastList<Vector2> sizes = new FastList<Vector2>();
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
                                            throw new PrefabException(netInfo, "LOD xys size doesn't match diffuse size");
                                        }
                                        if(apr.width != rgb.width || apr.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD apr size doesn't match diffuse size");
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
                                            throw new PrefabException(netInfo, "LOD apr not readable");
                                        }
                                        subInfos.Add(new KeyValuePair<NetInfo, object>(netInfo, segment));
                                        rgbTextures.Add(rgb);
                                        xysTextures.Add(xys);
                                        aprTextures.Add(apr);
                                        sizes.Add(new Vector2(rgb.width, rgb.height));
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
                                            throw new PrefabException(netInfo, "LOD xys size doesn't match diffuse size");
                                        }
                                        if(apr.width != rgb.width || apr.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD apr size doesn't match diffuse size");
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
                                            throw new PrefabException(netInfo, "LOD apr not readable");
                                        }
                                        subInfos.Add(new KeyValuePair<NetInfo, object>(netInfo, node));
                                        rgbTextures.Add(rgb);
                                        xysTextures.Add(xys);
                                        aprTextures.Add(apr);
                                        sizes.Add(new Vector2(rgb.width, rgb.height));
                                    }
                                }
                            } catch(PrefabException ex) { ex.Handle(); }
                        }
                    }
                    var netInfoExt = netInfo?.GetMetaData();
                    if(netInfoExt?.Tracks != null) {
                        for (int i = 0; i < netInfoExt.Tracks.Length; i++) {
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
                                            throw new PrefabException(netInfo, "LOD xys size doesn't match diffuse size");
                                        }
                                        if(apr.width != rgb.width || apr.height != rgb.height) {
                                            throw new PrefabException(netInfo, "LOD apr size doesn't match diffuse size");
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
                                            throw new PrefabException(netInfo, "LOD apr not readable");
                                        }

                                        subInfos.Add(new KeyValuePair<NetInfo, object>(netInfo, track));
                                        if (LSM) {
                                            // work around shared lod textures.
                                            rgb = Texture2D.Instantiate(rgb);
                                            xys = Texture2D.Instantiate(xys);
                                            apr = Texture2D.Instantiate(apr);
                                        }
                                        rgbTextures.Add(rgb);
                                        xysTextures.Add(xys);
                                        aprTextures.Add(apr);
                                        sizes.Add(new Vector2(rgb.width, rgb.height));
                                    }
                                }
                            } catch(PrefabException ex) {
                                ex.Handle();
                            } catch (Exception ex){
                                ex.Log();
                            }
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
            Rect[] rects = netMan.m_lodRgbAtlas.PackTextures(rgbTextures.ToArray(), 0, 4096, false);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.PauseLoading();
            yield return 0;
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.ContinueLoading();
            Rect[] xysRects = netMan.m_lodXysAtlas.PackTextures(xysTextures.ToArray(), 0, 4096, false);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.PauseLoading();
            yield return 0;
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.ContinueLoading();
            Rect[] aprRects = netMan.m_lodAprAtlas.PackTextures(aprTextures.ToArray(), 0, 4096, false);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.PauseLoading();
            yield return 0;
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.ContinueLoading();

            for (int i = 0; i < subInfos.Count; ++i) {
                var netInfo = subInfos[i].Key;
                try {
                    bool failed = false;
                    if (xysRects[i] != rects[i] ) {
                        failed = true;
                        Handle(new PrefabException(netInfo, $"xys rect mismatch: {xysRects[i]} != {rects[i]}"));
                    }
                    if (aprRects[i] != rects[i]) {
                        failed = true;
                        Handle(new PrefabException(netInfo, $"apr rect mismatch: {xysRects[i]} != {rects[i]}"));
                    }
                    if (failed) continue;
                } catch (PrefabException ex) {
                    ex.Handle();
                } catch (Exception ex) {
                    ex.Log();
                }

                try {
                    if (subInfos[i].Value is NetInfo.Segment segmentInfo) {
                        netInfo.InitMeshData(segmentInfo, rects[i], netMan.m_lodRgbAtlas, netMan.m_lodXysAtlas, netMan.m_lodAprAtlas);

                    } else if (subInfos[i].Value is NetInfo.Node nodeInfo) {
                        netInfo.InitMeshData(nodeInfo, rects[i], netMan.m_lodRgbAtlas, netMan.m_lodXysAtlas, netMan.m_lodAprAtlas);

                    } else if (subInfos[i].Value is NetInfoExtionsion.Track trackInfo) {
                        netInfo.GetMetaData()?.InitMeshData(trackInfo, rects[i], netMan.m_lodRgbAtlas, netMan.m_lodXysAtlas, netMan.m_lodAprAtlas);
                        if (LSM) {
                            // destroy cloned textures.
                            Texture2D.Destroy(rgbTextures[i]);
                            rgbTextures[i] = null;
                            Texture2D.Destroy(xysTextures[i]);
                            xysTextures[i] = null;
                            Texture2D.Destroy(aprTextures[i]);
                            aprTextures[i] = null;
                        }
                    }
                } catch(Exception ex) { ex.Log(); }
            }

            Singleton<LoadingManager>.instance.m_loadingProfilerMain.EndLoading();
            yield return 0;
            yield break;

        }

        public static void Handle(this PrefabException ex) {
            ex.Log($"{ex.m_prefabInfo.gameObject.name} : failed to rebuild LOD ", false);
            CODebugBase<LogChannel>.Error(
                LogChannel.Core,
                $"{ex.m_prefabInfo.gameObject.name}: {ex.Message}\nex.StackTrace",
                ex.m_prefabInfo.gameObject);
            LoadingManager.instance.m_brokenAssets += $"\n{ex.m_prefabInfo.gameObject.name}: {ex.Message}";
        }

    }
}
