using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;
using System.Collections.Generic;
using System.Reflection;
using AdaptiveRoads.Manager;
using System.Diagnostics;

namespace AdaptiveRoads.Patches.TMPE {
    [HarmonyPatch]
    static class OnSegmentFlagsChanged {
        static IEnumerable<MethodBase> TargetMethods() {
            /************* speed limit */
            // public bool SpeedLimitManager.SetSpeedLimit(*)
            foreach (var m in AccessTools.GetDeclaredMethods(typeof(SpeedLimitManager))) {
                if (m.Name == nameof(SpeedLimitManager.SetSpeedLimit))
                    yield return m;
            }

            /************* parking */
            // public bool ParkingRestrictionsManager.SetParkingAllowed(ushort segmentId, NetInfo.Direction finalDir, bool flag) {
            yield return AccessTools.DeclaredMethod(
                typeof(ParkingRestrictionsManager),
                nameof(ParkingRestrictionsManager.SetParkingAllowed),
                new Type[] { typeof(ushort), typeof(NetInfo.Direction), typeof(bool) });

            /************* priority signs */
            //public bool SetPrioritySign(ushort segmentId, bool startNode, PriorityType type, out SetPrioritySignError reason)
            yield return AccessTools.DeclaredMethod(
                typeof(TrafficPriorityManager),
                nameof(TrafficPriorityManager.SetPrioritySign),
                new Type[] { typeof(ushort), typeof(NetInfo.Direction), typeof(bool) });

            /************* vehicle restrictions */
            // public void VehicleRestrictionsManager.NotifyStartEndNode(ushort segmentId)
            yield return AccessTools.DeclaredMethod(
                typeof(VehicleRestrictionsManager),
                nameof(VehicleRestrictionsManager.NotifyStartEndNode));

            /************* vehicle restrictions */
            yield return AccessTools.DeclaredMethod(
                typeof(VehicleRestrictionsManager),
                nameof(VehicleRestrictionsManager.NotifyStartEndNode));

            /************ Junction restrictions */
            // private void JunctionRestrictionsManager.OnSegmentChange(ushort segmentId, bool startNode, ref ExtSegment seg, bool requireRecalc)
            yield return AccessTools.DeclaredMethod(
                typeof(JunctionRestrictionsManager),
                "OnSegmentChange");
        }

        static void Postfix(ushort segmentId) {
            Log.Debug($"OnSegmentFlagsChanged.PostFix() was called for " +
                $"segment:{segmentId} " +
                $"caller:{new StackFrame(1)}");
            NetworkExtensionManager.Instance.UpdateSegment(segmentId);
        }
    }
}