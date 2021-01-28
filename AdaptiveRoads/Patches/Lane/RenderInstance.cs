using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

// TODO patch RenderDestroyedInstance too.
namespace AdaptiveRoads.Patches.Lane {
    using JetBrains.Annotations;

    [HarmonyPatch()]
    public static class RenderInstance {
        static string logPrefix_ = "NetLane.RenderInstance Transpiler: ";

        // public void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, uint laneID, NetInfo.Lane laneInfo, NetNode.Flags startFlags, NetNode.Flags endFlags, Color startColor, Color endColor, float startAngle, float endAngle, bool invert, int layerMask, Vector4 objectIndex1, Vector4 objectIndex2, ref RenderManager.Instance data, ref int propIndex)
        static MethodInfo Target => ReflectionHelpers.GetMethod(
            typeof(NetLane), nameof(NetLane.RenderInstance));
        public static MethodBase TargetMethod() => Target;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckPropFlagsCommons.PatchCheckFlags(codes, Target);
                PropOverlay.Patch(codes, Target);
                TreeOverlay.Patch(codes, Target);
                Log.Info(logPrefix_ + "successfully patched NetLane.RenderInstance");
                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space
