using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using PrefabIndeces;

namespace AdvancedRoads.Patches.Segment {
    using System;
    using UnityEngine;
    using KianCommons;
    using static KianCommons.Patches.TranspilerUtils;
    using AdvancedRoads.Manager;

    public static class CheckSegmentFlagsCommons {
        public static bool CheckFlags(NetInfo.Segment segmentInfo, ushort segmentID, ref bool turnAround) {
            var segmentInfoExt = NetInfoExt.Segment.Get(segmentInfo as NetInfoExtension.Segment);
            if (segmentInfoExt == null) return true;

            NetSegmentExt netSegmentExt = NetworkExtensionManager.Instance.SegmentBuffer[segmentID];

            {
                turnAround = false;
                bool ret = NetInfoExt.Segment.CheckFlags(segmentInfo, segmentID.ToSegment().m_flags, turnAround);
                ret &= segmentInfoExt.CheckFlags(
                    netSegmentExt.m_flags,
                    netSegmentExt.Start.m_flags,
                    netSegmentExt.End.m_flags,
                    turnAround);
                if (ret) return true;
            }
            {
                turnAround = false;
                bool ret = NetInfoExt.Segment.CheckFlags(segmentInfo, segmentID.ToSegment().m_flags, turnAround);
                ret &= segmentInfoExt.CheckFlags(
                    netSegmentExt.m_flags,
                    netSegmentExt.Start.m_flags,
                    netSegmentExt.End.m_flags,
                    turnAround);
                if (ret) return true;
            }
            turnAround = false;
             return false;
        }

        static MethodInfo mCheckFlags2 => typeof(CheckSegmentFlagsCommons).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags2 is null");
        static MethodInfo mCheckFlags => typeof(NetInfo.Segment).GetMethod("CheckFlags")
            ?? throw new Exception("mCheckFlags is null");

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, MethodInfo method, int occurance = 1) {
            // callvirt NetInfo+Segment.CheckFlags(valuetype NetSegment+Flags, bool&)
            var index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckFlags), 0, counter: occurance);
            HelpersExtensions.Assert(index != 0, "index!=0");

            CodeInstruction LDLoc_SegmentInfo = new CodeInstruction(codes[index - 4]); // TODO search
            CodeInstruction LDLoca_turnAround = new CodeInstruction(codes[index - 1]); // TODO search
            CodeInstruction LDArg_SegmenteID = GetLDArg(method, "segmentID");

            { // insert our checkflags after base checkflags
                var newInstructions = new[]{
                    LDLoc_SegmentInfo,
                    LDArg_SegmenteID,
                    LDLoca_turnAround,
                    new CodeInstruction(OpCodes.Call, mCheckFlags2),
                    new CodeInstruction(OpCodes.And),
                };
                InsertInstructions(codes, newInstructions, index + 1);
            } // end block
        }

    }
}
