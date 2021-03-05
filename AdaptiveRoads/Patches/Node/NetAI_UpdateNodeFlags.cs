namespace AdaptiveRoads {
    using HarmonyLib;
    using KianCommons;
    using AdaptiveRoads.Manager;

    //[HarmonyPatch(typeof(NetAI))]
    //[InGamePatch]
    //[HarmonyPatch(nameof(NetAI.UpdateNodeFlags))]
    class NetAI_UpdateNodeFlags {
        static void Postfix(ref NetNode data) {
            if (data.CountSegments() != 2)return;
            
            ushort nodeID = NetUtil.GetID(data);

            ref NetNodeExt netNodeExt= ref NetworkExtensionManager.Instance.NodeBuffer[nodeID];
        }
    }
}
