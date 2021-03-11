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
        //public void PopulateGroupData(ushort segmentID, uint laneID, NetInfo.Lane laneInfo, bool destroyed,
        //NetNode.Flags startFlags, NetNode.Flags endFlags, float startAngle, float endAngle, bool invert, bool terrainHeight,
        //int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data,
        //ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool hasProps)
        public static MethodBase TargetMethod() => typeof(global::NetLane).
            GetMethod("PopulateGroupData", BindingFlags.Public | BindingFlags.Instance, true);

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