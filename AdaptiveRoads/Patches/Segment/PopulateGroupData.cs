using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdaptiveRoads.Patches.Segment {
    using JetBrains.Annotations;


    [InGamePatch]
    [HarmonyPatch()]
    public static class PopulateGroupData {
        static string logPrefix_ = "NetSegment.PopulateGroupData Transpiler: ";

        //public void PopulateGroupData(ushort segmentID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition,
        // RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        static MethodInfo Target => typeof(global::NetSegment).
            GetMethod("PopulateGroupData", BindingFlags.Public | BindingFlags.Instance);

        public static MethodBase TargetMethod() {
            var ret = Target;
            Assertion.Assert(ret != null, "did not manage to find original function to patch");
            Log.Info(logPrefix_ + "aquired method " + ret);
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckSegmentFlagsCommons.PatchCheckFlags(codes, original);
                Log.Info(logPrefix_ + "successfully patched " + original);
                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space