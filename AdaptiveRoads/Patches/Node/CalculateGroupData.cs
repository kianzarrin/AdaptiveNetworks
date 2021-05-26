using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AdaptiveRoads.Patches.Node {
    [HarmonyPatch()]
    [InGamePatch]
    public static class CalculateGroupData {
        //public bool CalculateGroupData(ushort nodeID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        public static MethodBase TargetMethod() => typeof(global::NetNode).
            GetMethod("CalculateGroupData", BindingFlags.Public | BindingFlags.Instance, true);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 1, counterGetSegment: 2, true); //DC
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 2, counterGetSegment: 2, true); //DC // CS has copy pasted code.
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 3, counterGetSegment: 1); //Junction
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 4, counterGetSegment: 0); //End

                // Bend node -> segment.Checkflags (does not use info flags.)
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 5, counterGetSegment: 0, true); //DC Bend 

                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space