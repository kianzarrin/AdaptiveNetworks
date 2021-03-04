namespace AdaptiveRoads.Patches {
    using ColossalFramework;
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
    using System.Linq;

    // protected Vector4 CitizenAI.GetPathTargetPosition(ushort, ref CitizenInstance, ref CitizenInstance.Frame, float)
    [UsedImplicitly]
    //[HarmonyPatch]
    static class GetPathTargetPositionPatch {
        delegate Vector4 dGetPathTargetPosition
            (ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, float minSqrDistance);

        [UsedImplicitly]
        static MethodBase TargetMethod() =>
            DeclaredMethod<dGetPathTargetPosition>(typeof(CitizenAI), "GetPathTargetPosition");

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            var codes = instructions.ToCodeList();

            int index_pavementWith = codes.Search(_c => _c.LoadsField(f_pavementWidth));
            var ldInfo = codes[index_pavementWith - 1];

            // seach for ldInfo that is followed by ldelem.ref
            int index1 = codes.Search(
                _c => _c.opcode == OpCodes.Ldelem_Ref, startIndex: index_pavementWith, count: -1);
            int index0 = codes.Search(
                _c => _c.IsSameInstruction(ldInfo), startIndex: index1, count: -1);
            // find the correct ldelem.ref
            index1 = codes.Search(
                _c => _c.opcode == OpCodes.Ldelem_Ref, startIndex: index0);

            // load ref laneInfo:
            var newCodes = codes.GetRange(index0, index1 - index0 + 1)
                .Select(_c => _c.Clone())
                .ToList();

            newCodes.Add(ldInfo.Clone());
            newCodes.Add(new CodeInstruction(OpCodes.Call, mModifyPavement));
            codes.InsertInstructions(index_pavementWith + 1, newCodes); //insert after
            return codes;
        }

        static MethodInfo mModifyPavement = GetMethod(typeof(RefreshJunctionDataPatch), nameof(ModifyPavement));
        static FieldInfo f_pavementWidth = typeof(NetInfo).GetField("m_pavementWidth");

        public static float ModifyPavement(float width, ref NetInfo.Lane laneInfo, NetInfo info) {
            if (info.GetMetaData() is NetInfoExtionsion.Net netData) {
                if (laneInfo.m_position > 0)
                    width = netData.PavementWidthRight;
            }
            return width;
        }
    }
}
