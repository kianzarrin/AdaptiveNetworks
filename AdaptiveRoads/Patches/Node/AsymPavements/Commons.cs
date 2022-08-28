namespace AdaptiveRoads.Patches.AsymPavements {
    using AdaptiveRoads.Manager;
    using Epic.OnlineServices.Presence;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;
    using KianCommons;

    public static class Commons {
        static FieldInfo f_pavementWidth = typeof(NetInfo).GetField("m_pavementWidth");
        static MethodInfo mModifyPavement = typeof(Commons).GetMethod(nameof(ModifyPavement), throwOnError: true);
        static MethodInfo mCalcualtePavementRatio = typeof(Commons).GetMethod(nameof(CalcualtePavementRatio), throwOnError: true);

        public static List<CodeInstruction> ApplyPatch(
            List<CodeInstruction> codes,
            MethodBase method,
            CodeInstruction ldSegmentID,
            CodeInstruction ldSegmentIDA,
            CodeInstruction ldSegmentIDB) {
            Log.Called(method, ldSegmentID, ldSegmentIDA, ldSegmentIDB);

            int index;

            /****************************************************
             * non-invert */
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 2); //A left
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentIDA.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_2), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 3); //main right
            {
                // finding pavementRatio_avgB:
                int iStore = codes.Search(_c => _c.IsStLoc(typeof(float), method), startIndex: index);
                int loc = codes[iStore].GetLoc();
                int iStore2 = codes.Search(_c => _c.IsStLoc(loc), startIndex: index, count: 2);
                int iLoad = codes.Search(_c => _c.IsLdLoc(loc), startIndex: iStore2, count: -1);
                //float pavementRatio_avgB = info.m_pavementWidth / info.m_halfWidth * 0.5f; // don't modify variable here because it is used to calculate start/end ratio
                //...
                //    float pavementRatioB = infoB.m_pavementWidth / infoB.m_halfWidth * 0.5f;
                //    if (dot_B > -0.5f) {
                //        startRatioB = Mathf.Clamp(startRatioB * (2f * pavementRatioB / (pavementRatio_avgB + pavementRatioB)), 0.05f, 0.7f);
                //        endRatioB = Mathf.Clamp(endRatioB * (2f * pavementRatio_avgB / (pavementRatio_avgB + pavementRatioB)), 0.05f, 0.7f);
                //    }
                //    pavementRatio_avgB = (pavementRatio_avgB /* modify varible here */+ pavementRatioB) * 0.5f;

                codes.InsertInstructions(iLoad + 1, //after
                    new[] {
                    ldSegmentID.Clone(),
                    ldSegmentIDB.Clone(),
                    new CodeInstruction(OpCodes.Ldc_I4_3), // occurance
                    new CodeInstruction(OpCodes.Call, mCalcualtePavementRatio),
                });
            }

            /****************************************************
             * invert */
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 1); //A left
            {
                // finding pavementRatio_avgA:
                int iStore = codes.Search(_c => _c.IsStLoc(typeof(float), method), startIndex: index);
                int loc = codes[iStore].GetLoc();
                int iStore2 = codes.Search(_c => _c.IsStLoc(loc), startIndex: index, count: 2);
                int iLoad = codes.Search(_c => _c.IsLdLoc(loc), startIndex: iStore2, count: -1);
                //float pavementRatio_avgA = info.m_pavementWidth / info.m_halfWidth * 0.5f; // don't modify variable here because it is used to calculate start/end ratio
                //...
                //    float pavementRatioA = infoA.m_pavementWidth / infoA.m_halfWidth * 0.5f;
                //    if (dot_A > -0.5f) {
                //        startRatioA = Mathf.Clamp(startRatioA * (2f * pavementRatioA / (pavementRatio_avgA + pavementRatioA)), 0.05f, 0.7f);
                //        endRatioA = Mathf.Clamp(endRatioA * (2f * pavementRatio_avgA / (pavementRatio_avgA + pavementRatioA)), 0.05f, 0.7f);
                //    }
                //    pavementRatio_avgA = (pavementRatio_avgA /* modify varible here */+ pavementRatioA) * 0.5f;

                codes.InsertInstructions(iLoad + 1, //after
                    new[] {
                    ldSegmentID.Clone(),
                    ldSegmentIDA.Clone(),
                    new CodeInstruction(OpCodes.Ldc_I4_1), // occurance
                    new CodeInstruction(OpCodes.Call, mCalcualtePavementRatio),
                });
            }

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 4); //main right
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentIDB.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_4), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            /********************************
             * m_dataVector0 */
            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 5); //m_dataVector0.z
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_5), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            index = codes.Search(_c => _c.LoadsField(f_pavementWidth), count: 6); //m_dataVector0.w
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentID.Clone(), // does not matter
                    new CodeInstruction(OpCodes.Ldc_I4_6), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            string m = "\n";
            foreach(var code in codes) m += code + "\n";
            Log.Debug(m);
            Log.Succeeded();

            return codes;
        }

        public static float CalcualtePavementRatio(float pavementRatio /*discard*/, ushort segmentId, ushort segmentID2, int occurance) {
            NetInfo info = segmentId.ToSegment().Info;
            float pavementWidth = ModifyPavement(info.m_pavementWidth, segmentId /*does not matter*/, segmentID2, occurance);
            return pavementWidth / info.m_halfWidth * 0.5f;
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