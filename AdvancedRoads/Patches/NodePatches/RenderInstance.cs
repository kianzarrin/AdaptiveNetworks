using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdvancedRoads.Patches.NetNodePatches {
    using JetBrains.Annotations;
    using Util;

    [HarmonyPatch()]
    public static class CalculateGroupData {
        static string logPrefix_ = "NetNode_CalculateGroupData Transpiler: ";

        // RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
        static MethodInfo Target => typeof(global::NetNode).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodBase TargetMethod() {
            var ret = Target;
            HelpersExtensions.Assert(ret != null, "did not manage to find original function to patch");
            Log.Info(logPrefix_+"aquired method " + ret);
            return ret;
        }

        //static bool Prefix(ushort nodeID){}
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckNodeFlagsCommons.PatchAllCheckFlags(codes, Target);
                Log.Info(logPrefix_+"successfully patched NetNode.RenderInstance");
                return codes;
            }catch(Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space