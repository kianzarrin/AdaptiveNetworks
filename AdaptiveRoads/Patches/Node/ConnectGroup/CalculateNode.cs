namespace AdaptiveRoads.Patches.Node.ConnectGroup {
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [InGamePatch]
    [HarmonyPatch(typeof(NetNode), nameof(NetNode.CalculateNode))]
    public static class CalculateNodePatch {
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);

                // (info3.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info3.m_connectGroup) != NetInfo.ConnectGroup.None)
                // (info2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info3.m_connectGroup) != NetInfo.ConnectGroup.None)
                // (info3.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info3.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None)
                CheckNetConnectGroupNone.Patch(codes, original); // 2
                CheckNetConnectGroup.Patch(codes, original); // 4

                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;

            } // end class
        } // end name space
    }
}
