using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace AdaptiveRoads.Patches.TMPE {
    // ParkingRestrictionsManager:
    // public bool SetParkingAllowed(ushort segmentId, NetInfo.Direction finalDir, bool flag) {
    [HarmonyPatch]
    static class SetSpeedLimit {
        static IEnumerable<MethodBase> TargetMethods() {
            foreach (var m in AccessTools.GetDeclaredMethods(typeof(SpeedLimitManager))) {
                if (m.Name == nameof(SpeedLimitManager.SetSpeedLimit))
                    yield return m;
            }
        }
        static void Postfix(ushort segmentId) {
            Log.Debug("SetSpeedLimit.PostFix() was called for segment:" + segmentId);
            NetManager.instance.UpdateSegment(segmentId); // mark for update - also updates both nodes.
        }
    }
}