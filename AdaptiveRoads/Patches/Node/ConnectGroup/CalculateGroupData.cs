using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AdaptiveRoads.Patches.Node.ConnectGroup {
    [HarmonyPatch()]
    [InGamePatch]
    public static class CalculateGroupData {
        //public bool CalculateGroupData(ushort nodeID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        public static MethodBase TargetMethod() => typeof(global::NetNode).
            GetMethod("CalculateGroupData", BindingFlags.Public | BindingFlags.Instance, true);

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);

                // (node.m_connectGroup == NetInfo.ConnectGroup.None || (node.m_connectGroup & info3.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None)
                // (node2.m_connectGroup == NetInfo.ConnectGroup.None || (node2.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None)
                // (node5.m_connectGroup == NetInfo.ConnectGroup.None || (node5.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None)
                // (info2.m_nodeConnectGroups & info3.m_connectGroup) != NetInfo.ConnectGroup.None || (info3.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None)
                // (info2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info3.m_connectGroup) != NetInfo.ConnectGroup.None)
                // (info3.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info3.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None)
                CheckNodeConnectGroupNone.Patch(codes, original); // 3
                CheckNodeConnectGroup.Patch(codes, original); // 3
                CheckNetConnectGroupNone.Patch(codes, original); // 2
                CheckNetConnectGroup.Patch(codes, original); // 4

                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space