using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;
namespace AdaptiveRoads.Patches.TMPE {
    // ParkingRestrictionsManager:
    // public bool SetParkingAllowed(ushort segmentId, NetInfo.Direction finalDir, bool flag) {
    [HarmonyPatch(typeof(ParkingRestrictionsManager))]
    [HarmonyPatch(nameof(ParkingRestrictionsManager.SetParkingAllowed))]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(NetInfo.Direction), typeof(bool)})]
    static class SetParkingAllowed {
        static void Postfix(ushort segmentId) {
            Log.Debug("SetParkingAllowed.PostFix() was called for segment:" + segmentId);
            NetManager.instance.UpdateSegment(segmentId); // mark for update - also updates both nodes.
        }
    }
}