namespace AdaptiveRoads.Manager {
    using ColossalFramework;
    using System;
    using UnityEngine;
    using KianCommons;
    using System.Collections;
    using KianCommons.IImplict;

    public class TrackManager : Singleton<TrackManager>, IRenderableManager, IAwakingObject, IDestroyableObject {
        [NonSerialized]
        public DrawCallData m_drawCallData;

        public string GetName() => gameObject?.name ?? "null";

        public DrawCallData GetDrawCallData() => m_drawCallData;

        #region Life-cycle
        public void Awake() {
            RenderManager.RegisterRenderableManager(this);
        }

        public void OnDestroy() {
            var renderables = ReflectionHelpers.GetFieldValue<RenderManager>("m_renderables") as FastList<IRenderableManager>;
            renderables.Remove(this);
        }

        #endregion


        #region main rendering

        public void BeginRendering(RenderManager.CameraInfo cameraInfo) {
            try {
                this.m_drawCallData.m_defaultCalls = 0;
                this.m_drawCallData.m_lodCalls = 0;
                this.m_drawCallData.m_batchedCalls = 0;
            } catch(Exception ex) {
                ex.Log(false);
            }
        }

        public const uint SEGMENT_HOLDER = BuildingManager.MAX_BUILDING_COUNT;
        public const uint NODE_HOLDER = SEGMENT_HOLDER + NetManager.MAX_SEGMENT_COUNT;
        public const uint TRACK_HOLDER_SEGMNET = NODE_HOLDER + NetManager.MAX_NODE_COUNT;
        public const uint TRACK_HOLDER_NODE = TRACK_HOLDER_SEGMNET + NetManager.MAX_SEGMENT_COUNT;

        public void EndRendering(RenderManager.CameraInfo cameraInfo) {
            try {
                FastList<RenderGroup> renderedGroups = Singleton<RenderManager>.instance.m_renderedGroups;
                for(int groupIndex = 0; groupIndex < renderedGroups.m_size; groupIndex++) {
                    RenderGroup renderGroup = renderedGroups.m_buffer[groupIndex];
                    if(renderGroup.m_instanceMask != 0) {
                        const int resolutionRatio = NetManager.NODEGRID_RESOLUTION / RenderManager.GROUP_RESOLUTION; // = 270/45 = 6
                        int net_x0 = renderGroup.m_x * resolutionRatio;
                        int net_z0 = renderGroup.m_z * resolutionRatio;
                        int net_x1 = (renderGroup.m_x + 1) * resolutionRatio - 1; // = net_x + 5
                        int net_z1 = (renderGroup.m_z + 1) * resolutionRatio - 1; // = net_z + 5
                        for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                            for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                                int gridIndex = net_z * NetManager.NODEGRID_RESOLUTION + net_x;
                                ushort nodeID = NetManager.instance.m_nodeGrid[gridIndex];
                                int watchdog = 0;
                                while(nodeID != 0) {
                                    nodeID.ToNode().RenderInstance(cameraInfo, nodeID, renderGroup.m_instanceMask);
                                    nodeID = nodeID.ToNode().m_nextGridNode;
                                    if(++watchdog >= 32768) {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }
                            }
                        }
                        for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                            for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                                int gridIndex = net_z * 270 + net_x;
                                ushort segmentID = NetManager.instance.m_segmentGrid[gridIndex];
                                int watchdog = 0;
                                while(segmentID != 0) {
                                    segmentID.ToSegment().RenderInstance(cameraInfo, segmentID, renderGroup.m_instanceMask);
                                    segmentID = segmentID.ToSegment().m_nextGridSegment;
                                    if(++watchdog >= 36864) {
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
                    if(prefab != null) {
                        if(prefab.m_segments != null) {
                            for(int i = 0; i < prefab.m_segments.Length; i++) {
                                NetInfo.Segment segment = prefab.m_segments[i];
                                NetInfo.LodValue combinedLod = segment.m_combinedLod;
                                if(combinedLod != null && combinedLod.m_lodCount != 0) {
                                    //NetSegment.RenderLod(cameraInfo, combinedLod);
                                }
                            }
                        }
                        if(prefab.m_nodes != null) {
                            for(int i = 0; i < prefab.m_nodes.Length; i++) {
                                NetInfo.Node node = prefab.m_nodes[i];
                                NetInfo.LodValue combinedLod2 = node.m_combinedLod;
                                if(combinedLod2 != null && combinedLod2.m_lodCount != 0) {
                                    if(node.m_directConnect) {
                                        //NetSegment.RenderLod(cameraInfo, combinedLod2);
                                    } else {
                                        //NetNode.RenderLod(cameraInfo, combinedLod2);
                                    }
                                }
                            }
                        }
                    }
                }
            } catch(Exception ex) {
                ex.Log(false);
            }
        }

        public bool CalculateGroupData(int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            throw new NotImplementedException();
        }

        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps) {
            throw new NotImplementedException();
        }

        #endregion

        #region overlay
        public void BeginOverlay(RenderManager.CameraInfo cameraInfo) {
            try {
                this.m_drawCallData.m_overlayCalls = 0;
            } catch(Exception ex) {
                ex.Log(false);
            }
        }

        public void EndOverlay(RenderManager.CameraInfo cameraInfo) { }

        public void UndergroundOverlay(RenderManager.CameraInfo cameraInfo) { }

        #endregion

        public void CheckReferences() { }

        // TODO: NetManger.RebuildLods to call this
        public Coroutine RebuildLods() {
            NetInfo.ClearLodValues();
            return base.StartCoroutine(this.InitRenderDataImpl());
        }

        public void InitRenderData() {
            Singleton<LoadingManager>.instance.QueueLoadingAction(this.InitRenderDataImpl());
        }
        public IEnumerator InitRenderDataImpl() {
            throw new NotImplementedException();
        }
    }
}
