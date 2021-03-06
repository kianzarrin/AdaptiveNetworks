using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdaptiveRoads.Patches.Lane {


    [HarmonyPatch]
    [InGamePatch]
    [HarmonyBefore("com.klyte.redirectors.PS")]
    public static class PopulateGroupData {
        static string logPrefix_ = "NetLane.PopulateGroupData Transpiler: ";

        //public void PopulateGroupData(ushort segmentID, uint laneID, NetInfo.Lane laneInfo, bool destroyed,
        //NetNode.Flags startFlags, NetNode.Flags endFlags, float startAngle, float endAngle, bool invert, bool terrainHeight,
        //int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data,
        //ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool hasProps)
        static MethodInfo Target => typeof(global::NetLane).
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