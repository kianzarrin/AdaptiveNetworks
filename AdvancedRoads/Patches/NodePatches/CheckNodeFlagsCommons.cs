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
        public static bool CheckFlags(NetInfo.Node node, ushort nodeID, ref RenderManager.Instance data) {
            var nodeInfoExt = NetInfoExt.Node.Get(node as NetInfoExtension.Node);
            if (nodeInfoExt == null) return true;

            NetNodeExt netNodeExt = NetworkExtensionManager.Instance.NodeBuffer[nodeID];
            int segmentIndex = (ushort)(data.m_dataInt0 & 7);
            ushort segmentID = nodeID.ToNode().GetSegment(segmentIndex);
            NetSegmentExt netSegmentExt = NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            NetSegmentEnd netSegmentEnd = netSegmentExt.GetEnd(nodeID);

            return nodeInfoExt.CheckFlags(netNodeExt.m_flags, netSegmentEnd.m_flags);
        }

        static MethodInfo mCheckFlags2 => typeof(CheckNodeFlagsCommons).GetMethod("CheckFlags");
        static MethodInfo mCheckFlags => typeof(NetInfo.Node).GetMethod("CheckFlags");

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, int occurance, MethodInfo method) {
            // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: occurance);
            PatchCheckFlagsAt(codes, index, method);
        } // end method

        public static void PatchAllCheckFlags(List<CodeInstruction> codes, MethodInfo method) {
            CodeInstruction CallBaseCheckFlags = new CodeInstruction(OpCodes.Callvirt, mCheckFlags);
            for (int index=0;index< codes.Count; ++index) {
                if(IsSameInstruction(codes[index], CallBaseCheckFlags)) {
                    PatchCheckFlagsAt(codes, index, method);
                }
            }
        }

        public static void PatchCheckFlagsAt(List<CodeInstruction> codes, int index, MethodInfo method) {
            HelpersExtensions.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_NodeInfo = new CodeInstruction(codes[index - 2]); // TODO search 
            CodeInstruction LDArg_NodeID = GetLDArg(method, "nodeID");
            CodeInstruction LDArg_RenderData = GetLDArg(method, "data");

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_NodeInfo,
                    LDArg_NodeID,
                    LDArg_RenderData,
                    new CodeInstruction(OpCodes.Call, mCheckFlags2),
                    new CodeInstruction(OpCodes.And),
                };
                ReplaceInstructions(codes, newInstructions, index);
            } // end block
        }
    }
}
