namespace AdaptiveRoads.Patches {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using KianCommons;
    /// <summary>
    /// avoid stripping dot with space after
    /// </summary>
    [HarmonyPatch]
    //[InGamePatch]
    static class StripNamePatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return typeof(PackageHelper).GetMethod(nameof(PackageHelper.StripName), throwOnError: true);
            yield return typeof(SaveAssetPanel).GetMethod("StripName", throwOnError: true);
        }

        static bool Prefix(string name, ref string __result) {
            // find last index of dot that is not followed by space
            int lastIndex = -1;
            for(int i = 0; i < name.Length - 1; ++i) {
                if (name[i] == '.' && name[i + 1] != ' ') {
                    lastIndex = i;
                }
            }

            // if last index found then remove everything up to and including dot
            if (lastIndex >= 0) {
                name = name.Remove(0, lastIndex + 1);
            }

            // override game result
            __result = name;
            return false;
        }


    }
}
