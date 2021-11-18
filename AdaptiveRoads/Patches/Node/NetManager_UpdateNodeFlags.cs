namespace AdaptiveRoads.Patches.Node {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch(nameof(NetManager.UpdateNodeFlags))]
    [InGamePatch]
    class NetManager_UpdateNodeFlags {
        static void Postfix(ushort node) {
            //Log.Called(node);
            NetworkExtensionManager.Instance.UpdateNode(node);
        }
    }
}