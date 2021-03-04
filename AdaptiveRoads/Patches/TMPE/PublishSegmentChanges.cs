using HarmonyLib;
using KianCommons;
using CitiesGameBridge.Service;
namespace AdaptiveRoads.Patches.TMPE {
    // CitiesGameBridge.Service.NetService:
    // public void PublishSegmentChanges(ushort segmentId)
    [InGamePatch]
    [HarmonyPatch(typeof(NetService))]
    [HarmonyPatch(nameof(NetService.PublishSegmentChanges))]
    class PublishSegmentChanges {
        static void Postfix(ushort segmentId) {
            Log.Debug("PublishSegmentChanges.PostFix() was called for segment:" + segmentId);
            ushort nodeID1 = segmentId.ToSegment().m_startNode;
            ushort nodeID2 = segmentId.ToSegment().m_endNode;
            NetManager.instance.UpdateNode(nodeID1); // mark for update 
            NetManager.instance.UpdateNode(nodeID2); // mark for update 
        }
    }
}