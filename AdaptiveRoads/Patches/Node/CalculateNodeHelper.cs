namespace AdaptiveRoads.Patches.Node {
    using HarmonyLib;
    using KianCommons;
    using System;
    /// <summary>
    /// prevent updating columns when fast refreshing
    /// </summary>
    [HarmonyPatch]
    internal static class CalculateNodeHelper {
        static bool fastRefreshing;
        internal static void FastCalculateNode(ushort nodeId) {
            try {
                fastRefreshing = true;
            } catch (Exception ex) {
                nodeId.ToNode().CalculateNode(nodeId);
                ex.Log();
            } finally {
                fastRefreshing = false;
            }
        }

        [HarmonyPatch(typeof(NetNode), nameof(NetNode.UpdateBuilding))]
        static bool PrefixUpdateBuilding() => !fastRefreshing; // skip if fast fastRefreshing
    }
}
