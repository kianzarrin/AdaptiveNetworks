namespace AdaptiveRoads.Patches.Node {
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [HarmonyPatch()]
    [InGamePatch]
    public static class RenderInstance {
        public static MethodBase TargetMethod() {
            // RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
            var ret = typeof(NetNode).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);
            Assertion.Assert(ret != null, "did not manage to find original function to patch");
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 1, counterGetSegment: 2); //DC
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 2, counterGetSegment: 1); //Junction
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 3, counterGetSegment: 0); //End

                // Bend node -> segment.Checkflags (does not use info flags.)
                CheckNodeFlagsCommons.PatchCheckFlags(codes, original, occuranceCheckFlags: 4, counterGetSegment: 2); // DC bend


                Log.Info(ReflectionHelpers.ThisMethod + " successful!");
                return codes;
            } catch(Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class

    [HarmonyPatch()]
    public static class RenderInstanceOverlayPatch {

        public static MethodBase TargetMethod() {
            // RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
            var ret = typeof(NetNode).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);
            Assertion.Assert(ret != null, "did not manage to find original function to patch");
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                NodeOverlay.Patch(codes, original, occuranceDrawMesh: 1, counterGetSegment: 2); // DC
                NodeOverlay.Patch(codes, original, occuranceDrawMesh: 2, counterGetSegment: 1); // junction
                NodeOverlay.Patch(codes, original, occuranceDrawMesh: 3, counterGetSegment: 0); // end
                NodeOverlay.PatchBend(codes, original, occuranceDrawMesh: 4); // end
                NodeOverlay.Patch(codes, original, occuranceDrawMesh: 5, counterGetSegment: 0); // DC-bend
                Log.Info(ReflectionHelpers.ThisMethod + " successful!");
                return codes;
            } catch(Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space
