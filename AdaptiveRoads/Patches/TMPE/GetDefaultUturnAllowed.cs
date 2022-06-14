namespace AdaptiveRoads.Patches.TMPE {
    using HarmonyLib;
    using KianCommons.Plugins;
    using System.Reflection;
    using TrafficManager.API.Manager;
    using static Commons;

    [HarmonyPatch]
    [InGamePatch]
    static class GetDefaultUturnAllowed {
        static bool Prepare() => TargetMethod() != null;

        static MethodBase TargetMethod() {
            return JRManType.GetMethod(nameof(IJunctionRestrictionsManager.GetDefaultUturnAllowed));
        }

        static bool Prefix(ushort segmentId, bool startNode, ref bool __result) {
            if (IsRoadWithDCJunction(segmentId, startNode)) {
                __result = false;
                return false;
            } else {
                return true;
            }
        }
    }
}