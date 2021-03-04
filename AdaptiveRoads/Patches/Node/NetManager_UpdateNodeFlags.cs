namespace AdaptiveRoads.Patches.Node {
    using AdaptiveRoads.Manager;
    using HarmonyLib;

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch(nameof(NetManager.UpdateNodeFlags))]
    [InGamePatch]
    class NetManager_UpdateNodeFlags {
        static void Postfix(ushort node) {
            NetworkExtensionManager.Instance.NodeBuffer[node].UpdateFlags();
        }
    }
}