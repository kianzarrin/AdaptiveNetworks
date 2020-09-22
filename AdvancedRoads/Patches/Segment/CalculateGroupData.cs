using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdvancedRoads.Patches.Segment {
    using JetBrains.Annotations;
    

    [HarmonyPatch()]
    public static class CalculateGroupData {
        static string logPrefix_ = "NetSegment.CalculateGroupData Transpiler: ";

        //public bool CalculateGroupData(ushort segmentID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        static MethodInfo Target => typeof(global::NetSegment).
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
                CheckSegmentFlagsCommons.PatchCheckFlags(codes, Target);
                Log.Info(logPrefix_ + "successfully patched " + Target);
                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space