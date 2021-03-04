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

    [HarmonyPatch]
    [InGamePatch]
    public static class RenderDestroyedInstance {
        static string logPrefix_ = "NetLane.RenderInsRenderDestroyedInstancetance Transpiler: ";

        // public void RenderDestroyedInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, uint laneID,
        // NetInfo netInfo, NetInfo.Lane laneInfo, NetNode.Flags startFlags, 
        public static MethodBase TargetMethod() => AccessTools.DeclaredMethod(
            typeof(NetLane),
            nameof(NetLane.RenderDestroyedInstance));

        public static IEnumerable<CodeInstruction> Transpiler(
            MethodBase original, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckPropFlagsCommons.PatchCheckFlags(codes, original); 

                Log.Info(logPrefix_ + "successfully patched NetLane.RenderDestroyedInstance");
                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    } // end class
} // end name space
