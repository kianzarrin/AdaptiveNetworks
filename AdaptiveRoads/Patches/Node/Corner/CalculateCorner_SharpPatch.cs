namespace AdaptiveRoads.Patches.Node.Corner;
using AdaptiveRoads.Manager;
using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

[HarmonyPatch]
public static class CalculateCorner_SharpPatch {
    public static bool SharpnerOverriden;

    static MethodBase TargetMethod() {
        // public void CalculateCorner(ushort segmentID, bool heightOffset, bool start, bool leftSide,
        // out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
        return typeof(NetSegment).GetMethod(
                nameof(NetSegment.CalculateCorner),
                BindingFlags.Public | BindingFlags.Static) ??
                throw new System.Exception("CalculateCornerPatch Could not find target method.");
    }

    delegate float Max(float a, float b);

    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original) {
        var codes = instructions.ToList();
        MethodInfo mMax = typeof(Mathf).GetMethod<Max>(throwOnError: true);
        MethodInfo mModifySharpness = typeof(CalculateCorner_SharpPatch).GetMethod(nameof(ModifySharpness), throwOnError: true);

        int iLoadSharpness = codes.Search(c => c.LoadsConstant(2f), count: 3);

        codes.InsertInstructions(iLoadSharpness + 1, new[] {
            // 2 is already on the stack
            TranspilerUtils.GetLDArg(original, "ignoreSegmentID"),
            TranspilerUtils.GetLDArg(original, "startNodeID"),
            new CodeInstruction(OpCodes.Call, mModifySharpness),
        });

        return codes;

    }

    public static float ModifySharpness(float sharpness, ushort segmentId, ushort nodeId) {
        if (SharpnerOverriden) {
            var data = segmentId.ToSegment().Info?.GetMetaData();
            if (data != null && data.SharpCorners) {
                const float OFFSET_SAFETYNET = 0.02f;
                sharpness = OFFSET_SAFETYNET;
            }
        }
        return sharpness;
    }

}