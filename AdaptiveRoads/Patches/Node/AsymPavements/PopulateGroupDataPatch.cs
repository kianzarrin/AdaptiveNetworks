namespace AdaptiveRoads.Patches.AsymPavements {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons.Patches;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;

    // non-dc node lod
    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch2(typeof(NetNode), typeof(PopulateGroupData), instance: true)]
    static class PopulateGroupDataPatch {
        delegate void PopulateGroupData(NetNode instance, ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            var codes = instructions.ToCodeList();

            var ldSegmentID = new CodeInstruction(OpCodes.Ldloc_S, 79); // TODO acquire dynamically
            var ldSegmentIDA = new CodeInstruction(OpCodes.Ldloc_S, 106); // TODO acquire dynamically
            var ldSegmentIDB = new CodeInstruction(OpCodes.Ldloc_S, 107); // TODO acquire dynamically

            return Commons.ApplyPatch(codes, ldSegmentID, ldSegmentIDA, ldSegmentIDB);

        }
    }
}