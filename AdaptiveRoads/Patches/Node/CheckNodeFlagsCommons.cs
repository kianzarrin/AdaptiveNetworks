using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

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
            if (segmentID == 0) segmentID = NetUtil.GetFirstSegment(nodeID); // end node
            ref NetSegment netSegment = ref segmentID.ToSegment();
            ref NetNodeExt netNodeExt = ref NetworkExtensionManager.Instance.NodeBuffer[nodeID];
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(nodeID);
            return nodeInfoExt.CheckFlags(
                netNodeExt.m_flags, netSegmentEnd.m_flags,
                netSegmentExt.m_flags, netSegment.m_flags);
        }

        static MethodInfo mCheckFlagsExt => typeof(CheckNodeFlagsCommons).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlagsExt is null");
        static MethodInfo mCheckFlags => typeof(NetInfo.Node).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags is null");
        static MethodInfo mGetSegment => typeof(NetNode).GetMethod("GetSegment")
            ?? throw new Exception("mGetSegment is null");

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(
            List<CodeInstruction> codes, MethodBase method, int occurance, int counterGetSegment) {
            // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
            var index = codes.Search(c => c.Calls(mCheckFlags), count: occurance);
            Assertion.Assert(index > 0, "index>0");

            CodeInstruction LDLoc_NodeInfo = Build_LDLoc_NodeInfo_FromSTLoc(codes, index);
            CodeInstruction LDArg_NodeID = GetLDArg(method, "nodeID");
            //CodeInstruction ldarga_Data = new CodeInstruction(OpCodes.Ldarga_S, GetArgLoc(method, "data"));
            CodeInstruction LDArg_SegmentID = BuildSegnentLDLocFromPrevSTLoc(codes, index, counterGetSegment);


            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_NodeInfo,
                    LDArg_NodeID,
                    LDArg_SegmentID,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
                codes.InsertInstructions(index + 1, newInstructions);
            } // end block
        }

        static FieldInfo fNodes => typeof(NetInfo).GetField("m_nodes") ?? throw new Exception("fNodes is null");

        public static CodeInstruction Build_LDLoc_NodeInfo_FromSTLoc(
            List<CodeInstruction> codes, int index, int counter = 1, int dir = -1) {
            /* IL_155f: ldloc.s      info2_V_80
             * IL_1561: ldfld        class NetInfo/Node[] NetInfo::m_nodes  <- step 1: find this
             * IL_1566: ldloc.s      index2_V_147
             * IL_1568: ldelem.ref
             * IL_1569: stloc.s      node_V_148  <- step 2: find this and convert it to ldloc.
             */
            index = codes.Search(c => c.LoadsField(fNodes), startIndex: index, count: counter * dir);
            index = codes.Search(c => c.IsStloc(), startIndex: index);
            return codes[index].BuildLdLocFromStLoc();
        }

        public static CodeInstruction BuildSegnentLDLocFromPrevSTLoc(
            List<CodeInstruction> codes, int index, int counter = 1) {
            if (counter == 0)
                return new CodeInstruction(OpCodes.Ldc_I4_0); // load 0u

            index = codes.Search(c => c.Calls(mGetSegment), startIndex: index, count: counter * -1);
            index = codes.Search(c => c.IsStloc(), startIndex: index);
            return codes[index].BuildLdLocFromStLoc();
        }
    }
}
