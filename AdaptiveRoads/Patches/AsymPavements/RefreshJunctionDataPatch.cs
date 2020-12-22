namespace NodeController.Patches {
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

    // non-dc node
    // private void NetNode.RefreshJunctionData(
    //      ushort nodeID, int segmentIndex, ushort nodeSegment, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data
    [UsedImplicitly]
    [HarmonyPatch]
    static class RefreshJunctionDataPatch {
        [UsedImplicitly]

        static MethodBase TargetMethod() {
            return AccessTools.Method(
            typeof(NetNode),
            "RefreshJunctionData",
            new Type[] {
                typeof(ushort),
                typeof(int),
                typeof(ushort),
                typeof(Vector3),
                typeof(uint).MakeByRefType(),
                typeof(RenderManager.Instance).MakeByRefType()
            });
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            var codes = instructions.ToCodeList();
            var ldSegmentID = GetLDArg(original, "nodeSegment");

            // TODO aquire dynamically
            var ldSegmentIDA = new CodeInstruction(OpCodes.Ldloc_S, 20);
            var ldSegmentIDB = new CodeInstruction(OpCodes.Ldloc_S, 21);
            int index;

            // non-invert
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 2); //A left
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentIDA.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_2), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 3); //main right
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentIDB.Clone(),
                    new CodeInstruction(OpCodes.Ldc_I4_3), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            // invert
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 1); //A left
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentIDA.Clone(),
                    new CodeInstruction(OpCodes.Ldc_I4_1), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 4); //main right
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentIDB.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_4), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });
            return codes;
        }

        static MethodInfo mModifyPavement = GetMethod(typeof(RefreshJunctionDataPatch), nameof(ModifyPavement));
        static FieldInfo f_pavementWidth = typeof(NetInfo).GetField("m_pavementWidth");

        public static float ModifyPavement(float width, ushort segmentID, ushort segmentID2, int occurance) {
            ref var segment = ref segmentID.ToSegment();
            NetInfo info = segment.Info;
            if (info.GetMetaData() is NetInfoExtionsion.Net netData) {
                ushort nodeID = segment.GetSharedNode(segmentID2);
                bool startNode = segment.IsStartNode(nodeID);
                bool reverse = startNode ^ segment.IsInvert();
                switch (occurance) {
                    case 2:
                    case 3:
                        if (reverse)
                            return width;
                        break;
                    case 1:
                    case 4:
                        if (!reverse)
                            return width;
                        break;
                    default:
                        throw new ArgumentException("unexpected occurance:" + occurance);
                }

                switch (occurance) {
                    case 2:
                    case 4:
                        return netData.PavementWidthRight;
                    case 1:
                    case 3:
                        float pwLeft = info.m_pavementWidth;
                        float pwRight = netData.PavementWidthRight;
                        float A = pwLeft / pwRight - 1;
                        var info2 = segmentID2.ToSegment().Info;
                        float r = info2.m_pavementWidth * info.m_halfWidth / info2.m_halfWidth;
                        return A * r + pwLeft;
                    default:
                        throw new ArgumentException("unexpected occurance:" + occurance);
                }
            }
            return width;
        }

    }
}