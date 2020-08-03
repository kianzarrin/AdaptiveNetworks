namespace AdvancedRoads {
    using KianCommons;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class NetworkExtensionManager {
        #region LifeCycle
        // this initalizesation maybe useful in case of a hot reload.
        public static NetworkExtensionManager Instance { get; private set; } = new NetworkExtensionManager();

        public static byte[] Serialize() => SerializationUtil.Serialize(Instance);

        public static void Deserialize(byte[] data) {
            if (data == null) {
                Instance = new NetworkExtensionManager();
                Log.Debug($"NetworkExtensionManager.Deserialize(data=null)");
            } else {
                Log.Debug($"NetworkExtensionManager.Deserialize(data): data.Length={data?.Length}");
                Instance = SerializationUtil.Deserialize(data) as NetworkExtensionManager;
            }
        }

        public void OnLoad() {
            Log.Debug("NetworkExtensionManager.OnLoad() called");
            for(ushort nodeID=0;nodeID< NetManager.MAX_NODE_COUNT;++nodeID) {
                if (NetUtil.IsNodeValid(nodeID)) {
                    NetManager.instance.UpdateNode(nodeID);
                }
            }
            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                if (NetUtil.IsSegmentValid(segmentID)) {
                    NetManager.instance.UpdateSegment(segmentID);
                }
            }
        }
        public void OnAfterDeserialize() {
             
        }

        #endregion LifeCycle

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





    }
}
