namespace AdaptiveRoads.Patches.Node {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using ColossalFramework;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System;
    using static KianCommons.Patches.TranspilerUtils;
    using KianCommons.Patches;
    using AdaptiveRoads.Data.NetworkExtensions;

    public static class CheckNodeFlagsCommons {
        public static NetNode.Flags AddDCFlags(NetNode.Flags flags, ushort nodeID, ushort segmentID, ushort segmentID2) {
            bool bend = segmentID == 0;
            if (!bend) {
                // optimisation: bend node already has these flags.
                flags |= NetNodeExt.CalculateDCAsymFlags(nodeID, segmentID, segmentID2);
            }
            return flags;
        }
        //public static NetNode.Flags AddDCFlags(NetNode.Flags flags) => flags;

        public static bool CheckFlagsDC(NetInfo.Node node, ushort nodeID, ushort segmentID, ushort segmentID2) {
            var nodeInfoExt = node?.GetMetaData();
            if (nodeInfoExt == null) return true;
            bool bend = segmentID == 0;
            if (bend)
                GetBendDCSegmentID(nodeID, out segmentID, out segmentID2);

            bool ret = CheckFlagsImpl(nodeInfoExt, nodeID, segmentID);
            if (nodeInfoExt.CheckTargetFlags)
                ret = ret && CheckFlagsImpl(nodeInfoExt, nodeID, segmentID2);
            return ret;
        }

        public static bool CheckFlags(NetInfo.Node node, ushort nodeID, ushort segmentID) {
            var nodeInfoExt = node?.GetMetaData();
            if (nodeInfoExt == null) return true;
            if (segmentID == 0) {
                if (nodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.End))
                    segmentID = NetUtil.GetFirstSegment(nodeID); // end node
                else
                    GetBendDCSegmentID(nodeID, out segmentID, out _);
            }

            return CheckFlagsImpl(nodeInfoExt, nodeID, segmentID);
        }

        static bool CheckFlagsImpl(NetInfoExtionsion.Node node, ushort nodeID, ushort segmentID) {
            ref NetSegment netSegment = ref segmentID.ToSegment();
            ref NetNodeExt netNodeExt = ref NetworkExtensionManager.Instance.NodeBuffer[nodeID];
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(nodeID);
            return node.CheckFlags(
                netNodeExt.m_flags, netSegmentEnd.m_flags,
                netSegmentExt.m_flags, netSegment.m_flags,
                netSegmentExt.UserData);
        }

        static void GetBendDCSegmentID(ushort nodeID, out ushort segmentID1, out ushort segmentID2) {
            // copy pasted from decompiler.
            segmentID1 = 0;
            segmentID2 = 0;
            bool primarySegmentDetected = false;
            int counter = 0;
            for (int segmentIndex = 0; segmentIndex < 8; segmentIndex++) {
                ushort segmentID = nodeID.ToNode().GetSegment(segmentIndex);
                if (segmentID != 0) {
                    bool firstLoop = ++counter == 1;
                    bool startNode = segmentID.ToSegment().IsStartNode(nodeID);
                    if ((!firstLoop && !primarySegmentDetected) || (firstLoop && !startNode)) {
                        primarySegmentDetected = true;
                        segmentID1 = segmentID;
                    } else {
                        segmentID2 = segmentID;
                    }
                }
            }
        }

        static MethodInfo mAddDCFlags => typeof(CheckNodeFlagsCommons).GetMethod(nameof(AddDCFlags), throwOnError: true);
        static MethodInfo mCheckFlagsExt => typeof(CheckNodeFlagsCommons).GetMethod(nameof(CheckFlags), throwOnError: true);
        static MethodInfo mCheckFlagsExtDC => typeof(CheckNodeFlagsCommons).GetMethod(nameof(CheckFlagsDC), throwOnError: true);
        static MethodInfo mCheckFlags => typeof(NetInfo.Node).GetMethod("CheckFlags", throwOnError: true);
        static MethodInfo mGetSegment => typeof(NetNode).GetMethod("GetSegment", throwOnError: true);

        /// <param name="counterGetSegment">
        /// if set to 0, segmentID is auto-calculated (only for end ndoes and DC bend nodes)
        /// if set to n > 0, will use the n-th previous call to GetSegment() to determine segmentID.
        /// </param>
        public static void PatchCheckFlags(
            List<CodeInstruction> codes, MethodBase method, int occuranceCheckFlags, int counterGetSegment, bool DC = false) {
            // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
            var iCheckFlags = codes.Search(c => c.Calls(mCheckFlags), count: occuranceCheckFlags);
            Assertion.Assert(iCheckFlags > 0, "index>0");

            int iLdNodeInfo = codes.Search(
                _c => _c.IsLdLoc(typeof(NetInfo.Node), method),
                startIndex: iCheckFlags, count: -1);
            CodeInstruction ldNodeInfo = codes[iLdNodeInfo].Clone();
            CodeInstruction ldNodeID = GetLDArg(method, "nodeID");
            CodeInstruction ldSegmentID = BuildSegmentLDLocFromPrevSTLoc(codes, iCheckFlags, counterGetSegment);

            CodeInstruction ldSegmentID2 = null;
            CodeInstruction[] newCodes2;
            if (DC) {
                ldSegmentID2 = BuildSegmentLDLocFromPrevSTLoc(codes, iCheckFlags, counterGetSegment - 1);

                newCodes2 = new[]{
                    ldNodeInfo,
                    ldNodeID,
                    ldSegmentID,
                    ldSegmentID2,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExtDC),
                    new CodeInstruction(OpCodes.And),
                };
            } else {
                newCodes2 = new[]{
                    ldNodeInfo,
                    ldNodeID,
                    ldSegmentID,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
            }
            codes.InsertInstructions(iCheckFlags + 1, newCodes2);// insert our checkflags after base checkflags


            if (DC) {
                // add these instructions last so that we don't change iCheckFlags.
                // call NetNode.Flags AddDCFlags(NetNode.Flags flags, ushort nodeID, ushort segmentID, ushort segmentID2)
                codes.InsertInstructions(iCheckFlags, new[]{
                    // flags already in stack
                    ldNodeID,
                    ldSegmentID,
                    ldSegmentID2,
                    new CodeInstruction(OpCodes.Call, mAddDCFlags),
                });
            }
        }

        public static CodeInstruction BuildSegmentLDLocFromPrevSTLoc(
            List<CodeInstruction> codes, int index, int counter = 1) {
            if (counter <= 0)
                return new CodeInstruction(OpCodes.Ldc_I4_0); // load 0u (calculate in the injection)

            index = codes.Search(c => c.Calls(mGetSegment), startIndex: index, count: counter * -1);
            index = codes.Search(c => c.IsStloc(), startIndex: index);
            return codes[index].BuildLdLocFromStLoc();
        }
    }
}
