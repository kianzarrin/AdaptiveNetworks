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
    public static class PopulateGroupData {
        //public void PopulateGroupData(ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        public static MethodBase TargetMethod() => typeof(global::NetNode).
            GetMethod("PopulateGroupData", BindingFlags.Public | BindingFlags.Instance, true);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 1, counterGetSegment: 2, true); //DC
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 2, counterGetSegment: 2, true); //DC

                // Unlike RenderInstance and CalculateGroupData, counterGetSegment for PopulateGroupData Junction is 2:
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 3, counterGetSegment: 2); //Junction
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 4, counterGetSegment: 0); // End

                // End - BEND ->  segment.Checkflags (does not use info flags.)
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