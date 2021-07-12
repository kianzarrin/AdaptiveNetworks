namespace AdaptiveRoads.Patches.AsymPavements {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;
    using AdaptiveRoads.Manager;
    using System.Diagnostics;

    // non-dc node
    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch2(typeof(NetNode), typeof(RefreshJunctionData), instance:true)]
    static class RefreshJunctionDataPatch {
        delegate void RefreshJunctionData(NetNode instance, ushort nodeID, int segmentIndex, ushort nodeSegment, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            var codes = instructions.ToCodeList();

            var ldSegmentID = GetLDArg(original, "nodeSegment");
            var ldSegmentIDA = new CodeInstruction(OpCodes.Ldloc_S, 20); // TODO aquire dynamically
            var ldSegmentIDB = new CodeInstruction(OpCodes.Ldloc_S, 21); // TODO aquire dynamically

            return Commons.ApplyPatch(codes, ldSegmentID, ldSegmentIDA, ldSegmentIDB);
     
        }
    }
}