using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;

namespace AdvancedRoads.Patches.TMPE {
    // CitiesGameBridge.Service.NetService:
    // public void PublishSegmentChanges(ushort segmentId)
    [HarmonyPatch(typeof(SegmentEndManager))]
    [HarmonyPatch(nameof(SegmentEndManager.UpdateSegmentEnd))]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(bool) })]
    class UpdateSegmentEnd {
        static void Postfix(ushort segmentId, bool startNode) {
            Log.Debug("UpdateSegmentEnd.PostFix() was called for segment:" + segmentId);
            NetManager.instance.UpdateSegment(segmentId); // mark for update 
        }
    }
}
