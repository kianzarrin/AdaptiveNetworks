namespace AdaptiveRoads.Patches.Corner {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons.Patches;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using KianCommons;

    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch2(typeof(NetNode), typeof(PopulateGroupData), instance: true)]
    static class PopulateGroupDataPatch {
        delegate void PopulateGroupData(NetNode instance, ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps);

        static FieldInfo f_requireSegmentRenderers = ReflectionHelpers.GetField<NetInfo>(nameof(NetInfo.m_requireSegmentRenderers));
        static MethodInfo mGetSegment = typeof(NetNode).GetMethod(nameof(NetNode.GetSegment));

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            var codes = instructions.ToCodeList();
            int iGetSegment1 = codes.Search(c => c.Calls(mGetSegment), count: 1);
            int iStSegmentID1 = codes.Search(c => c.IsStloc(), startIndex: iGetSegment1);
            var ldSegmentID1 = TranspilerUtils.BuildLdLocFromStLoc(codes[iStSegmentID1]);

            int iGetSegment2 = codes.Search(c => c.Calls(mGetSegment), count: 1);
            int iStSegmentID2 = codes.Search(c => c.IsStloc(), startIndex: iGetSegment2);
            var ldSegmentID2 = TranspilerUtils.BuildLdLocFromStLoc(codes[iStSegmentID2]);

            codes.InsertInstructions(iStSegmentID2 + 1, new[] {
                ldSegmentID1,
                ldSegmentID2,
                new CodeInstruction(OpCodes.Call, ShiftData.mPrefix),
            });

            // DC part of the code is before m_requireSegmentRenderers
            int i_requireSegmentRenderers = codes.Search(c => c.LoadsField(f_requireSegmentRenderers));
            codes.InsertInstructions(i_requireSegmentRenderers, new[] {
                new CodeInstruction(OpCodes.Call, ShiftData.mPostfix),
            });

            return codes;
        }
    }
}
