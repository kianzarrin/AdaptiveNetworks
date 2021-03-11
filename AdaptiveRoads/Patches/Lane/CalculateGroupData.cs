using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdaptiveRoads.Patches.Lane {
    [InGamePatch]
    [HarmonyPatch]
    [HarmonyBefore("com.klyte.redirectors.PS")]
    public static class CalculateGroupData {
        //public bool CalculateGroupData(uint laneID, NetInfo.Lane laneInfo, bool destroyed, NetNode.Flags startFlags,NetNode.Flags endFlags,
        //bool invert, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays, ref bool hasProps)
        static MethodBase TargetMethod() => typeof(global::NetLane).
            GetMethod("CalculateGroupData", BindingFlags.Public | BindingFlags.Instance, true );


        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckPropFlagsCommons.PatchCheckFlags(codes, original);
                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space