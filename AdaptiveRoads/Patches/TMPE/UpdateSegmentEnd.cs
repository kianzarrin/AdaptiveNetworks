using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;

namespace AdaptiveRoads.Patches.TMPE {
    // CitiesGameBridge.Service.NetService:
    // public void PublishSegmentChanges(ushort segmentId)
#pragma warning disable CS0618 // Type or member is obsolete
    [HarmonyPatch(typeof(SegmentEndManager))]
    [HarmonyPatch(nameof(SegmentEndManager.UpdateSegmentEnd))]
#pragma warning restore CS0618 // Type or member is obsolete
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(bool) })]
    class UpdateSegmentEnd {
        static void Postfix(ushort segmentId, bool startNode) {
            Log.Debug("SegmentEndManager.UpdateSegmentEnd.PostFix() was called for segment:" + segmentId);
            ushort nodeID = segmentId.ToSegment().GetNode(startNode);
            NetManager.instance.UpdateNode(nodeID); // mark for update 
        }
    }
}
