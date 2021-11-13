namespace AdaptiveRoads.Manager.delete {
    using ColossalFramework;
    using KianCommons;
    using System;
    using static TrackManager;
#if false
    public static class NetInfoExtionsion2 {
        public class Track2 {
#region node
            public void RenderNodeInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, int layerMask) {
                if(!nodeID.ToNode().IsValid()) {
                    return;
                }
                NetInfo info = nodeID.ToNode().Info;
                if(!cameraInfo.Intersect(nodeID.ToNode().m_bounds)) {
                    return;
                }
                if((layerMask & info.m_netLayers) == 0) {
                    return;
                }
                var flags = nodeID.ToNode().m_flags;
                if(flags.IsFlagSet(NetNode.Flags.Bend | NetNode.Flags.Junction)) {
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
                    for(int j = i + 1; j < 8; ++i) {
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
#endif
}
