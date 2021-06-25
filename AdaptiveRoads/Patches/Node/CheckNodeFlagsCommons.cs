using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ColossalFramework;

namespace AdaptiveRoads.Patches.Node {
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System;
    using static KianCommons.Patches.TranspilerUtils;
    using KianCommons.Patches;

    public static class CheckNodeFlagsCommons {
        public static bool CheckFlags(NetInfo.Node node, ushort nodeID, ushort segmentID) {
            var nodeInfoExt = node?.GetMetaData();
            if (nodeInfoExt == null) return true;
            if (segmentID == 0) {
                if (nodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.End))
                    segmentID = NetUtil.GetFirstSegment(nodeID); // end node
                else
                    segmentID = GetBendDCSegmentID(nodeID);
            }

            ref NetSegment netSegment = ref segmentID.ToSegment();
            ref NetNodeExt netNodeExt = ref NetworkExtensionManager.Instance.NodeBuffer[nodeID];
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(nodeID);
            return nodeInfoExt.CheckFlags(
                netNodeExt.m_flags, netSegmentEnd.m_flags,
                netSegmentExt.m_flags, netSegment.m_flags);
        }

        public static ushort GetBendDCSegmentID(ushort nodeID) {
            // copy pasted from decompiler.
            ushort segmentID1 = 0;
            ushort segmentID2 = 0;
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
            return segmentID1;
        }

        static MethodInfo mCheckFlagsExt => typeof(CheckNodeFlagsCommons).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlagsExt is null");
        static MethodInfo mCheckFlags => typeof(NetInfo.Node).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags is null");
        static MethodInfo mGetSegment => typeof(NetNode).GetMethod("GetSegment")
            ?? throw new Exception("mGetSegment is null");

        /// <param name="counterGetSegment">
        /// if set to 0, segmentID is auto-calculated (only for end ndoes and DC bend nodes)
        /// if set to n > 0, will use the n-th previous call to GetSegment() to determine segmentID.
        /// </param>
        public static void PatchCheckFlags(
            List<CodeInstruction> codes, MethodBase method, int occuranceCheckFlags, int counterGetSegment) {
            // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
            var iCheckFlags = codes.Search(c => c.Calls(mCheckFlags), count: occuranceCheckFlags);
            Assertion.Assert(iCheckFlags > 0, "index>0");

            int iLdNodeInfo = codes.Search(
                _c => _c.IsLdLoc(typeof(NetInfo.Node), method),
                startIndex: iCheckFlags, count: -1);
            CodeInstruction ldNodeInfo = codes[iLdNodeInfo].Clone();
            CodeInstruction ldNodeID = GetLDArg(method, "nodeID");
            CodeInstruction ldSegmentID = BuildSegmentLDLocFromPrevSTLoc(codes, iCheckFlags, counterGetSegment);

            { // insert our checkflags after base checkflags
                var insertions = new[]{
                    ldNodeInfo,
                    ldNodeID,
                    ldSegmentID,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
                codes.InsertInstructions(iCheckFlags + 1, insertions);
            } // end block
        }

        public static CodeInstruction BuildSegmentLDLocFromPrevSTLoc(
            List<CodeInstruction> codes, int index, int counter = 1) {
            if (counter == 0)
                return new CodeInstruction(OpCodes.Ldc_I4_0); // load 0u

            index = codes.Search(c => c.Calls(mGetSegment), startIndex: index, count: counter * -1);
            index = codes.Search(c => c.IsStloc(), startIndex: index);
            return codes[index].BuildLdLocFromStLoc();
        }

        // TODO delete
        //#region for Populate/CalculateGroupData DC Bend:

        //public static void PatchCheckFlagsAlt(
        //    List<CodeInstruction> codes, MethodBase method, int occurance) {
        //    // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
        //    var iCheckFlags = codes.Search(c => c.Calls(mCheckFlags), count: occurance);
        //    int InsertionIndex = iCheckFlags + 1; // populate group data
        //    if(method.Name == "CalculateGroupData") {
        //        // segments are calcualted after check flags so we neeed to move our check flags to a later point.

        //    }

        //    Assertion.Assert(iCheckFlags > 0, "index>0");

        //    int iLdNodeInfo = codes.Search(
        //        _c => _c.IsLdLoc(typeof(NetInfo.Node)),
        //        startIndex: iCheckFlags, count: -1);
        //    CodeInstruction ldNodeInfo = codes[iLdNodeInfo].Clone();
        //    CodeInstruction ldNodeID = GetLDArg(method, "nodeID");


        //    CodeInstruction ldSegmentID = BuildSegmentLDLocFromPrevSTLocAlt(codes, iCheckFlags);


        //    { // insert our checkflags after base checkflags
        //        var insertions = new[]{
        //            ldNodeInfo,
        //            ldNodeID,
        //            ldSegmentID,
        //            new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
        //            new CodeInstruction(OpCodes.And),
        //        };
        //        codes.InsertInstructions(iCheckFlags + 1, insertions);
        //    } // end block
        //}

        //// for Populate/CalculateGroupData DC Bend:
        //public static CodeInstruction BuildSegmentLDLocFromPrevSTLocAlt(
        //    List<CodeInstruction> codes, int index) {
        //    // notes:
        //    //  - bend node has only two segments
        //    //  - in this situation there is only one line that calls GetSegment()
        //    //  - the segID = this.GetSegment() is called in a loop
        //    //  - segID is then assigned to segmentID1 or segmentID2.
        //    //    in two different lines
        //    //  - segID is loaded in multiple places but only in two places
        //    //    it is stored afterwards.

        //    // Strategy:
        //    //  step1- search for segID = this.GetSegment() backward
        //    //  step2- search for the next place where segID is loaded and immidately stored afterwards:
        //    //         segmentID1 = segID
        //    //  step3- converted stloc to ldloc
            

        //    // step1: segID = this.GetSegment()
        //    int iGetSegment = codes.Search(c => c.Calls(mGetSegment), startIndex: index, count: -1);
        //    int iStSegID = codes.Search(c => c.IsStLoc(typeof(ushort)), startIndex: iGetSegment);
        //    codes[iStSegID].IsStLoc(out int locSegID);

        //    // step2: segmentID1 = segID
        //    int iSegmentID = codes.Search( (int i) =>
        //       codes[i].IsStLoc(typeof(ushort)) &&
        //       codes[i - 1].IsLdLoc(locSegID));

        //    // step3: stloc segmentID1
        //    return codes[iSegmentID].BuildLdLocFromStLoc();
        //}

        //// only for CalculateGroupData DC Bend:
        //static int GetInsertionIndexAlt(List<CodeInstruction> codes, int index) {

        //    return 0;
        //}
        //#endregion
    }
}
