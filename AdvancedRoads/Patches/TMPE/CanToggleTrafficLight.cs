namespace AdvancedRoads.Patches.TMPE {
    using System.Reflection;
    using TrafficManager.Manager.Impl;
    using CSUtil.Commons;
    using AdvancedRoads;
    using AdvancedRoads.Util;
    using HarmonyLib;
    using TrafficManager.API.Traffic.Enums;

    [HarmonyPatch]
    public static class CanToggleTrafficLight {
        public static MethodBase TargetMethod() {
            return typeof(TrafficLightManager).
                GetMethod(nameof(TrafficLightManager.CanToggleTrafficLight));
        }

        public static bool Prefix(ref bool __result, ushort nodeId, ref ToggleTrafficLightError reason) {
            var nodeData = NetworkExtManager.Instance.buffer[nodeId];
            return PrefixUtils.HandleTernaryBool(
                nodeData?.CanHaveTrafficLights(out reason),
                ref __result);
        }
    }
}