namespace AdaptiveRoads.Patches.Node {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;

    [InGamePatch]
    [HarmonyPatch(typeof(NetNode), nameof(NetNode.CalculateNode))]
    class CalculateNode {
        static void Postfix(ref NetNode __instance) {
            if (!__instance.IsValid()) return;
            ushort nodeID = NetUtil.GetID(__instance);
            NetworkExtensionManager.Instance.UpdateNode(nodeID);
        } // end postfix
    }
}