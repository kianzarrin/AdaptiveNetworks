using HarmonyLib;
using KianCommons;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.Patches.Node {
    [HarmonyPatch(typeof(NetNode))]
    [HarmonyPatch(nameof(NetNode.UpdateNode))]
    class NetNode_UpdateNode {
        static void Postfix(ushort nodeID) {
            //Log.Debug("NetNode_UpdateNode.PostFix() was called for node:" + nodeID);
            NetworkExtensionManager.Instance.UpdateNode(nodeID);
        }
    }
}