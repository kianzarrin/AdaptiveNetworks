namespace AdaptiveRoads.Patches.Lane {
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [HarmonyPatch(typeof(NetLane), nameof(NetLane.RefreshInstance))]
    [InGamePatch]
    [HarmonyBefore("com.klyte.redirectors.PS")]
    public static class RefreshInstance {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = instructions.ToList();
                SeedIndexCommons.Patch(codes, original);
                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    }
}
