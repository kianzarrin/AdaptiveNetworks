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
    public static class PopulateGroupData {
        //public void PopulateGroupData(ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        public static MethodBase TargetMethod() => typeof(global::NetNode).
            GetMethod("PopulateGroupData", BindingFlags.Public | BindingFlags.Instance, true);

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);

                CheckNodeConnectGroupNone.Patch(codes, original);
                CheckNodeConnectGroup.Patch(codes, original);
                CheckNetConnectGroupNone.Patch(codes, original);
                CheckNetConnectGroup.Patch(codes, original);

                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space