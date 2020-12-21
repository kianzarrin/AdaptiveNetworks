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
            var ldSegmentIDA = new CodeInstruction(OpCodes.Ldloc_S, 20);
            var ldSegmentIDB = new CodeInstruction(OpCodes.Ldloc_S, 21);
            int index;

            //index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 1); //main left
            //codes.InsertInstructions(index + 1, //after
            //    new[] {
            //        ldSegmentID.Clone(), //ldInfo
            //        new CodeInstruction(OpCodes.Ldc_I4_1), // occurance
            //        new CodeInstruction(OpCodes.Call, mModifyPavement),
            //    });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 2); //A left
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentIDA.Clone(),
                    ldSegmentIDA.Clone(), //doesn't matter
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

            //index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 4); //B right
            //codes.InsertInstructions(index + 1, //after
            //    new[] {
            //        ldSegmentIDB.Clone(),
            //        new CodeInstruction(OpCodes.Ldc_I4_4), // occurance
            //        new CodeInstruction(OpCodes.Call, mModifyPavement),
            //    });

            //index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 5); //.z
            //codes.InsertInstructions(index + 1, //after
            //    new[] {
            //        ldSegmentID.Clone(),
            //        new CodeInstruction(OpCodes.Ldc_I4_5), // occurance
            //        new CodeInstruction(OpCodes.Call, mModifyPavement),
            //    });

            //index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 6); //.w
            //codes.InsertInstructions(index + 1, //after
            //    new[] {
            //        ldSegmentID.Clone(),
            //        new CodeInstruction(OpCodes.Ldc_I4_6), // occurance
            //        new CodeInstruction(OpCodes.Call, mModifyPavement),
            //    });
            return codes;
        }

        static MethodInfo mModifyPavement = GetMethod(typeof(RefreshJunctionDataPatch), nameof(ModifyPavement));
        static FieldInfo f_pavementWidth = typeof(NetInfo).GetField("m_pavementWidth");

        public static float ModifyPavement(float width, ushort segmentID, ushort segmentID2, int occurance) {
            NetInfo info = segmentID.ToSegment().Info;
            if (info.GetMetaData() is NetInfoExtionsion.Net netData) {
                // 5=left
                // 2=right
                // 9.5=right2 

                if (occurance == 2) {
                    return netData.PavementWidthRight;
                }else if(occurance == 3) {
                    // LeftPavementWidth * HalfWidth / RightPavementWidth - HalfWdith
                    float A = info.m_pavementWidth * info.m_halfWidth / netData.PavementWidthRight - info.m_halfWidth;
                    float B = info.m_pavementWidth;
                    var info2 = segmentID2.ToSegment().Info;
                    float r = info2.m_pavementWidth / info2.m_halfWidth;
                    return A * r + B;
                }
                throw new Exception("unexpected occurance:" + occurance);

                //width = occurance switch
                //{
                //    1 => info.m_pavementWidth, //main Left (5=>-3)
                //    2 => netData.m_pavementWidthRight, //A left 
                //    3 => netData.m_pavementWidthRight2, //main right (2=>9.5)
                //    4 => info.m_pavementWidth, //B right
                //    5 => info.m_pavementWidth, //.z
                //    6 => info.m_pavementWidth, //.w
                //    _ => throw new Exception("unexoected occurance:" + occurance),
                //};
            }
            return width;
        }

    }
}