using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AdaptiveRoads.Data.NetworkExtensions;

namespace AdaptiveRoads.Patches.Segment {
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System;
    using KianCommons.Patches;

    public static class CheckSegmentFlagsCommons {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;
        public static bool CheckFlags(NetInfo.Segment segmentInfo, ushort segmentID, ref bool turnAround) {
            var segmentInfoExt = segmentInfo?.GetMetaData();
            if (segmentInfoExt == null) return true; // bypass

            ref NetSegmentExt netSegmentExt = ref man_.SegmentBuffer[segmentID];
            ref NetSegment netSegment = ref segmentID.ToSegment();
            ref NetNode netNodeStart = ref netSegment.m_startNode.ToNode();
            ref NetNode netNodeEnd = ref netSegment.m_endNode.ToNode();
            ref NetNodeExt netNodeExtStart = ref man_.NodeBuffer[netSegment.m_startNode];
            ref NetNodeExt netNodeExtEnd = ref man_.NodeBuffer[netSegment.m_endNode];

            var segmentTailFlags = netSegmentExt.Start.m_flags; 
            var segmentHeadFlags = netSegmentExt.End.m_flags;
            var nodeTailFlags = netNodeStart.m_flags;
            var nodeHeadFlags = netNodeEnd.m_flags;
            var nodeExtTailFlags = netNodeExtStart.m_flags;
            var nodeExtHeadFlags = netNodeExtEnd.m_flags;

            bool reverse = /*netSegment.IsInvert() ^*/ NetUtil.LHT;
            if (reverse) {
                Helpers.Swap(ref segmentTailFlags, ref segmentHeadFlags);
                Helpers.Swap(ref nodeTailFlags, ref nodeHeadFlags);
                //Log.DebugWait($"CheckSegmentFlagsCommons: segment:{segmentID} is reverse");
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
                    tailNodeExtFlags: nodeExtTailFlags,
                    headNodeExtFlags: nodeExtHeadFlags,
                    userData: netSegmentExt.UserData,
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
                    tailNodeExtFlags: nodeExtTailFlags,
                    headNodeExtFlags: nodeExtHeadFlags,
                    userData: netSegmentExt.UserData,
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
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodBase method, int occurance = 1) {
            // callvirt NetInfo+Segment.CheckFlags(valuetype NetSegment+Flags, bool&)
            var index = codes.Search(c => c.Calls(mCheckFlags), count: occurance);
            Assertion.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_SegmentInfo = GetPrevLdLocSegmentInfo(method, codes, index);
            CodeInstruction LDLoca_turnAround = new CodeInstruction(codes[index - 1]);
            Assertion.Assert(LDLoca_turnAround.opcode == OpCodes.Ldloca_S);
            CodeInstruction LDArg_SegmenteID = TranspilerUtils.GetLDArg(method, "segmentID");

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_SegmentInfo,
                    LDArg_SegmenteID,
                    LDLoca_turnAround,
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
                };
                codes.InsertInstructions(index + 1, newInstructions);
            } // end block
        }

        static FieldInfo fSegments => typeof(NetInfo).GetField("m_segments") ?? throw new Exception("fSegments is null");

        public static CodeInstruction GetPrevLdLocSegmentInfo(MethodBase method, List<CodeInstruction> codes, int index) {
            index = codes.Search(c => c.IsLdLoc(typeof(NetInfo.Segment), method), index, count: -1);
            return codes[index].Clone(); // duplicated without lables and blocks
        }

    }
}
