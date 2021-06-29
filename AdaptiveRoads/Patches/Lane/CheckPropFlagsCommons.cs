using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;

/* TODO use this to pass __state from prefix to transpiler or initialize it on first use:
see : https://github.com/boformer/NetworkSkins2/blob/0d165621204f77a0183f2ef769914d96e3dbbac5/NetworkSkins/Patches/NetNode/NetNodeTerrainUpdatedPatch.cs

var customLanesLocalVar = il.DeclareLocal(typeof(NetInfo.Lane[])); // variable type
customLanesLocalVar.SetLocalSymInfo("customLanes"); // variable name


var beginLabel = il.DefineLabel();
codes[index].labels.Add(beginLabel);
*/

namespace AdaptiveRoads.Patches.Lane {
    using System;
    using KianCommons;
    using static KianCommons.Patches.TranspilerUtils;
    using AdaptiveRoads.Manager;
    using ColossalFramework;

    public static class CheckPropFlagsCommons {
        //public class StateT {
        //    public NetLaneExt.Flags laneFlags;
        //    public NetSegmentExt.Flags segmentFlags;
        //    public NetSegment.Flags vanillaSegmentFlags;
        //    public NetNodeExt.Flags startNodeFlags, endNodeFlags;
        //    public NetSegmentEnd.Flags segmentStartFags, segmentEndFlags;
        //}

        //public static StateT InitState(NetInfo.Lane laneInfo, uint laneID) {
        //    ushort segmentID = laneID.ToLane().m_segment;
        //    ref NetSegment segment = ref segmentID.ToSegment();

        //    bool segmentInverted = segment.m_flags.IsFlagSet(NetSegment.Flags.Invert);
        //    bool backward = (laneInfo.m_finalDirection & NetInfo.Direction.Both) == NetInfo.Direction.Backward ||
        //        (laneInfo.m_finalDirection & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidForward;
        //    bool reverse = segmentInverted != backward; // xor

        //    ushort startNodeID = reverse ? segment.m_startNode : segment.m_endNode; // tail
        //    ushort endNodeID = !reverse ? segment.m_startNode : segment.m_endNode; // head

        //    ref NetLaneExt netLaneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
        //    ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
        //    ref NetNodeExt netNodeExtStart = ref NetworkExtensionManager.Instance.NodeBuffer[startNodeID];
        //    ref NetNodeExt netNodeExtEnd = ref NetworkExtensionManager.Instance.NodeBuffer[endNodeID];
        //    ref NetSegmentEnd netSegmentStart = ref netSegmentExt.GetEnd(startNodeID);
        //    ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(endNodeID);

        //    return new StateT {
        //        laneFlags = netLaneExt.m_flags, segmentFlags = netSegmentExt.m_flags, vanillaSegmentFlags = segment.m_flags,
        //        startNodeFlags = netNodeExtStart.m_flags, endNodeFlags =netNodeExtEnd.m_flags,
        //        segmentStartFags = netSegmentStart.m_flags, segmentEndFlags = netSegmentEnd.m_flags,
        //    };
        //}

        //public static bool CheckFlags2(NetLaneProps.Prop prop, NetInfo.Lane laneInfo, uint laneID, ref StateT state) {
        //    var propInfoExt = prop?.GetExt();
        //    if (propInfoExt == null)
        //        return true;
        //    if (state == null)
        //        state = InitState(laneInfo, laneID);

        //    return propInfoExt.CheckFlags(
        //        state.laneFlags, state.segmentFlags, state.vanillaSegmentFlags,
        //        state.startNodeFlags, state.endNodeFlags,
        //        state.segmentStartFags, state.segmentEndFlags);
        //}

        // TODO use the other checkflags.
        public static bool CheckFlags(NetLaneProps.Prop prop, NetInfo.Lane laneInfo, uint laneID) {
            var propInfoExt = prop?.GetMetaData();
            //var propIndex = prop as NetInfoExtension.Lane.Prop;
            //Log.DebugWait($"CheckFlags called for lane:{laneID} propInfoExt={propInfoExt} propIndex={propIndex} prop={prop}", (int)laneID);
            if (propInfoExt == null) return true;
            //var laneInfoExt = laneInfo?.GetExt();
            //if (laneInfoExt == null) return true;

            // TODO prepare data at the begining.
            ushort segmentID = laneID.ToLane().m_segment;
            ref NetSegment segment = ref segmentID.ToSegment();
            ref NetLane netLane= ref laneID.ToLane();

            bool reverse = segment.IsInvert() == laneInfo.IsGoingBackward(); // xor

            ushort startNodeID =  !reverse ? segment.m_startNode : segment.m_endNode; // tail
            ushort endNodeID = reverse ? segment.m_startNode : segment.m_endNode; // head

            ref NetLaneExt netLaneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            ref NetNodeExt netNodeExtStart = ref NetworkExtensionManager.Instance.NodeBuffer[startNodeID];
            ref NetNodeExt netNodeExtEnd = ref NetworkExtensionManager.Instance.NodeBuffer[endNodeID];
            ref NetSegmentEnd netSegmentStart = ref netSegmentExt.GetEnd(startNodeID);
            ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(endNodeID);

            //if (propIndex.LaneIndex == 1 && propIndex.Index == 1)         
            //{
            //    //Log.DebugWait($"calling propInfoExt.CheckFlags called for lane{laneID} " +
            //    //    $"netSegmentStart.m_flags={netSegmentStart.m_flags}, netSegmentEnd.m_flags={netSegmentEnd.m_flags} " +
            //    //    $"propInfoExt.SegmentEndFlags.Required={propInfoExt.SegmentEndFlags.Required}",
            //    //    id: (int)laneID, copyToGameLog: false);
            //}
            //Log.DebugWait($"[speed={netLaneExt.SpeedLimit} laneExt={netLaneExt}");

            return propInfoExt.Check(
                netLaneExt.m_flags, netSegmentExt.m_flags, segment.m_flags,
                netNodeExtStart.m_flags, netNodeExtEnd.m_flags,
                netSegmentStart.m_flags, netSegmentEnd.m_flags,
                laneSpeed: netLaneExt.SpeedLimit,
                forwardSpeedLimit: netSegmentExt.ForwardSpeedLimit,
                backwardSpeedLimit: netSegmentExt.BackwardSpeedLimit,
                netSegmentExt.Curve, netLane.m_curve);
        }

        static MethodInfo mCheckFlagsExt =>
            typeof(CheckPropFlagsCommons)
            .GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlagsExt is null");

        static MethodInfo mCheckFlags =>
            typeof(NetLaneProps.Prop)
            .GetMethod(nameof(NetLaneProps.Prop.CheckFlags))
            ?? throw new Exception("mCheckFlags is null");

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodBase method) {
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: 1);
            Assertion.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_prop = Build_LDLoc_PropInfo_FromSTLoc(codes, index);
            CodeInstruction LDArg_laneInfo = GetLDArg(method, "laneInfo");
            CodeInstruction LDArg_laneID = GetLDArg(method, "laneID");

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_prop,
                    LDArg_laneInfo,
                    LDArg_laneID,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
                InsertInstructions(codes, newInstructions, index + 1);
            } // end block
        }

        static FieldInfo fProps =>
            typeof(NetLaneProps).
            GetField(nameof(NetLaneProps.m_props))
            ?? throw new Exception("fProps is null");

        public static CodeInstruction Build_LDLoc_PropInfo_FromSTLoc(List<CodeInstruction> codes, int index, int counter = 1, int dir = -1) {
            /* IL_008f: ldloc.0      // laneProps
             * IL_0090: ldfld        class NetLaneProps/Prop[] NetLaneProps::m_props <- find this
             * IL_0095: ldloc.s      index1
             * IL_0097: ldelem.ref
             * IL_0098: stloc.s      prop  <- seek to this -- build ldloc from this */
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Ldfld, fProps), index, counter: counter, dir: dir);
            index = SearchGeneric(codes, i => codes[i].IsStloc(), index, counter: 1, dir: 1);
            return codes[index].BuildLdLocFromStLoc();
        }
    }
}
