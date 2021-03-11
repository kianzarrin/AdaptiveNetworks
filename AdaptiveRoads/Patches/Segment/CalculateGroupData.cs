using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;
using static KianCommons.ReflectionHelpers;

namespace AdaptiveRoads.Patches.Segment {
    using JetBrains.Annotations;


    [InGamePatch]
    [HarmonyPatch()]
    public static class CalculateGroupData {

        //public bool CalculateGroupData(ushort segmentID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        public static MethodBase TargetMethod() => typeof(NetSegment).GetMethod(
            "CalculateGroupData", BindingFlags.Public | BindingFlags.Instance, throwOnError:true);

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckSegmentFlagsCommons.PatchCheckFlags(codes, original);
                Log.Info($"{ThisMethod} patched {original} successfully!");
                return codes;
            }
            catch (Exception ex) {
                Log.Error(ex.ToString());
                throw ex;
            }
        }
    } // end class
} // end name space