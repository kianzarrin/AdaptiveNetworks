namespace AdaptiveRoads.Patches.AsymPavements {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;

    public static class Commons {
        static FieldInfo f_pavementWidth = typeof(NetInfo).GetField("m_pavementWidth");
        static MethodInfo mModifyPavement = GetMethod(typeof(Commons), nameof(ModifyPavement));

        public static List<CodeInstruction> ApplyPatch(
            List<CodeInstruction> codes,
            CodeInstruction ldSegmentID,
            CodeInstruction ldSegmentIDA,
            CodeInstruction ldSegmentIDB) {
            Log.Called(ldSegmentID, ldSegmentIDA, ldSegmentIDB);

            int index;

            /****************************************************
             * non-invert */
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 2); //A left
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentIDA.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_2), // occurrence
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 3); //main right
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentIDB.Clone(),
                    new CodeInstruction(OpCodes.Ldc_I4_3), // occurrence
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            /****************************************************
             * invert */
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 1); //A left
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentIDA.Clone(),
                    new CodeInstruction(OpCodes.Ldc_I4_1), // occurrence
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 4); //main right
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentIDB.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_4), // occurrence
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            /********************************
             * m_dataVector0 */
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 5); //m_dataVector0.z
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_5), // occurrence
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 6); //m_dataVector0.w
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_6), // occurrence
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });
            return codes;
        }

        public static float ModifyPavement(float width, ushort segmentID, ushort segmentID2, int occurance) {
            ref var segment = ref segmentID.ToSegment();
            NetInfo info = segment.Info;
            NetInfo info2 = segmentID2.ToSegment().Info;
            if (info.GetMetaData() is not NetInfoExtionsion.Net netData)
                return width;

            float pwLeft = info.m_pavementWidth;
            float pwRight = netData.PavementWidthRight;
            if (pwLeft == pwRight) return width;
            float pwSmall = Mathf.Min(pwLeft, pwRight);
            float pwBig = Mathf.Max(pwLeft, pwRight);

            ushort nodeID = segment.GetSharedNode(segmentID2);
            bool startNode = segment.IsStartNode(nodeID);
            bool reverse = startNode ^ segment.IsInvert();

            var op = Util.GetOperation(occurance: occurance, reverse: reverse, biggerLeft: pwLeft < pwRight);
            switch (op) {
                case Util.Operation.PWBig:
                    return pwBig;
                case Util.Operation.PWSmall:
                    return pwSmall;
                case Util.Operation.PWAR: {
                        float A = pwBig / pwSmall - 1;
                        float r = info2.m_pavementWidth * info.m_halfWidth / info2.m_halfWidth;
                        return A * r + pwBig;
                    }
                case Util.Operation.PWAR2: {
                        float pwRight2;
                        if (info2.GetMetaData() is NetInfoExtionsion.Net net2)
                            pwRight2 = net2.PavementWidthRight;
                        else
                            pwRight2 = info2.m_pavementWidth;

                        float A = pwBig / pwSmall - 1;
                        float r = pwRight2 * info.m_halfWidth / info2.m_halfWidth;
                        return A * r + pwBig;
                    }
                case Util.Operation.PWForced:
                    return Util.GetForced(occurance: occurance, reverse: reverse, biggerLeft: pwLeft < pwRight);
                default:
                    return width;
            }
        }
    }
}