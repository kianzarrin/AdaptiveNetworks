using HarmonyLib;
using KianCommons;
using CitiesGameBridge.Service;
namespace AdvancedRoads.Patches.TMPE {
    // CitiesGameBridge.Service.NetService:
    // public void PublishSegmentChanges(ushort segmentId)
    [HarmonyPatch(typeof(NetService))]
    [HarmonyPatch(nameof(NetService.PublishSegmentChanges))]
    class PublishSegmentChanges {
        static void Postfix(ushort segmentId) {
            Log.Debug("PublishSegmentChanges.PostFix() was called for segment:" + segmentId);
            NetManager.instance.UpdateSegment(segmentId); // mark for update - also updates both nodes.
        }
    }
}