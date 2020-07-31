using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using KianCommons.Patches;

// TODO patch RenderDestroyedInstance too.
namespace AdvancedRoads.Patches.Lane {
    using JetBrains.Annotations;

    [HarmonyPatch()]
    public static class RenderDestroyedInstance {
        static string logPrefix_ = "NetLane.RenderInsRenderDestroyedInstancetance Transpiler: ";

        // public void RenderDestroyedInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, uint laneID,
        // NetInfo netInfo, NetInfo.Lane laneInfo, NetNode.Flags startFlags, 
        static MethodInfo Target => typeof(global::NetLane).GetMethod("RenderDestroyedInstance", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodBase TargetMethod() {
            var ret = Target;
            HelpersExtensions.Assert(ret != null, "did not manage to find original function to patch");
            Log.Info(logPrefix_ + "aquired method " + ret);
            return ret;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                CheckPropFlagsCommons.PatchCheckFlags(codes, Target); 

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
