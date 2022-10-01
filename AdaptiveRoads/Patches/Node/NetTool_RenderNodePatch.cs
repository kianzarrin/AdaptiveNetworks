namespace AdaptiveRoads.Patches.Node {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;

    [HarmonyPatch(typeof(NetTool), "RenderNode")]
    [InGamePatch]
    static class NetTool_RenderNodePatch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            MethodInfo mCheckFlags = typeof(NetInfo.Node).GetMethod(nameof(NetInfo.Node.CheckFlags), throwOnError: true);
            MethodInfo mCheckFlagsExt = typeof(NetTool_RenderNodePatch).GetMethod(nameof(CheckFlags), throwOnError: true);
            var codes = instructions.ToList();

            int iCheckFlags = codes.Search(c => c.Calls(mCheckFlags));
            int iLdNodeInfo = codes.Search(_c => _c.IsLdLoc(typeof(NetInfo.Node), original), startIndex: iCheckFlags, count: -1);
            var ldInfo = TranspilerUtils.GetLDArg(original, "info");
            var insertion = new CodeInstruction[]{
                    ldInfo,
                    codes[iLdNodeInfo].Clone(),
                    new CodeInstruction(OpCodes.Call, mCheckFlagsExt),
                    new CodeInstruction(OpCodes.And),
            };

            codes.InsertInstructions(iCheckFlags + 1, insertion);// insert our checkflags after base checkflags
            return codes;
        }

        static bool CheckFlags(NetInfo info, NetInfo.Node nodeInfo) {
            if(nodeInfo.GetMetaData() is not NetInfoExtionsion.Node nodeExt) return true;
            return nodeExt.CheckFlags(default,default,default,default,default);
        }
    }
}
