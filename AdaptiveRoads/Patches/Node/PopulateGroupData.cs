using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AdaptiveRoads.Patches.Node {
    [HarmonyPatch()]
    public static class PopulateGroupData {
        static string logPrefix_ = "NetNode.PopulateGroupData Transpiler: ";

        //public void PopulateGroupData(ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        static MethodInfo Target => typeof(global::NetNode).
            GetMethod("PopulateGroupData", BindingFlags.Public | BindingFlags.Instance);

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
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 2, counterGetSegment: 2); //DC

                // Unlike RenderInstance and CalculateGroupData, counterGetSegment for PopulateGroupData Junction is 2:
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 3, counterGetSegment: 2); //Junction
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occuranceCheckFlags: 4, counterGetSegment: 0); // End

                // End - BEND ->  segment.Checkflags (does not use info flags.)
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