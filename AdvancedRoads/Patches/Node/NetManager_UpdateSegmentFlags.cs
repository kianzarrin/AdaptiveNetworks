using HarmonyLib;
using KianCommons;
namespace AdvancedRoads.Patches.Node {
    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch(nameof(NetManager.UpdateNodeFlags))]
    class NetManager_UpdateNodeFlags {
        static void Postfix(ushort node) {
            ushort nodeID = node;
            //Log.Debug("NetNode_UpdateNode.PostFix() was called for node:" + nodeID);
            if (!NetUtil.IsNodeValid(nodeID)) return;
            ref NetNodeExt netNodeExt = ref NetworkExtensionManager.Instance.NodeBuffer[nodeID];

            netNodeExt.UpdateFlags();
        }
    }
}