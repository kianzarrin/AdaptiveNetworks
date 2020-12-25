using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdaptiveRoads.Patches.Segment {
    using JetBrains.Annotations;
    

    [HarmonyPatch()]
    public static class RenderInstance {
        static string logPrefix_ = "NetSegment.RenderInstance Transpiler: ";

        // private void NetSegment.RenderInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, int layerMask, NetInfo info, ref RenderManager.Instance data)
        static MethodInfo Target => typeof(global::NetSegment).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodBase TargetMethod() {
            var ret = Target;
            Assertion.Assert(ret != null, "did not manage to find original function to patch");
            Log.Info(logPrefix_ + "aquired method " + ret);
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckSegmentFlagsCommons.PatchCheckFlags(codes, original);
                Log.Info(logPrefix_ + "successfully patched NetSegment.RenderInstance");
                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space
