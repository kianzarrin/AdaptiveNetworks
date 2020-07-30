using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using PrefabIndeces;

/* TODO use this to pass __state from prefix to transpiler or initialize it on first use:
see : https://github.com/boformer/NetworkSkins2/blob/0d165621204f77a0183f2ef769914d96e3dbbac5/NetworkSkins/Patches/NetNode/NetNodeTerrainUpdatedPatch.cs

var customLanesLocalVar = il.DeclareLocal(typeof(NetInfo.Lane[])); // variable type
customLanesLocalVar.SetLocalSymInfo("customLanes"); // variable name


var beginLabel = il.DefineLabel();
codes[index].labels.Add(beginLabel);
*/

namespace AdvancedRoads.Patches.Lane {
    using System;
    using KianCommons;
    using static KianCommons.Patches.TranspilerUtils;
    using AdvancedRoads.Manager;
    using ColossalFramework;

    public static class CheckPropFlagsCommons {
        public class StateT {
            public NetInfoExt.LaneProp propInfoExt;
            public NetLaneExt.Flags laneFlags;
            public NetSegmentExt.Flags segmentFlags;
            public NetNodeExt.Flags startNodeFlags, endNodeFlags;
            public NetSegmentEnd.Flags segmentStartFags, segmentEndFlags;
        }


        public static StateT InitState(NetLaneProps.Prop prop, NetInfo.Lane laneInfo, uint laneID, ushort segmentID) {
            var propInfoExt = NetInfoExt.LaneProp.Get(prop as NetInfoExtension.Lane.Prop);
            if (propInfoExt == null)
                return default;
            bool segmentInverted = segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
            bool backward = (laneInfo.m_finalDirection & NetInfo.Direction.Both) == NetInfo.Direction.Backward ||
                (laneInfo.m_finalDirection & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidForward;
            bool reverse = segmentInverted != backward; // xor

            ushort startNodeID = reverse ? segmentID.ToSegment().m_startNode : segmentID.ToSegment().m_endNode; // tail
            ushort endNodeID = !reverse ? segmentID.ToSegment().m_startNode : segmentID.ToSegment().m_endNode; // head

            ref NetLaneExt netLaneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            ref NetNodeExt netNodeExtStart = ref NetworkExtensionManager.Instance.NodeBuffer[startNodeID];
            ref NetNodeExt netNodeExtEnd = ref NetworkExtensionManager.Instance.NodeBuffer[endNodeID];
            ref NetSegmentEnd netSegmentStart = ref netSegmentExt.GetEnd(startNodeID);
            ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(endNodeID);

            return new StateT {
                propInfoExt = propInfoExt,
                laneFlags = netLaneExt.m_flags, segmentFlags = netSegmentExt.m_flags,
                startNodeFlags = netNodeExtStart.m_flags, endNodeFlags =netNodeExtEnd.m_flags,
                segmentStartFags = netSegmentStart.m_flags, segmentEndFlags = netSegmentEnd.m_flags,
            };
        }

        public static bool CheckFlags2(NetLaneProps.Prop prop, NetInfo.Lane laneInfo, uint laneID, ushort segmentID, ref StateT state) {
            if (state == null)
                state = InitState(prop, laneInfo, laneID, segmentID);
            if (state.propInfoExt == null)
                return true;

            return state.propInfoExt.CheckFlags(
                state.laneFlags, state.segmentFlags,
                state.startNodeFlags, state.endNodeFlags,
                state.segmentStartFags, state.segmentEndFlags);
        }

        // TODO use the other checkflags.
        public static bool CheckFlags(NetLaneProps.Prop prop, NetInfo.Lane laneInfo, uint laneID, ushort segmentID) {
            var propInfoExt = NetInfoExt.LaneProp.Get(prop as NetInfoExtension.Lane.Prop);
            if (propInfoExt == null) return true;
            //var laneInfoExt = NetInfoExt.Lane.Get(laneInfo as NetInfoExtension.Lane);
            //if (laneInfoExt == null) return true;

            // TODO move prepration to prefix ... how can I read from state?
            bool segmentInverted = segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
            bool backward = (laneInfo.m_finalDirection & NetInfo.Direction.Both) == NetInfo.Direction.Backward ||
                (laneInfo.m_finalDirection & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidForward;
            bool reverse = segmentInverted != backward; // xor

            ushort startNodeID =  reverse ? segmentID.ToSegment().m_startNode : segmentID.ToSegment().m_endNode; // tail
            ushort endNodeID = !reverse ? segmentID.ToSegment().m_startNode : segmentID.ToSegment().m_endNode; // head

            ref NetLaneExt netLaneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            ref NetNodeExt netNodeExtStart = ref NetworkExtensionManager.Instance.NodeBuffer[startNodeID];
            ref NetNodeExt netNodeExtEnd = ref NetworkExtensionManager.Instance.NodeBuffer[endNodeID];
            ref NetSegmentEnd netSegmentStart = ref netSegmentExt.GetEnd(startNodeID);
            ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(endNodeID);

            return propInfoExt.CheckFlags(
                netLaneExt.m_flags, netSegmentExt.m_flags,
                netNodeExtStart.m_flags, netNodeExtEnd.m_flags,
                netSegmentStart.m_flags, netSegmentEnd.m_flags);
        }

        static MethodInfo mCheckFlagsExt => typeof(CheckPropFlagsCommons).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlagsExt is null");
        static MethodInfo mCheckFlags => typeof(NetInfo.Node).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags is null");


        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodInfo method) {
            // callvirt instance bool NetInfo/Node::CheckFlags(Flags)
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: occurance);
            HelpersExtensions.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_prop = new CodeInstruction(codes[index - 5]); // TODO search 
            CodeInstruction LDArg_laneInfo = GetLDArg(method, "laneInfo");
            CodeInstruction LDArg_segmentID = GetLDArg(method, "segmentID");
            CodeInstruction LDArg_laneID = GetLDArg(method, "laneID");

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_prop,
                    LDArg_laneInfo,
                    LDArg_segmentID,
                    LDArg_laneID,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
                InsertInstructions(codes, newInstructions, index + 1);
            } // end block
        }
    }
}
