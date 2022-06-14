namespace AdaptiveRoads.Patches.TMPE {
    using HarmonyLib;
    using KianCommons;
    using System.Reflection;
    using TrafficManager.API.Manager;
    using static Commons;

    [HarmonyPatch]
    [InGamePatch]
    static class GetDefaultEnteringBlockedJunctionAllowed {
        static bool Prepare() => TargetMethod() != null;

        static MethodBase TargetMethod() {
            return JRManType.GetMethod(nameof(IJunctionRestrictionsManager.GetDefaultEnteringBlockedJunctionAllowed));
        }

        static bool Prefix(ushort segmentId, bool startNode, ref bool __result) {
            if (IsRoadWithDCJunction(segmentId, startNode)) {
                __result = true;
                return false;
            } else {
                return true;
            }
        }
    }
}