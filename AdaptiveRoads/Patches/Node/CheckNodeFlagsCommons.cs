using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using PrefabIndeces;

namespace AdaptiveRoads.Patches.Node {
    using System;
    using KianCommons;
    using static KianCommons.Patches.TranspilerUtils;
    using AdaptiveRoads.Manager;
    using TrafficManager.RedirectionFramework;
    using UnityEngine.Assertions;

    public static class CheckNodeFlagsCommons {
        public static bool CheckFlags(NetInfo.Node node, ushort nodeID, ushort segmentID) {
            var nodeInfoExt = node?.GetMetaData();
            if (nodeInfoExt == null) return true;
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
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodInfo method, int occurance, int counterGetSegment) {
            // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: occurance);
            Assertion.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_NodeInfo = Build_LDLoc_NodeInfo_FromSTLoc(codes, index);
            CodeInstruction LDArg_NodeID = GetLDArg(method, "nodeID");
            CodeInstruction LDLoc_segmentID = BuildSegnentLDLocFromPrevSTLoc(codes, index, counter: counterGetSegment);

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_NodeInfo,
                    LDArg_NodeID,
                    LDLoc_segmentID,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
                InsertInstructions(codes, newInstructions, index + 1);
            } // end block
        }

        static FieldInfo fNodes => typeof(NetInfo).GetField("m_nodes") ?? throw new Exception("fNodes is null");

        public static CodeInstruction Build_LDLoc_NodeInfo_FromSTLoc(List<CodeInstruction> codes, int index, int counter=1, int dir=-1) {
           /* IL_155f: ldloc.s      info2_V_80
            * IL_1561: ldfld        class NetInfo/Node[] NetInfo::m_nodes  <- step 1: find this
            * IL_1566: ldloc.s      index2_V_147
            * IL_1568: ldelem.ref
            * IL_1569: stloc.s      node_V_148  <- step 2: find this and convert it to ldloc.
            */
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Ldfld, fNodes), index, counter: counter, dir: dir);
            index = SearchGeneric(codes, i => codes[i].IsStloc(), index, counter: 1, dir: 1);
            return BuildLdLocFromStLoc(codes[index]);
        }

        public static CodeInstruction BuildSegnentLDLocFromPrevSTLoc(List<CodeInstruction> codes, int index, int counter = 1) {
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Call, mGetSegment), index, counter: counter, dir: -1);
            index = SearchGeneric(codes, i => codes[i].IsStloc(), index, counter: 1, dir: 1);
            return BuildLdLocFromStLoc(codes[index]);
        }
    }
}
