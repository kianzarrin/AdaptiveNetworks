using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;

namespace AdaptiveRoads.Patches.TMPE {
    // CitiesGameBridge.Service.NetService:
    // public void PublishSegmentChanges(ushort segmentId)
    [HarmonyPatch(typeof(JunctionRestrictionsManager))]
    [HarmonyPatch("OnSegmentChange")]
    class OnSegmentChange {
        static void Postfix(ushort segmentId, bool startNode) {
            Log.Debug("OnSegmentChange.PostFix() was called for segment:" + segmentId);
            ushort nodeID = segmentId.ToSegment().GetNode(startNode);
            NetManager.instance.UpdateNode(nodeID); // mark for update 
        }
    }
}
