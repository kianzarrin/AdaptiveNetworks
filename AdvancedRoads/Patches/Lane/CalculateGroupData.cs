using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdvancedRoads.Patches.Lane {
    [HarmonyPatch()]
    public static class CalculateGroupData {
        static string logPrefix_ = "NetLane.CalculateGroupData Transpiler: ";

        //public bool CalculateGroupData(uint laneID, NetInfo.Lane laneInfo, bool destroyed, NetNode.Flags startFlags,NetNode.Flags endFlags,
        //bool invert, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays, ref bool hasProps)
        static MethodInfo Target => typeof(global::NetLane).
            GetMethod("CalculateGroupData", BindingFlags.Public | BindingFlags.Instance);

        public static MethodBase TargetMethod() {
            var ret = Target;
            HelpersExtensions.Assert(ret != null, "did not manage to find original function to patch");
            Log.Info(logPrefix_ + "aquired method " + ret);
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckPropFlagsCommons.PatchCheckFlags(codes, Target);

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