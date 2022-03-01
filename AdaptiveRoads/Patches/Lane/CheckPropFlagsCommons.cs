using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using AdaptiveRoads.Data.NetworkExtensions;
using System.Diagnostics;

/* TODO use this to pass __state from prefix to transpiler or initialize it on first use:
see : https://github.com/boformer/NetworkSkins2/blob/0d165621204f77a0183f2ef769914d96e3dbbac5/NetworkSkins/Patches/NetNode/NetNodeTerrainUpdatedPatch.cs

var customLanesLocalVar = il.DeclareLocal(typeof(NetInfo.Lane[])); // variable type
customLanesLocalVar.SetLocalSymInfo("customLanes"); // variable name
*/

namespace AdaptiveRoads.Patches.Lane {
    using System;
    using KianCommons;
    using static KianCommons.Patches.TranspilerUtils;
    using AdaptiveRoads.Manager;
    using ColossalFramework;

    public static class CheckPropFlagsCommons {
        public struct StateT {
            public bool Initialized;

            public NetLaneExt.Flags laneFlags;
            public NetSegmentExt.Flags segmentFlags;
            public NetSegment.Flags vanillaSegmentFlags;
            public NetNodeExt.Flags startNodeFlags, endNodeFlags;
            public NetSegmentEnd.Flags segmentStartFags, segmentEndFlags;

            public float laneSpeed, forwardSpeed, backwardSpeed;
            public float SegmentCurve, laneCurve;

            public void Init(NetInfo.Lane laneInfo, uint laneID) {
                ushort segmentID = laneID.ToLane().m_segment;
                ref NetSegment segment = ref segmentID.ToSegment();
                ref NetLane netLane = ref laneID.ToLane();

                bool reverse = segment.IsInvert() != laneInfo.IsGoingBackward(); // xor

                ushort startNodeID = !reverse ? segment.m_startNode : segment.m_endNode; // tail
                ushort endNodeID = reverse ? segment.m_startNode : segment.m_endNode; // head

                ref NetLaneExt netLaneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
                ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
                ref NetNodeExt netNodeExtStart = ref NetworkExtensionManager.Instance.NodeBuffer[startNodeID];
                ref NetNodeExt netNodeExtEnd = ref NetworkExtensionManager.Instance.NodeBuffer[endNodeID];
                ref NetSegmentEnd netSegmentStart = ref netSegmentExt.GetEnd(startNodeID);
                ref NetSegmentEnd netSegmentEnd = ref netSegmentExt.GetEnd(endNodeID);

                laneFlags = netLaneExt.m_flags; segmentFlags = netSegmentExt.m_flags; vanillaSegmentFlags = segment.m_flags;
                startNodeFlags = netNodeExtStart.m_flags; endNodeFlags = netNodeExtEnd.m_flags;
                segmentStartFags = netSegmentStart.m_flags; segmentEndFlags = netSegmentEnd.m_flags;
                laneSpeed = netLaneExt.SpeedLimit;
                forwardSpeed = netSegmentExt.ForwardSpeedLimit;
                backwardSpeed = netSegmentExt.BackwardSpeedLimit;
                SegmentCurve = netSegmentExt.Curve; laneCurve = netLane.m_curve;

                Initialized = true;
            }
        }

#if DEBUG
        public static Stopwatch timer = new Stopwatch();
        public static Stopwatch timer2 = new Stopwatch();
#endif

        public static bool CheckFlags(NetLaneProps.Prop prop, NetInfo.Lane laneInfo, uint laneID, ref StateT state) {
#if DEBUG
            timer.Start();
#endif
            try {
                timer2.Start();
                var propInfoExt = prop?.GetMetaData();
                // 3% FPS boost while sacrificing future proofing.
                // var propInfoExt = (prop as PrefabMetadata.API.IInfoExtended<NetInfoExtionsion.LaneProp>)?.MetaData[0] as NetInfoExtionsion.LaneProp;
                timer2.Stop();
                if (propInfoExt == null) return true;

                if (!state.Initialized) state.Init(laneInfo, laneID);

                return propInfoExt.Check(
                    state.laneFlags, state.segmentFlags, state.vanillaSegmentFlags,
                    state.startNodeFlags, state.endNodeFlags,
                    state.segmentStartFags, state.segmentEndFlags,
                    laneSpeed: state.laneSpeed,
                    forwardSpeedLimit: state.forwardSpeed,
                    backwardSpeedLimit: state.backwardSpeed,
                    segmentCurve: state.SegmentCurve, laneCurve: state.laneCurve);
            } finally {
#if DEBUG
                timer.Stop();
#endif
            }
        }

        static MethodInfo mCheckFlagsExt =>
            typeof(CheckPropFlagsCommons).GetMethod(nameof(CheckFlags), throwOnError: true);

        static MethodInfo mCheckFlags =>
            typeof(NetLaneProps.Prop)
            .GetMethod(nameof(NetLaneProps.Prop.CheckFlags))
            ?? throw new Exception("mCheckFlags is null");

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodBase method, ILGenerator il) {
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: 1);
            Assertion.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_prop = Build_LDLoc_PropInfo_FromSTLoc(codes, index);
            CodeInstruction LDArg_laneInfo = GetLDArg(method, "laneInfo");
            CodeInstruction LDArg_laneID = GetLDArg(method, "laneID");

            var stateVar = il.DeclareLocal(typeof(StateT)); // variable type
            stateVar.SetLocalSymInfo("laneStateCache"); // variable name
            var LoadRefState = new CodeInstruction(OpCodes.Ldloca, stateVar.LocalIndex);

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_prop,
                    LDArg_laneInfo,
                    LDArg_laneID,
                    LoadRefState,
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
