namespace AdaptiveRoads.Manager {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ColossalFramework;
    using UnityEngine;
    using static NetManager;
    using static TrackManager;
    using KianCommons;
    public static partial class NetInfoExtionsion {
        public partial class Track {
            public void RenderLod(RenderManager.CameraInfo cameraInfo) {
                if(m_combinedLod != null && m_combinedLod.m_lodCount != 0) {
                    RenderLod(cameraInfo, m_combinedLod);
                }
            }

            public static void RenderLod(RenderManager.CameraInfo cameraInfo, NetInfo.LodValue lod) {
                // copied from NetSegment.RenderLod 
                NetManager instance = Singleton<NetManager>.instance;
                MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                materialBlock.Clear();
                Mesh mesh;
                int upperLoadCount;
                if(lod.m_lodCount <= 1) {
                    mesh = lod.m_key.m_mesh.m_mesh1;
                    upperLoadCount = 1;
                } else if(lod.m_lodCount <= 4) {
                    mesh = lod.m_key.m_mesh.m_mesh4;
                    upperLoadCount = 4;
                } else {
                    mesh = lod.m_key.m_mesh.m_mesh8;
                    upperLoadCount = 8;
                }
                for(int i = lod.m_lodCount; i < upperLoadCount; i++) {
                    lod.m_leftMatrices[i] = default(Matrix4x4);
                    lod.m_rightMatrices[i] = default(Matrix4x4);
                    lod.m_meshScales[i] = default;
                    lod.m_objectIndices[i] = default;
                    lod.m_meshLocations[i] = cameraInfo.m_forward * -100000f;
                }
                materialBlock.SetMatrixArray(instance.ID_LeftMatrices, lod.m_leftMatrices);
                materialBlock.SetMatrixArray(instance.ID_RightMatrices, lod.m_rightMatrices);
                materialBlock.SetVectorArray(instance.ID_MeshScales, lod.m_meshScales);
                materialBlock.SetVectorArray(instance.ID_ObjectIndices, lod.m_objectIndices);
                materialBlock.SetVectorArray(instance.ID_MeshLocations, lod.m_meshLocations);
                if(lod.m_surfaceTexA != null) {
                    materialBlock.SetTexture(instance.ID_SurfaceTexA, lod.m_surfaceTexA);
                    materialBlock.SetTexture(instance.ID_SurfaceTexB, lod.m_surfaceTexB);
                    materialBlock.SetVector(instance.ID_SurfaceMapping, lod.m_surfaceMapping);
                    lod.m_surfaceTexA = null;
                    lod.m_surfaceTexB = null;
                }
                if(lod.m_heightMap != null) { // TODO: this is for fence. do I need this?
                    materialBlock.SetTexture(instance.ID_HeightMap, lod.m_heightMap);
                    materialBlock.SetVector(instance.ID_HeightMapping, lod.m_heightMapping);
                    materialBlock.SetVector(instance.ID_SurfaceMapping, lod.m_surfaceMapping);
                    lod.m_heightMap = null;
                }
                if(mesh != null) {
                    Bounds bounds = default(Bounds);
                    bounds.SetMinMax(lod.m_lodMin - new Vector3(100f, 100f, 100f), lod.m_lodMax + new Vector3(100f, 100f, 100f));
                    mesh.bounds = bounds;
                    lod.m_lodMin = new Vector3(100000f, 100000f, 100000f);
                    lod.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
                    instance.m_drawCallData.m_lodCalls++;
                    instance.m_drawCallData.m_batchedCalls += lod.m_lodCount - 1;
                    Graphics.DrawMesh(mesh, Matrix4x4.identity, lod.m_material, lod.m_key.m_layer, null, 0, materialBlock);
                }
                lod.m_lodCount = 0;
            }

            #region node
            public void RenderNodeInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, int layerMask)
{
                if(!nodeID.ToNode().IsValid()) {
                    return;
                }
                NetInfo info = nodeID.ToNode().Info;
                if(!cameraInfo.Intersect(nodeID.ToNode().m_bounds)) {
                    return;
                }
                if((layerMask & info.m_netLayers) == 0){
                    return;
                }
                var flags = nodeID.ToNode().m_flags;
                if(flags.IsFlagSet(NetNode.Flags.Bend | NetNode.Flags.Junction)){
                    return;
                }
                if(flags.IsFlagSet(NetNode.Flags.Bend)) {
                    if(info.m_segments == null || info.m_segments.Length == 0) {
                        return;
                    }
                } else if(info.m_nodes == null || info.m_nodes.Length == 0) {
                    return;
                }
                uint count = (uint)CalculateRendererCount(info);
                RenderManager instance = Singleton<RenderManager>.instance;
                if(instance.RequireInstance((uint)(86016 + nodeID), count, out var instanceIndex)) {
                    int num = 0;
                    while(instanceIndex != 65535) {
                        RenderInstance(cameraInfo, nodeID, info, num, m_flags, ref instanceIndex, ref instance.m_instances[instanceIndex]);
                        if(++num > 36) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
                info.m_netAI.RenderNode(nodeID, ref this, cameraInfo);
            }

            public uint CalculateNodeRenderCount(ushort nodeID) {
                int count = 0;
                ref var node = ref nodeID.ToNode();
                for(int i = 0; i < 8; ++i) {
                    ushort sourceSegmentID = node.GetSegment(i);
                    var sourceData = sourceSegmentID.ToSegment().Info?.GetMetaData();
                    if(sourceData == null || sourceData.TrackLaneCount == 0)
                        continue;
                    for(int j = i+1; j<8; ++i) {
                        ushort targetSegmentID = node.GetSegment(i);
                        var targetData = targetSegmentID.ToSegment().Info?.GetMetaData();
                        if(targetData == null || targetData.TrackLaneCount == 0)
                            continue;
                        count += sourceData.TrackLaneCount * targetData.TrackLaneCount;
                    }
                }
                return (uint)count;
            }

            public bool RequireNodeInstance(ushort nodeID, uint count, out uint instanceIndex) =>
                Singleton<RenderManager>.instance.RequireInstance(TRACK_HOLDER_SEGMNET + nodeID, count, out instanceIndex);

            private void RenderNodeInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, int layerMask, uint renderInstanceIndex) {

            }

            private void RenderNodeInstance(RenderManager.CameraInfo cameraInfo, ushort sourceSegmentID, ushort targetSegmentID, int layerMask, ref uint renderInstanceIndex) { 

            }

            private void RenderNodeInstance(RenderManager.CameraInfo cameraInfo, LaneData sourceLane, LaneData targetLane, int layerMask, ref uint renderInstanceIndex) {

            }
            #endregion
        }
    }
}
