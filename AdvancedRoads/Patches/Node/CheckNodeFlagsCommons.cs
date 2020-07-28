using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using PrefabIndeces;

namespace AdvancedRoads.Patches {
    using System;
    using UnityEngine;
    using KianCommons;
    using static KianCommons.Patches.TranspilerUtils;
    using AdvancedRoads.Manager;

    public static class CheckNodeFlagsCommons {
        public static bool CheckFlags(NetInfo.Node node, ushort nodeID, ushort segmentID) {
            var nodeInfoExt = NetInfoExt.Node.Get(node as NetInfoExtension.Node);
            if (nodeInfoExt == null) return true;
             NetNodeExt netNodeExt = NetworkExtensionManager.Instance.NodeBuffer[nodeID];
            NetSegmentExt netSegmentExt = NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            NetSegmentEnd netSegmentEnd = netSegmentExt.GetEnd(nodeID);

            return nodeInfoExt.CheckFlags(netNodeExt.m_flags, netSegmentEnd.m_flags);
        }

        static MethodInfo mCheckFlags2 => typeof(CheckNodeFlagsCommons).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags2 is null");
        static MethodInfo mCheckFlags => typeof(NetInfo.Node).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags is null");
        static MethodInfo mGetSegment => typeof(NetNode).GetMethod("GetSegment")
            ?? throw new Exception("mGetSegment is null");

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodInfo method, int occurance, int counterGetSegment) {
            // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: occurance);
            HelpersExtensions.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_NodeInfo = new CodeInstruction(codes[index - 2]); // TODO search 
            CodeInstruction LDArg_NodeID = GetLDArg(method, "nodeID");
            CodeInstruction LDLoc_segmentID = BuildSegnentLDLocFromPrevSTLoc(codes, index, counter: counterGetSegment);

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_NodeInfo,
                    LDArg_NodeID,
                    LDLoc_segmentID,
                    new CodeInstruction(OpCodes.Call, mCheckFlags2),
                    new CodeInstruction(OpCodes.And),
                };
                ReplaceInstructions(codes, newInstructions, index);
            } // end block
        }

        public static CodeInstruction BuildSegnentLDLocFromPrevSTLoc(List<CodeInstruction> codes, int index, int counter = 1) {
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Call, mGetSegment), index, counter: counter, dir: -1);
            var code = codes[index + 1];
            HelpersExtensions.Assert(IsStLoc(code), $"IsStLoc(code) | code={code}");
            return BuildLdLocFromStLoc(code);
        }
    }
}
