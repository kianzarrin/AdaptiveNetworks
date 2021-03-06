// TODO patch RenderDestroyedInstance too.
namespace AdaptiveRoads.Patches.Lane {
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [HarmonyPatch]
    [InGamePatch]
    [HarmonyBefore("com.klyte.redirectors.PS")]
    public static class RenderInstance {
        static string logPrefix_ = "NetLane.RenderInstance Transpiler: ";

        // public void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, uint laneID, NetInfo.Lane laneInfo, NetNode.Flags startFlags, NetNode.Flags endFlags, Color startColor, Color endColor, float startAngle, float endAngle, bool invert, int layerMask, Vector4 objectIndex1, Vector4 objectIndex2, ref RenderManager.Instance data, ref int propIndex)
        static MethodBase TargetMethod() => ReflectionHelpers.GetMethod(
            typeof(NetLane), nameof(NetLane.RenderInstance));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckPropFlagsCommons.PatchCheckFlags(codes, original);
                Log.Info(logPrefix_ + "successfully patched NetLane.RenderInstance");
                return codes;
            } catch(Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class

    [HarmonyPatch]
    [HarmonyBefore("com.klyte.redirectors.PS")]
    public static class RenderInstanceOverlayPatch {
        static MethodBase TargetMethod() => ReflectionHelpers.GetMethod(
            typeof(NetLane), nameof(NetLane.RenderInstance));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                PropOverlay.Patch(codes, original);
                TreeOverlay.Patch(codes, original);
                Log.Info(ReflectionHelpers.ThisMethod + " successful!");
                return codes;
            } catch(Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    }
} // end name space
