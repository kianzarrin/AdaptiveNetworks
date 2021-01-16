using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AdaptiveRoads.Patches.Node {
    [HarmonyPatch()]
    public static class CalculateGroupData {
        static string logPrefix_ = "NetNode.CalculateGroupData Transpiler: ";

        //public bool CalculateGroupData(ushort nodeID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        static MethodInfo Target => typeof(global::NetNode).
            GetMethod("CalculateGroupData", BindingFlags.Public | BindingFlags.Instance);

        public static MethodBase TargetMethod() {
            var ret = Target;
            Assertion.Assert(ret != null, "did not manage to find original function to patch");
            Log.Info(logPrefix_ + "aquired method " + ret);
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 1, counterGetSegment: 2); //DC
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 2, counterGetSegment: 2); //DC // CS has copy pasted code.
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 3, counterGetSegment: 1); //Junction
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 4, counterGetSegment: 0); //End

                // Bend node -> segment.Checkflags (does not use info flags.)
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 5, counterGetSegment: 0); //DC Bend 

                Log.Info(logPrefix_ + "successfully patched " + Target);
                return codes;
            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space