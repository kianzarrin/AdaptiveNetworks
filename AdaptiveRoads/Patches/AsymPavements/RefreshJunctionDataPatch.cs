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
    // private void NetNode.RefreshJunctionData(
    //      ushort nodeID, int segmentIndex, ushort nodeSegment, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data
    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch]
    static class RefreshJunctionDataPatch {
        [UsedImplicitly]
        static FieldInfo f_pavementWidth = typeof(NetInfo).GetField("m_pavementWidth");

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
            var ldSegmentIDA = new CodeInstruction(OpCodes.Ldloc_S, 20); // TODO aquire dynamically
            var ldSegmentIDB = new CodeInstruction(OpCodes.Ldloc_S, 21); // TODO aquire dynamically
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
            codes.InsertInstructions(index + 1, //after
                new[] {
                    ldSegmentID.Clone(),
                    ldSegmentIDB.Clone(),
                    new CodeInstruction(OpCodes.Ldc_I4_3), // occurance
                    new CodeInstruction(OpCodes.Call, mModifyPavement),
                });

            /****************************************************
             * invert */
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
            return codes;
        }

        static MethodInfo mModifyPavement = GetMethod(typeof(RefreshJunctionDataPatch), nameof(ModifyPavement));


        public static class Util {
            public enum Operation {
                None, //return input width
                PWBig,
                PWSmall,
                PWAR,
            }

            [Flags]
            public enum Geometry {
                None = 0,
                Reverse = 1,
                BiggerLeft = 2,
            }

            public static Geometry GetGeometry(bool reverse, bool biggerLeft) {
                Geometry ret = Geometry.None;
                if (reverse)
                    ret |= Geometry.Reverse;
                if (biggerLeft)
                    ret |= Geometry.BiggerLeft;
                return ret;
            }

            public static Operation GetOperation(int occurance, bool reverse, bool biggerLeft) {
                int index = occurance - 1;
                var geometry = GetGeometry(reverse: reverse, biggerLeft: biggerLeft);
                return Operations[index, (int)geometry];
            }

            const int CASE_COUNT = 6;
            const int GEOMETRY_COUNT = 4;
            static Operation[,] Operations = new Operation[CASE_COUNT, GEOMETRY_COUNT] {

                //     right          right-reverse       left             left-reverse    */                                                
                { Operation.PWBig  , Operation.PWAR   , Operation.PWAR   , Operation.PWBig  }, // case 1
                { Operation.PWSmall, Operation.PWBig  , Operation.PWBig  , Operation.PWSmall}, // case 2
                { Operation.PWAR   , Operation.PWBig  , Operation.PWBig  , Operation.PWAR   }, // case 3
                { Operation.PWBig  , Operation.PWSmall, Operation.PWSmall, Operation.PWBig  }, // case 4
                { Operation.PWBig  , Operation.PWBig  , Operation.PWBig  , Operation.PWBig  }, // case 5
                { Operation.PWBig  , Operation.PWBig  , Operation.PWBig  , Operation.PWBig  }, // case 6
            };
        }



        public static float ModifyPavement(float width, ushort segmentID, ushort segmentID2, int occurance) {
            ref var segment = ref segmentID.ToSegment();
            NetInfo info = segment.Info;
            NetInfo info2 = segmentID2.ToSegment().Info;
            if (info.GetMetaData() is not NetInfoExtionsion.Net netData)
                return width;

            float pwLeft = info.m_pavementWidth;
            float pwRight = netData.PavementWidthRight;
            float pwSmall = Mathf.Min(pwLeft, pwRight);
            float pwBig = Mathf.Max(pwLeft, pwRight);
            if (pwLeft == pwRight)
                return width;

            ushort nodeID = segment.GetSharedNode(segmentID2);
            bool startNode = segment.IsStartNode(nodeID);
            bool reverse = startNode ^ segment.IsInvert();

            var op = Util.GetOperation(occurance: occurance, reverse: reverse, biggerLeft: pwLeft < pwRight);
            switch (op) {
                case Util.Operation.PWBig:
                    return pwBig;
                case Util.Operation.PWSmall:
                    return pwSmall;
                case Util.Operation.PWAR:
                    float A = pwBig / pwSmall - 1;
                    float r = info2.m_pavementWidth * info.m_halfWidth / info2.m_halfWidth;
                    return A * r + pwBig;
                default:
                    return width;
            } 
        }
#if DEBUG
        public class ARAsymTest : MonoBehaviour {
            static GameObject go_;
            static ARAsymTest instance_;
            public static ARAsymTest Instance {
                get {
                    if (!instance_) {
                        go_ = new GameObject(nameof(ARAsymTest));
                        instance_ = go_.AddComponent<ARAsymTest>();
                    }
                    return instance_;
                }
            }

            public bool Switch12;
            public Util.Operation Case1 = default;
            public Util.Operation Case2 = Util.Operation.PWAR;
            public Util.Operation Case3 = Util.Operation.PWSmall;
            public Util.Operation Case4 = default;
            public Util.Operation Case5 = Util.Operation.PWBig;
            public Util.Operation Case6 = Util.Operation.PWBig;

            public Util.Operation GetOperation(int occurance) {
                return occurance switch {
                    1 => Case1,
                    2 => Case2,
                    3 => Case3,
                    4 => Case4,
                    5 => Case5,
                    6 => Case6,
                    _ => default,
                };
            }

            bool updateNow_;
            public bool UpdateNow {
                get => updateNow_;
                set {
                    updateNow_ = value;
                    SimulationManager.instance.AddAction(RefreshImpl);
                }
            }

            void RefreshImpl() {
                for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                    if (!NetUtil.IsSegmentValid(segmentID)) continue;
                    if (!segmentID.ToSegment().Info.IsAdaptive()) continue;
                    NetManager.instance.UpdateSegment(segmentID);
                }
            }

            public static float ModifyPavementDebug(float width, ushort segmentID, ushort segmentID2, int occurance) {
                Log.Debug($"ModifyPavementDebug() called", false);
                if (ARAsymTest.Instance.Switch12) Helpers.Swap(ref segmentID, ref segmentID2);
                ref var segment = ref segmentID.ToSegment();
                NetInfo info = segment.Info;
                NetInfo info2 = segmentID2.ToSegment().Info;
                if (info.GetMetaData() is not NetInfoExtionsion.Net netData)
                    return width;

                float pwLeft = info.m_pavementWidth;
                float pwRight = netData.PavementWidthRight;
                float pwSmall = Mathf.Min(pwLeft, pwRight);
                float pwBig = Mathf.Max(pwLeft, pwRight);
                if (pwLeft >= pwRight)
                    return width;

                ushort nodeID = segment.GetSharedNode(segmentID2);
                bool startNode = segment.IsStartNode(nodeID);
                bool reverse = startNode ^ segment.IsInvert();

                Log.Debug($"XXX ModifyPavementRight: segmentID={segmentID} segmentID2={segmentID2}\n" +
                    $"pwBig={pwBig} pwSmall={pwSmall}\n" +
                    $"occurance={occurance} reverse={reverse} ");

                switch (ARAsymTest.Instance.GetOperation(occurance)) {
                    case Util.Operation.PWBig:
                        return pwBig;
                    case Util.Operation.PWSmall:
                        return pwSmall;
                    case Util.Operation.PWAR:
                        float A = pwBig / pwSmall - 1;
                        float r = info2.m_pavementWidth * info.m_halfWidth / info2.m_halfWidth;
                        return A * r + pwBig;
                    default:
                        return width;
                }
            }
        }
#endif
    }
}