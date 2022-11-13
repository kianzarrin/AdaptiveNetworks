namespace AdaptiveRoads.Manager {
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KianCommons.Serialization;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Plugins;
    using UnityEngine;
    using KianCommons.IImplict;
    using static Util.Shortcuts;
    using TrafficManager.API.Notifier;
    using AdaptiveRoads.Data.NetworkExtensions;
    using AdaptiveRoads.LifeCycle;

    public static class NetworkExtensionManagerExtensions {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;
        public static ref NetNodeExt ToNodeExt(this ushort id) => ref man_.NodeBuffer[id];
        public static ref NetSegmentExt ToSegmentExt(this ushort id) => ref man_.SegmentBuffer[id];
        public static ref NetLaneExt ToLaneExt(this uint id) => ref man_.LaneBuffer[id];
    }

    [Serializable]
    public class NetworkExtensionManager : MonoBehaviour, IAwakingObject {
        #region LifeCycle

        [Obsolete("call create instead", error:true)] private NetworkExtensionManager() { }

        public void Awake() {
            try {
                LogCalled();
                Log.Debug(Environment.StackTrace);
                InitBuffers();
                this.enabled = true;
            } catch(Exception ex) { ex.Log(); }
        }

#if DEBUG
        internal static NetworkExtensionManager CreateNew() => Create();
#endif

        private static NetworkExtensionManager Create() {
            try {
                Log.Called();
                var go = new GameObject(nameof(NetworkExtensionManager), typeof(NetworkExtensionManager));
                GameObject.DontDestroyOnLoad(go);
                return go.GetComponent<NetworkExtensionManager>();
            }catch(Exception ex) { ex.Log(); }
            return null;
        }

        static NetworkExtensionManager instance_;
        public static NetworkExtensionManager Instance => instance_ ??= Create();
        public static NetworkExtensionManager RawInstance => instance_;
        public static bool Exists => instance_ != null;

        internal int SerializationCapacity =>
            (NetManager.MAX_NODE_COUNT +
            NetManager.MAX_SEGMENT_COUNT * 5 + // segment + segment Start + segment End + Segment UserValues (at least null) + Segment UserFlags (at least null)
            NetManager.MAX_LANE_COUNT) * sizeof(int);

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
                var n0 = LaneBuffer.LongCount(_l => !_l.IsEmpty);
                uint n = (uint)(ulong)n0;
                Assertion.GT((uint)LaneBuffer.Length, n, $"LaneBuffer.Length > n | n0 = {n0}");
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

            if (s.Version >= new Version(3, 8)) {
                // add to the end for forward compatibility
                // user data
                Log.Debug("serialize custom user data");
                try {
                    for (ushort i = 0; i < SegmentBuffer.Length; ++i)
                        SegmentBuffer[i].SerializeUserData(s);
                } catch (Exception ex) { ex.Log(); }
            }

        }

        public static void Deserialize(SimpleDataSerializer s) {
            try {
                instance_ = Create();
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

            if (s.Version >= new Version(3, 8)) {
                // user data
                Log.Debug("deserialize custom user data");
                try {
                    for (ushort i = 0; i < SegmentBuffer.Length; ++i)
                        SegmentBuffer[i].DeserializeUserData(s);
                } catch (Exception ex) { ex.Log(); }
            }
        }

        // should be called from simulation thread.
        void LoadImpl() {
            LogCalled();
            TMPENotifier.EventModified -= OnTMPEModified;
            UpdateAllNetworkFlags();
            UpdateAllNetworkFlags();
            TMPENotifier.EventModified -= OnTMPEModified;
            TMPENotifier.EventModified += OnTMPEModified;
#if DEBUG
            ThreadingExtensions.Handle2(OutlineData.timer1, OutlineData.counter1, "lane-outline");
            ThreadingExtensions.Handle2(OutlineData.timer2, OutlineData.counter2, "transition-outline");
#endif
            LogSucceeded();
        }

        // should be called from simulation thread.
        public void UpdateAllNetworkFlags() {
            try {
                if(!Helpers.InSimulationThread()) {
                    Log.Info("calling UpdateAllNetworkFlags() again from simulation thread");
                    SimulationManager.instance.AddAction(UpdateAllNetworkFlags);
                }
                Log.Called();
                for(ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                    if(!NetUtil.IsSegmentValid(segmentID)) continue;
                    SegmentBuffer[segmentID].UpdateAllFlags();
                }
                for(ushort nodeID = 0; nodeID < NetManager.MAX_NODE_COUNT; ++nodeID) {
                    if(!NetUtil.IsNodeValid(nodeID)) continue;
                    NodeBuffer[nodeID].UpdateFlags();
                }
                for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                    if (!NetUtil.IsSegmentValid(segmentID)) continue;
                    SegmentBuffer[segmentID].UpdateScriptedFlags();
                }
                for (ushort nodeID = 0; nodeID < NetManager.MAX_NODE_COUNT; ++nodeID) {
                    if (!NetUtil.IsNodeValid(nodeID)) continue;
                    NodeBuffer[nodeID].UpdateScriptedFlags();
                }
                for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                    if(!NetUtil.IsSegmentValid(segmentID)) continue;
                    NetManager.instance.UpdateSegmentRenderer(segmentID, true);
                }
                for(ushort nodeID = 0; nodeID < NetManager.MAX_NODE_COUNT; ++nodeID) {
                    if(!NetUtil.IsNodeValid(nodeID)) continue;
                    NetManager.instance.UpdateNodeRenderer(nodeID, true);
                }
            } catch(Exception ex) {
                ex.Log();
            }
        }

        public void HotReload() {
            if (NetInfoExtionsion.EditedNetInfo == null) {
                // don't do this in asset editor. It's done when when road editor refreshes.
                NetManager.instance.RebuildLods(); 
            }
            RecalculateARPrefabs();
        }

        public static void OnTMPELoaded() {
            SimulationManager.instance.AddAction(Instance.LoadImpl);
        }

        // should be called from main thread.
        public void RecalculateARPrefabs() {
            int n = PrefabCollection<NetInfo>.LoadedCount();
            for(uint i = 0; i < n; ++i) {
                var prefab = PrefabCollection<NetInfo>.GetLoaded(i);
                prefab?.RecalculateMetaData();
            }
        }

        private void OnTMPEModified(OnModifiedEventArgs args) {
            if (args.InstanceID.Type == InstanceType.NetNode)
                UpdateNode(args.InstanceID.NetNode, level: -2);
            if (args.InstanceID.Type == InstanceType.NetSegment)
                UpdateSegment(args.InstanceID.NetSegment, level:-2);
        }

        public void OnUnload() {
            TMPENotifier.EventModified -= OnTMPEModified;
            Destroy(gameObject);
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

#endregion LifeCycle

        public void SimulationStep() {
            bool updateSegments = m_segmentsUpdated;
            bool updateNodes = m_nodesUpdated;
            m_segmentsUpdated = false;
            m_nodesUpdated = false;

            for (int i = 0; i < 2; ++i) {
                if (updateSegments) {
                    for (int maskIndex = 0; maskIndex < m_updatedSegments.Length; maskIndex++) {
                        ulong bitmask = m_updatedSegments[maskIndex];
                        if (bitmask != 0) {
                            for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
                                if ((bitmask & 1UL << bitIndex) != 0UL) {
                                    ushort segmentID = (ushort)(maskIndex << 6 | bitIndex);
                                    if (Log.VERBOSE) Log.Debug($"updating segment:{segmentID} ...");
                                    SegmentBuffer[segmentID].UpdateAllFlags();
                                }
                            }
                        }
                    }
                }

                if (updateNodes) {
                    for (int maskIndex = 0; maskIndex < m_updatedNodes.Length; maskIndex++) {
                        ulong bitmask = m_updatedNodes[maskIndex];
                        if (bitmask != 0) {
                            for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
                                if ((bitmask & 1UL << bitIndex) != 0) {
                                    ushort nodeID = (ushort)(maskIndex << 6 | bitIndex);
                                    if (Log.VERBOSE) Log.Debug($"updating node:{nodeID} ...");
                                    NodeBuffer[nodeID].UpdateFlags();
                                }
                            }
                        }
                    }
                }

                if (updateSegments) {
                    for (int maskIndex = 0; maskIndex < m_updatedSegments.Length; maskIndex++) {
                        ulong bitmask = m_updatedSegments[maskIndex];
                        if (bitmask != 0) {
                            for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
                                if ((bitmask & 1UL << bitIndex) != 0UL) {
                                    ushort segmentID = (ushort)(maskIndex << 6 | bitIndex);
                                    if (Log.VERBOSE) Log.Debug($"updating segment:{segmentID} ...");
                                    SegmentBuffer[segmentID].UpdateScriptedFlags();
                                }
                            }
                        }
                    }
                }

                if (updateNodes) {
                    for (int maskIndex = 0; maskIndex < m_updatedNodes.Length; maskIndex++) {
                        ulong bitmask = m_updatedNodes[maskIndex];
                        if (bitmask != 0) {
                            for (int bitIndex = 0; bitIndex < 64; bitIndex++) {
                                if ((bitmask & 1UL << bitIndex) != 0) {
                                    ushort nodeID = (ushort)(maskIndex << 6 | bitIndex);
                                    if (Log.VERBOSE) Log.Debug($"updating node:{nodeID} ...");
                                    NodeBuffer[nodeID].UpdateScriptedFlags();
                                }
                            }
                        }
                    }
                }
            }

            if (updateSegments) {
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

            if(updateNodes) {
                for(int maskIndex = 0; maskIndex < m_updatedNodes.Length; maskIndex++) {
                    ulong bitmask = m_updatedNodes[maskIndex];
                    if(bitmask != 0) {
                        m_updatedNodes[maskIndex] = 0;
                        for(int bitIndex = 0; bitIndex < 64; bitIndex++) {
                            if((bitmask & 1UL << bitIndex) != 0) {
                                ushort nodeID = (ushort)(maskIndex << 6 | bitIndex);
                                NetManager.instance.UpdateNodeRenderer(nodeID, true);
                            }
                        }

                    }
                }
            }
        }


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

#region data transfer
        private byte[] CopyInstanceID(InstanceID instanceID) {
            throw new NotImplementedException();
        }

        private void PasteInstanceID(byte[] data, Dictionary<InstanceID, InstanceID> map) {
            if (data == null)
                return;
        }
#endregion

        public void UpdateNode(ushort nodeID, ushort fromSegmentID = 0, int level = -1) {
            if(!nodeID.ToNode().IsValid()) return;
            if(!Helpers.InSimulationThread()) {
                if (Log.VERBOSE) Log.Debug("send to simulation thread");
                SimulationManager.instance.AddAction(() => UpdateNode(nodeID, fromSegmentID, level));
                return;
            }
            if(Log.VERBOSE) Log.Debug($"mark node:{nodeID} info:{nodeID.ToNode().Info} update-level={level} from segment:{fromSegmentID}" /*+ Environment.StackTrace*/,false);
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
            if(!segmentID.ToSegment().IsValid()) return;
            if(!Helpers.InSimulationThread()) {
                if (Log.VERBOSE) Log.Debug("send to simulation thread");
                SimulationManager.instance.AddAction(() => UpdateSegment(segmentID, fromNodeID, level));
                return;
            }
            if (Log.VERBOSE) Log.Debug($"mark segment:{segmentID} info:{segmentID.ToSegment().Info} update-level={level} from node:{fromNodeID}" /*+ Environment.StackTrace*/,false);
            m_updatedSegments[segmentID >> 6] |= 1UL << (int)segmentID;
            m_segmentsUpdated = true;
            if (level <= 0) {
                ushort node1 = segmentID.ToSegment().m_startNode;
                ushort node2 = segmentID.ToSegment().m_endNode;
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
