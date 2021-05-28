namespace AdaptiveRoads.Patches.Node.ConnectGroup {
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [HarmonyPatch()]
    [InGamePatch]
    public static class RenderInstance {
        public static MethodBase TargetMethod() =>
            typeof(NetNode)
            .GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance, throwOnError:true);

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                // (node.m_connectGroup == NetInfo.ConnectGroup.None || (node.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None)
                // (node4.m_connectGroup == NetInfo.ConnectGroup.None || (node4.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None)
                CheckNodeConnectGroupNone.Patch(codes, original);
                CheckNodeConnectGroup.Patch(codes, original);

                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            } catch(Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space
