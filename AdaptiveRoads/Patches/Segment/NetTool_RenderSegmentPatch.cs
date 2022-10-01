namespace AdaptiveRoads.Patches.Segment {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;

    [HarmonyPatch(typeof(NetTool), "RenderSegment")]
    [InGamePatch]
    static class NetTool_RenderSegmentPatch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            MethodInfo mCheckFlags = typeof(NetInfo.Segment).GetMethod(nameof(NetInfo.Segment.CheckFlags), throwOnError: true);
            MethodInfo mCheckFlagsExt = typeof(NetTool_RenderSegmentPatch).GetMethod(nameof(CheckFlags), throwOnError: true);
            var codes = instructions.ToList();

            int iCheckFlags = codes.Search(c => c.Calls(mCheckFlags));
            int iLdSegmentInfo = codes.Search(_c => _c.IsLdLoc(typeof(NetInfo.Segment), original), startIndex: iCheckFlags, count: -1);
            var ldInfo = TranspilerUtils.GetLDArg(original, "info");
            var insertion = new CodeInstruction[]{
                    ldInfo,
                    codes[iLdSegmentInfo].Clone(),
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
            };

            codes.InsertInstructions(iCheckFlags + 1, insertion);// insert our checkflags after base checkflags
            Log.Succeeded();
            return codes;
        }

        static bool CheckFlags(NetInfo info, NetInfo.Segment segmentInfo) {
            if(segmentInfo.GetMetaData() is not NetInfoExtionsion.Segment segmentExt) return true;
            NetSegmentExt.Flags flags = NetSegmentExt.Flags.UniformSpeedLimit;
            if (info.m_hasParkingSpaces) flags |= NetSegmentExt.Flags.ParkingAllowedBoth;
            return segmentExt.CheckFlags(flags,
                default,default,default,default,default,default,default,default);
        }
    }
}
