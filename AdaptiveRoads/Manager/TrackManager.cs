namespace AdaptiveRoads.Manager {
    using ColossalFramework;
    using System;
    using UnityEngine;
    using KianCommons;
    using System.Collections;
    using KianCommons.IImplict;
    using HarmonyLib;

    public class TrackManager : Singleton<TrackManager>, IRenderableManager, IAwakingObject, IDestroyableObject {
        private static NetworkExtensionManager NetworkExtensionManager => NetworkExtensionManager.RawInstance; // help with modtools

        public string GetName() => gameObject?.name ?? "null";
        private int m_roadLayer;
        public DrawCallData GetDrawCallData() => NetManager.instance.GetDrawCallData();

        #region Life-cycle
        public void Awake() {
            try {
                this.m_roadLayer = LayerMask.NameToLayer("Road");
                var oldIndinces = RenderManager.instance.m_indices;
                if(oldIndinces.Length < MAX_HOLDER_COUNT) {
                    RenderManager.instance.m_indices = new ushort[MAX_HOLDER_COUNT];
                    Array.Copy(oldIndinces, RenderManager.instance.m_indices, oldIndinces.Length);
                }
                RenderManager.RegisterRenderableManager(this);
            }catch(Exception ex) {
                ex.Log();
            }
        }

        public void OnDestroy() {
            var renderables = ReflectionHelpers.GetFieldValue<RenderManager>("m_renderables") as FastList<IRenderableManager>;
            renderables.Remove(this);
        }

        #endregion


        #region main rendering
        public void BeginRendering(RenderManager.CameraInfo cameraInfo) {
            var netMan = NetManager.instance;
            Material material = RenderManager.instance.m_groupLayerMaterials[m_roadLayer];
            if(material != null) {
                if(material.HasProperty(netMan.ID_MainTex)) {
                    material.SetTexture(netMan.ID_MainTex, netMan.m_lodRgbAtlas);
                }
                if(material.HasProperty(netMan.ID_XYSMap)) {
                    material.SetTexture(netMan.ID_XYSMap, netMan.m_lodXysAtlas);
                }
                if(material.HasProperty(netMan.ID_APRMap)) {
                    material.SetTexture(netMan.ID_APRMap, netMan.m_lodAprAtlas);
                }
            }
        }

        public const uint SEGMENT_HOLDER = BuildingManager.MAX_BUILDING_COUNT; //49,152
        public const uint NODE_HOLDER = SEGMENT_HOLDER + NetManager.MAX_SEGMENT_COUNT; // 86,016
        public const uint TRACK_HOLDER_SEGMNET = NODE_HOLDER + NetManager.MAX_NODE_COUNT; // 118,784
        public const uint TRACK_HOLDER_NODE = TRACK_HOLDER_SEGMNET + NetManager.MAX_SEGMENT_COUNT; //155,648
        public const uint MAX_HOLDER_COUNT = TRACK_HOLDER_NODE + NetManager.MAX_NODE_COUNT; // 188,416
        public const int INVALID_RENDER_INDEX = ushort.MaxValue;

        public void EndRendering(RenderManager.CameraInfo cameraInfo) {
            try {
                if(!NetworkExtensionManager.Exists)
                    return;

                FastList<RenderGroup> renderedGroups = Singleton<RenderManager>.instance.m_renderedGroups;
                for(int groupIndex = 0; groupIndex < renderedGroups.m_size; groupIndex++) {
                    RenderGroup renderGroup = renderedGroups.m_buffer[groupIndex];
                    int layerMask = renderGroup.m_instanceMask;
                    if(layerMask != 0) {
                        const int resolutionRatio = NetManager.NODEGRID_RESOLUTION / RenderManager.GROUP_RESOLUTION; // = 270/45 = 6
                        int net_x0 = renderGroup.m_x * resolutionRatio;
                        int net_z0 = renderGroup.m_z * resolutionRatio;
                        int net_x1 = (renderGroup.m_x + 1) * resolutionRatio - 1; // = net_x + 5
                        int net_z1 = (renderGroup.m_z + 1) * resolutionRatio - 1; // = net_z + 5
                        for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                            for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                                int gridIndex = net_z * 270 + net_x;
                                ushort segmentID = NetManager.instance.m_segmentGrid[gridIndex];
                                int watchdog = 0;
                                while(segmentID != 0) {
                                    segmentID.ToSegmentExt().RenderTrackInstance(cameraInfo, layerMask);
                                    segmentID = segmentID.ToSegment().m_nextGridSegment;
                                    if(++watchdog >= 36864) {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }
                            }
                        }
                        for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                            for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                                int gridIndex = net_z * NetManager.NODEGRID_RESOLUTION + net_x;
                                ushort nodeID = NetManager.instance.m_nodeGrid[gridIndex];
                                int watchdog = 0;
                                while(nodeID != 0) {
                                    nodeID.ToNodeExt().RenderTrackInstance(cameraInfo, layerMask);
                                    nodeID = nodeID.ToNode().m_nextGridNode;
                                    if(++watchdog >= 32768) {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                int nPrefabs = PrefabCollection<NetInfo>.PrefabCount();
                for(int n = 0; n < nPrefabs; n++) {
                    NetInfo prefab = PrefabCollection<NetInfo>.GetPrefab((uint)n);
                    var tracks = prefab?.GetMetaData()?.Tracks;
                    if(tracks != null) {
                        for(int i = 0; i < tracks.Length; i++) {
                            NetInfo.LodValue combinedLod = tracks[i].m_combinedLod;
                            if(combinedLod != null && combinedLod.m_lodCount != 0)
                                NetSegment.RenderLod(cameraInfo,combinedLod);
                        }
                    }
                }
            } catch(Exception ex) {
                ex.Log(false);
            }
        }

        public bool CalculateGroupData(int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            try {
                if(!NetworkExtensionManager.Exists)
                    return false;

                bool ret = false;
                const int resolutionRatio = NetManager.NODEGRID_RESOLUTION / RenderManager.GROUP_RESOLUTION; // = 270/45 = 6
                int net_x0 = groupX * resolutionRatio;
                int net_z0 = groupZ * resolutionRatio;
                int net_x1 = (groupX + 1) * resolutionRatio - 1;
                int net_z1 = (groupZ + 1) * resolutionRatio - 1;
                for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                    for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                        ushort segmentID = NetManager.instance.m_segmentGrid[net_z * NetManager.NODEGRID_RESOLUTION + net_x];
                        int watchdog = 0;
                        while(segmentID != 0) {
                            ret |= segmentID.ToSegmentExt().CalculateGroupData(layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                            segmentID = segmentID.ToSegment().m_nextGridSegment;
                            if(++watchdog >= 36864) {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                    for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                        int gridIndex = net_z * NetManager.NODEGRID_RESOLUTION + net_x;
                        ushort nodeID = NetManager.instance.m_nodeGrid[gridIndex];
                        int watchdog = 0;
                        while(nodeID != 0) {
                            ret |= nodeID.ToNodeExt().CalculateGroupData(layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                            nodeID = nodeID.ToNode().m_nextGridNode;
                            if(++watchdog >= 32768) {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                return ret;
            } catch(Exception ex) {
                ex.Log();
                return false;
            }
            
        }

        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps) {
            try {
                if(!NetworkExtensionManager.Exists)
                    return;

                const int resolutionRatio = NetManager.NODEGRID_RESOLUTION / RenderManager.GROUP_RESOLUTION; // = 270/45 = 6
                int net_x0 = groupX * resolutionRatio;
                int net_z0 = groupZ * resolutionRatio;
                int net_x1 = (groupX + 1) * resolutionRatio - 1;
                int net_z1 = (groupZ + 1) * resolutionRatio - 1;
                for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                    for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                        ushort segmentID = NetManager.instance.m_segmentGrid[net_z * NetManager.NODEGRID_RESOLUTION + net_x];
                        int watchdog = 0;
                        while(segmentID != 0) {
                            segmentID.ToSegmentExt().PopulateGroupData(groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                            segmentID = segmentID.ToSegment().m_nextGridSegment;
                            if(++watchdog >= 36864) {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                    for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                        int gridIndex = net_z * NetManager.NODEGRID_RESOLUTION + net_x;
                        ushort nodeID = NetManager.instance.m_nodeGrid[gridIndex];
                        int watchdog = 0;
                        while(nodeID != 0) {
                            nodeID.ToNodeExt().PopulateGroupData(groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                            nodeID = nodeID.ToNode().m_nextGridNode;
                            if(++watchdog >= 32768) {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
            } catch(Exception ex) { ex.Log(); }
        }

        #endregion

        #region overlay
        public void BeginOverlay(RenderManager.CameraInfo cameraInfo) { }

        public void EndOverlay(RenderManager.CameraInfo cameraInfo) { }

        public void UndergroundOverlay(RenderManager.CameraInfo cameraInfo) { }
        #endregion

        public void CheckReferences() { }

        public void InitRenderData() { }
    }
}
