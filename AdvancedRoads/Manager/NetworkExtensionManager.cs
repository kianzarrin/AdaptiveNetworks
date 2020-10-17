namespace AdaptiveRoads.Manager {
    using KianCommons;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class NetworkExtensionManager {
        #region LifeCycle
        private NetworkExtensionManager(){
        }
        // this initalizesation maybe useful in case of a hot reload.
        public static NetworkExtensionManager Instance { get; private set; } = new NetworkExtensionManager();

        public static byte[] Serialize() => SerializationUtil.Serialize(Instance);

        public static void Deserialize(byte[] data, Version version) {
            if (data == null) {
                Instance = new NetworkExtensionManager();
                Log.Debug($"NetworkExtensionManager.Deserialize(data=null)");
            } else {
                Log.Debug($"NetworkExtensionManager.Deserialize(data): data.Length={data?.Length}");
                Instance = SerializationUtil.Deserialize(data, version) as NetworkExtensionManager;
            }
        }

        public void OnLoad() {
            Log.Debug("NetworkExtensionManager.OnLoad() called");
            InitBuffers();
            for (ushort nodeID=0;nodeID< NetManager.MAX_NODE_COUNT;++nodeID) {
                if (NetUtil.IsNodeValid(nodeID)) {
                    NodeBuffer[nodeID].UpdateFlags();
                }
            }
            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                if (!NetUtil.IsSegmentValid(segmentID)) continue;
                if (segmentID.ToSegment().Info.GetExt() == null) continue;
                SegmentBuffer[segmentID].UpdateAllFlags();
            }
        }

        public void OnUnload() {
            Instance = new NetworkExtensionManager();
        }

        public void OnAfterDeserialize() {
             
        }

        public void InitBuffers() {
            for (ushort nodeID = 1; nodeID < NetManager.MAX_NODE_COUNT; ++nodeID)
                NodeBuffer[nodeID].Init(nodeID);
            for (ushort segmentID = 1; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                SegmentBuffer[segmentID].Init(segmentID);
                GetSegmentEnd(segmentID, startNode: true).Init(segmentID, startNode: true);
                GetSegmentEnd(segmentID, startNode: false).Init(segmentID, startNode: false);
            }
            for (uint laneId = 0; laneId < NetManager.MAX_LANE_COUNT; ++laneId) {
                LaneBuffer[laneId].Init(laneId);
            }

        }

        public void SimulationStep() {
            bool nodesUpdated = m_nodesUpdated;
            bool segmentsUpdated = m_segmentsUpdated;
            if (nodesUpdated) m_nodesUpdated = false;
            if (segmentsUpdated) m_segmentsUpdated = false;

            if (nodesUpdated) {
                for (int maskIndex = 0; maskIndex < m_updatedNodes.Length; maskIndex++) {
                    ulong bitmask = m_updatedNodes[maskIndex];
                    if (bitmask != 0) {
                        for (int bitIndex = 0; bitIndex < sizeof(ulong); bitIndex++) {
                            if ((bitmask & 1UL << bitIndex) != 0) {
                                ushort nodeID = (ushort)(maskIndex << 6 | bitIndex);
                                NodeBuffer[nodeID].UpdateFlags();
                            }
                        }
                    }
                }
            }

            if (segmentsUpdated) {
                for (int maskIndex = 0; maskIndex < m_updatedSegments.Length; maskIndex++) {
                    ulong bitmask = m_updatedSegments[maskIndex];
                    if (bitmask != 0) {
                        for (int bitIndex = 0; bitIndex < sizeof(ulong); bitIndex++) {
                            if ((bitmask & 1UL << bitIndex) != 0UL) {
                                ushort segmentID = (ushort)(maskIndex << 6 | bitIndex);
                                SegmentBuffer[segmentID].UpdateAllFlags();
                            }
                        }
                    }
                }
            }

            if (nodesUpdated) {
                for (int maskIndex = 0; maskIndex < m_updatedNodes.Length; maskIndex++) {
                    ulong bitmask = m_updatedNodes[maskIndex];
                    if (bitmask != 0) {
                        for (int bitIndex = 0; bitIndex < sizeof(ulong); bitIndex++) {
                            if ((bitmask & 1UL << bitIndex) != 0) {
                                ushort nodeID = (ushort)(maskIndex << 6 | bitIndex);
                                NetManager.instance.UpdateNodeRenderer(nodeID, true);
                            }
                        }
                    }
                }
            }

            if (segmentsUpdated) {
                for (int maskIndex = 0; maskIndex < m_updatedSegments.Length; maskIndex++) {
                    ulong bitmask = m_updatedSegments[maskIndex];
                    if (bitmask != 0) {
                        for (int bitIndex = 0; bitIndex < sizeof(ulong); bitIndex++) {
                            if ((bitmask & 1UL << bitIndex) != 0UL) {
                                ushort segmentID = (ushort)(maskIndex << 6 | bitIndex);
                                NetManager.instance.UpdateSegmentRenderer(segmentID, true);
                            }
                        }
                    }
                }
            }

        }
        #endregion LifeCycle

        [NonSerialized]
        public ulong[] m_updatedNodes = new ulong[512], m_updatedSegments = new ulong[576];

        [NonSerialized]
        public bool m_nodesUpdated, m_segmentsUpdated;

        public NetNodeExt[] NodeBuffer = new NetNodeExt[NetManager.MAX_NODE_COUNT];
        public NetSegmentExt[] SegmentBuffer = new NetSegmentExt[NetManager.MAX_SEGMENT_COUNT];
        public NetLaneExt[] LaneBuffer = new NetLaneExt[NetManager.MAX_LANE_COUNT];
        public NetSegmentEnd[] SegmentEndBuffer = new NetSegmentEnd[NetManager.MAX_SEGMENT_COUNT * 2];
        public ref NetSegmentEnd GetSegmentEnd(ushort segmentID, bool startNode) {
            if (startNode)
                return ref SegmentEndBuffer[segmentID * 2];
            else
                return ref SegmentEndBuffer[segmentID * 2 + 1];
        }

        public NetSegmentEnd GetSegmentEnd(ushort segmentId, ushort nodeId) {
            return SegmentBuffer[segmentId].GetEnd(nodeId);
        }

        #region data tranfer
        private byte[] CopyInstanceID(InstanceID instanceID) {
            throw new NotImplementedException();
        }

        private void PasteInstanceID(byte[] data, Dictionary<InstanceID,InstanceID> map) {
            if (data == null)
                return;
        }
        #endregion

        public void UpdateNode(ushort nodeID, ushort fromSegmentID = 0, int level=0) {
            m_updatedNodes[nodeID >> 6] |= 1UL << (int)nodeID;
            m_nodesUpdated = true;
            if (level <= 1) {
                for (int i = 0; i < 8; ++i) {
                    ushort segmentID = nodeID.ToNode().GetSegment(i);
                    if (segmentID == 0 || segmentID == fromSegmentID)
                        continue;
                    UpdateSegment(segmentID, nodeID, level + 1);
                }
            }
        }

        public void UpdateSegment(ushort segmentID, ushort fromNodeID = 0, int level = 0 ) {
            m_updatedSegments[segmentID >> 6] |= 1UL << (int)segmentID;
            m_segmentsUpdated = true;
            if (level <= 0) {
                ushort node1 = segmentID.ToSegment().GetNode(true);
                ushort node2 = segmentID.ToSegment().GetNode(false);
                if (node1 != 0 && node1 != fromNodeID) {
                    UpdateNode(node1, segmentID, level + 1);
                }
                if (node2 != 0 && node2 != fromNodeID) {
                    UpdateNode(node2, segmentID, level + 1);
                }
            }
        }




    }
}
