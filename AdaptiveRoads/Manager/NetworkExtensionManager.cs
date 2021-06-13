namespace AdaptiveRoads.Manager {
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KianCommons.Serialization;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Plugins;
    using TrafficManager.API;

    [Serializable]
    public class NetworkExtensionManager {
        #region LifeCycle
        private NetworkExtensionManager() {
            LogCalled();
            InitBuffers();
        }

#if DEBUG
        internal static NetworkExtensionManager CreateNew() => new NetworkExtensionManager();
#endif

        static NetworkExtensionManager instance_;
        public static NetworkExtensionManager Instance => instance_ ??= new NetworkExtensionManager();

        public static NetworkExtensionManager RawInstance => instance_;

        public static NetworkExtensionManager Ensure() => Instance;

        internal int SerializationCapacity =>
            (NetManager.MAX_NODE_COUNT + NetManager.MAX_SEGMENT_COUNT + NetManager.MAX_LANE_COUNT) * sizeof(int);

        public void Serialize(SimpleDataSerializer s) {
            try {
                Assertion.NotNull(s);
                LogCalled("s.version=" + s.Version);
                for (ushort i = 0; i < SegmentBuffer.Length; ++i)
                    SegmentBuffer[i].Serialize(s);
            } catch (Exception ex) { ex.Log(); }
            try {
                for (int i = 0; i < SegmentEndBuffer.Length; ++i)
                    SegmentEndBuffer[i].Serialize(s);
            } catch (Exception ex) { ex.Log(); }
            try {
                for (ushort i = 0; i < NodeBuffer.Length; ++i)
                    NodeBuffer[i].Serialize(s);
            } catch (Exception ex) { ex.Log(); }
            try {
                uint n = (uint)LaneBuffer.LongCount(_l => !_l.IsEmpty);
                s.WriteUInt32(n);
                Log.Debug($"Serializing {n} lanes");
                for (uint i = 0; i < LaneBuffer.Length; ++i) {
                    if (LaneBuffer[i].IsEmpty) continue;
                    s.WriteUInt32(i);
                    LaneBuffer[i].Serialize(s);
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }
        public static void Deserialize(SimpleDataSerializer s) {
            try {
                instance_ = new NetworkExtensionManager();
                if (s != null)
                    instance_.DeserializeImp(s);
            } catch (Exception ex) {
                ex.Log();
            }
        }

        internal void DeserializeImp(SimpleDataSerializer s) {
            try {
                Assertion.NotNull(s);
                LogCalled("s.version=" + s.Version);
                for (ushort i = 0; i < SegmentBuffer.Length; ++i)
                    SegmentBuffer[i].Deserialize(s);
            } catch (Exception ex) { ex.Log("failed to deserialize segments"); }
            try {
                for (int i = 0; i < SegmentEndBuffer.Length; ++i)
                    SegmentEndBuffer[i].Deserialize(s);
            } catch (Exception ex) { ex.Log("failed to deserialize segment ends"); }
            try {
                for (ushort i = 0; i < NodeBuffer.Length; ++i)
                    NodeBuffer[i].Deserialize(s);
            } catch (Exception ex) { ex.Log("failed to deserialize nodes"); }

            try {
                uint n = s.ReadUInt32();
                Assertion.GT((uint)LaneBuffer.Length, n, "LaneBuffer.Length > n");
                Log.Debug($"deserializing {n} lanes");
                for (uint i = 0; i < n; ++i) {
                    uint laneID = s.ReadUInt32();
                    Assertion.GT((uint)LaneBuffer.Length, laneID, "LaneBuffer.Length > laneID");
                    LaneBuffer[laneID].Deserialize(s);
                }
            } catch (Exception ex) {
                ex.Log($"failed to deserialize lanes");
            }
        }


        // should be called from simulation thread.
        void OnTMPELoadedImpl() {
            LogCalled();
            for (ushort nodeID = 0; nodeID < NetManager.MAX_NODE_COUNT; ++nodeID) {
                if (!NetUtil.IsNodeValid(nodeID)) continue;
                NodeBuffer[nodeID].UpdateFlags();
            }
            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                if (!NetUtil.IsSegmentValid(segmentID)) continue;
                if (!segmentID.ToSegment().Info.IsAdaptive()) continue;
                SegmentBuffer[segmentID].UpdateAllFlags();
#if HOTRELOAD // change to debug for hot-reload testing.
                NetManager.instance.UpdateSegment(segmentID);
#endif
            }

            Notifier.EventModified -= OnTMPEModified;
            Notifier.EventModified += OnTMPEModified;
            LogSucceeded();
        }

        public static void OnTMPELoaded() {
            Ensure();
            SimulationManager.instance.AddAction(Instance.OnTMPELoadedImpl);
        }

        private void OnTMPEModified(OnModifiedEventArgs args) {
            if (args.InstanceID.Type == InstanceType.NetNode)
                UpdateNode(args.InstanceID.NetNode, level: -2);
            if (args.InstanceID.Type == InstanceType.NetSegment)
                UpdateSegment(args.InstanceID.NetSegment, level:-2);
        }

        public void OnUnload() {
            Notifier.EventModified -= OnTMPEModified;
            instance_ = null;
        }

        //public void OnAfterDeserialize() { }

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

            for (int i = 0; i < 3; ++i) {
                if (segmentsUpdated) {
                    for (int maskIndex = 0; maskIndex < m_updatedSegments.Length; maskIndex++) {
                        ulong bitmask = m_updatedSegments[maskIndex];
                        if (bitmask != 0) {
                            for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
                                if ((bitmask & 1UL << bitIndex) != 0UL) {
                                    ushort segmentID = (ushort)(maskIndex << 6 | bitIndex);
                                    Log.Debug($"updating {segmentID} ...");
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
                            for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
                                if ((bitmask & 1UL << bitIndex) != 0) {
                                    ushort nodeID = (ushort)(maskIndex << 6 | bitIndex);
                                    NodeBuffer[nodeID].UpdateFlags();
                                }
                            }
                        }
                    }
                }
                if (!this.m_nodesUpdated && !this.m_segmentsUpdated) {
                    break;
                }
                // the line bellow is commented out because I am worried about
                // never ending circle.
                // if(i == 3) return; // try again another frame.
                if (this.m_nodesUpdated) {
                    nodesUpdated = true;
                    this.m_nodesUpdated = false;
                }
                if (this.m_segmentsUpdated) {
                    segmentsUpdated = true;
                    this.m_segmentsUpdated = false;
                }
            }

            if (nodesUpdated) {
                for (int maskIndex = 0; maskIndex < m_updatedNodes.Length; maskIndex++) {
                    ulong bitmask = m_updatedNodes[maskIndex];
                    if (bitmask != 0) {

                        m_updatedNodes[maskIndex] = 0;
                        for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
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
                        m_updatedSegments[maskIndex] = 0;
                        for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
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
        public readonly bool HUT = PluginUtil.GetHideUnconnectedTracks().IsActive();
        [NonSerialized]
        public readonly bool HTC = PluginUtil.GetHideCrossings().IsActive();
        [NonSerialized]
        public readonly bool DCR = PluginUtil.GetDirectConnectRoads().IsActive();

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

        public ref NetSegmentEnd GetSegmentEnd(ushort segmentId, ushort nodeId) {
            return ref SegmentBuffer[segmentId].GetEnd(nodeId);
        }

        #region data tranfer
        private byte[] CopyInstanceID(InstanceID instanceID) {
            throw new NotImplementedException();
        }

        private void PasteInstanceID(byte[] data, Dictionary<InstanceID, InstanceID> map) {
            if (data == null)
                return;
        }
        #endregion

        public void UpdateNode(ushort nodeID, ushort fromSegmentID = 0, int level = -1) {
            m_updatedNodes[nodeID >> 6] |= 1UL << (int)nodeID;
            m_nodesUpdated = true;
            if (level <= 0) {
                for (int i = 0; i < 8; ++i) {
                    ushort segmentID = nodeID.ToNode().GetSegment(i);
                    if (segmentID == 0 || segmentID == fromSegmentID)
                        continue;
                    UpdateSegment(segmentID, nodeID, level + 1);
                }
            }
        }

        public void UpdateSegment(ushort segmentID, ushort fromNodeID = 0, int level = -1) {
            Log.Debug($"mark segment:{segmentID} for update level={level}" /*+ Environment.StackTrace*/);
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
