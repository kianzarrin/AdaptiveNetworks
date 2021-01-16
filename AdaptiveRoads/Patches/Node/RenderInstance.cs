using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

namespace AdaptiveRoads.Patches.Node {
    using JetBrains.Annotations;
    

    [HarmonyPatch()]
    public static class RenderInstance {
        static string logPrefix_ = "NetNode.RenderInstance Transpiler: ";

        // RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
        static MethodInfo Target => typeof(global::NetNode).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodBase TargetMethod() {
            var ret = Target;
            Assertion.Assert(ret != null, "did not manage to find original function to patch");
            Log.Info(logPrefix_ + "aquired method " + ret);
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occurance: 1, counterGetSegment: 2); //DC
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occurance: 2, counterGetSegment: 1); //Junction
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occurance: 3, counterGetSegment: 0); //End

                // Bend node -> segment.Checkflags (does not use stored flags.)
                CheckNodeFlagsCommons.PatchCheckFlags(codes, Target, occurance: 4, counterGetSegment: 2); // DC bend

                NodeOverlay.Patch(codes, original, occurance: 1); // DC
                NodeOverlay.Patch(codes, original, occurance: 2); // junction
                NodeOverlay.Patch(codes, original, occurance: 3); // end
                NodeOverlay.PatchBend(codes, original, 4); // end
                NodeOverlay.Patch(codes, original, occurance: 5); // DC-bend

                Log.Info(logPrefix_ + "successfully patched NetNode.RenderInstance");
                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space
