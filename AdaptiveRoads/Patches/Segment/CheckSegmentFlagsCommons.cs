using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AdaptiveRoads.Patches.Segment {
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System;
    using static KianCommons.Patches.TranspilerUtils;

    public static class CheckSegmentFlagsCommons {
        public static bool CheckFlags(NetInfo.Segment segmentInfo, ushort segmentID, ref bool turnAround) {
            var segmentInfoExt = segmentInfo?.GetMetaData();
            if (segmentInfoExt == null) return true; // bypass

            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];
            ref NetSegment netSegment = ref segmentID.ToSegment();
            ref NetNode netNodeStart = ref netSegment.m_startNode.ToNode();
            ref NetNode netNodeEnd = ref netSegment.m_endNode.ToNode();

            var segmentTailFlags = netSegmentExt.Start.m_flags; 
            var segmentHeadFlags = netSegmentExt.End.m_flags; 
            var nodeTailFlags = netNodeStart.m_flags; 
            var nodeHeadFlags = netNodeEnd.m_flags; 

            bool reverse = /*netSegment.IsInvert() ^*/ NetUtil.LHT;
            if (reverse) {
                Helpers.Swap(ref segmentTailFlags, ref segmentHeadFlags);
                Helpers.Swap(ref nodeTailFlags, ref nodeHeadFlags);
                Log.DebugWait($"CheckSegmentFlagsCommons: segment:{segmentID} is reverse");
            }

            {
                turnAround = false;
                bool ret = segmentInfo.CheckFlags(netSegment.m_flags, turnAround);
                ret = ret && segmentInfoExt.CheckFlags(
                    netSegmentExt.m_flags,
                    tailFlags: segmentTailFlags,
                    headFlags:segmentHeadFlags,
                    tailNodeFlags: nodeTailFlags,
                    headNodeFlags: nodeHeadFlags,
                    turnAround);
                if (ret) return true;
            }
            {
                turnAround = true;
                bool ret = segmentInfo.CheckFlags(netSegment.m_flags, turnAround);
                ret = ret && segmentInfoExt.CheckFlags(
                    netSegmentExt.m_flags,
                    tailFlags: segmentTailFlags,
                    headFlags: segmentHeadFlags,
                    tailNodeFlags: nodeTailFlags,
                    headNodeFlags: nodeHeadFlags,
                    turnAround);
                if (ret) return true;
            }

            //fail
            turnAround = false;
            return false;
        }

        static MethodInfo mCheckFlagsExt => typeof(CheckSegmentFlagsCommons).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlagsExt is null");
        static MethodInfo mCheckFlags => typeof(NetInfo.Segment).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags is null");

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodInfo method, int occurance = 1) {
            // callvirt NetInfo+Segment.CheckFlags(valuetype NetSegment+Flags, bool&)
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: occurance);
            Assertion.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_SegmentInfo = Build_LDLoc_SegmentInfo_FromSTLoc(codes, index);
            CodeInstruction LDLoca_turnAround = new CodeInstruction(codes[index - 1]);
            Assertion.Assert(LDLoca_turnAround.opcode == OpCodes.Ldloca_S);
            CodeInstruction LDArg_SegmenteID = GetLDArg(method, "segmentID");

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_SegmentInfo,
                    LDArg_SegmenteID,
                    LDLoca_turnAround,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
                InsertInstructions(codes, newInstructions, index + 1);
            } // end block
        }

        static FieldInfo fSegments => typeof(NetInfo).GetField("m_segments") ?? throw new Exception("fSegments is null");

        public static CodeInstruction Build_LDLoc_SegmentInfo_FromSTLoc(List<CodeInstruction> codes, int index, int counter = 1, int dir = -1) {
            /* IL_0568: ldarg.s      info 
             * IL_056a: ldfld        class NetInfo/Segment[] NetInfo::m_segments <- find this
             * IL_056f: ldloc.s      index_V_39
             * IL_0571: ldelem.ref
             * IL_0572: stloc.s      segment <- seek to this then build ldloc from this*/
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Ldfld, fSegments), index, counter: counter, dir: dir);
            index = SearchGeneric(codes, i => codes[i].IsStloc(), index, counter: 1, dir: 1);
            return codes[index].BuildLdLocFromStLoc();
        }

    }
}
