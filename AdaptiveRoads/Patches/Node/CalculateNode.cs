using ColossalFramework;
using HarmonyLib;
using KianCommons;

namespace AdaptiveRoads.Patches {
    
    //[HarmonyPatch(typeof(NetNode), nameof(NetNode.CalculateNode))]
    class CalculateNode {
        static void Postfix(ref NetNode __instance) {
            //Log.Debug("CalculateNode.PostFix() was called");
            ushort nodeID = NetUtil.GetID(__instance);
            if (!NetUtil.IsNodeValid(nodeID)) return;
            //NetworkExtensionManager.Instance.OnBeforeCalculateNode(nodeID);

        } // end postfix
    }
}