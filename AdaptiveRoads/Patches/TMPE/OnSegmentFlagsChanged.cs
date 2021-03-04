using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;
using System.Collections.Generic;
using System.Reflection;
using AdaptiveRoads.Manager;
using System.Diagnostics;
using TrafficManager.API.Traffic.Enums;

namespace AdaptiveRoads.Patches.TMPE {
    [InGamePatch]
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
                new Type[] { typeof(ushort), typeof(NetInfo.Direction), typeof(bool) })
                ?? throw new Exception("ParkingRestrictionsManager.SetParkingAllowed() not found");

            /************* priority signs */
            //public bool SetPrioritySign(ushort segmentId, bool startNode, PriorityType type, out SetPrioritySignError reason)
            yield return AccessTools.DeclaredMethod(
                typeof(TrafficPriorityManager),
                nameof(TrafficPriorityManager.SetPrioritySign),
                new Type[] { typeof(ushort), typeof(bool), typeof(PriorityType), typeof(SetPrioritySignError).MakeByRefType()})
                ?? throw new Exception("TrafficPriorityManager.SetPrioritySign not found");


            /************* vehicle restrictions */
            // public void VehicleRestrictionsManager.NotifyStartEndNode(ushort segmentId)
            yield return AccessTools.DeclaredMethod(
                typeof(VehicleRestrictionsManager),
                nameof(VehicleRestrictionsManager.NotifyStartEndNode))
                ?? throw new Exception("VehicleRestrictionsManager.NotifyStartEndNode");

            /************ Junction restrictions */
            // private void JunctionRestrictionsManager.OnSegmentChange(ushort segmentId, bool startNode, ref ExtSegment seg, bool requireRecalc)
            yield return AccessTools.DeclaredMethod(
                typeof(JunctionRestrictionsManager),
                "OnSegmentChange")
                ?? throw new Exception("JunctionRestrictionsManager.OnSegmentChange not found");
        }

        static void Postfix(ushort segmentId) {
            Log.Debug($"OnSegmentFlagsChanged.PostFix() was called for " +
                $"segment:{segmentId} " +
                $"caller:{new StackFrame(1)}");
            NetworkExtensionManager.Instance.UpdateSegment(segmentId);
        }
    }
}