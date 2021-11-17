namespace AdaptiveRoads.Patches {
    using AdaptiveRoads.Manager;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System;
    using static KianCommons.ReflectionHelpers;
    using HarmonyLib;

    /// <summary>
    /// make sure netInfos with tracks get priority.
    /// </summary>
    [HarmonyPatch]
    static class GetNodeInfoPriorityPatch {
        static IEnumerable<MethodBase> TargetMethods() {
            var parenType = typeof(NetAI);
            var childTypes = parenType
                .Assembly
                .GetExportedTypes()
                .Where(childType => parenType.IsAssignableFrom(childType)); // parent is assignable from child
            foreach(var type in childTypes) {
                var method = AccessTools.DeclaredMethod(type, nameof(NetAI.GetNodeInfoPriority));
                if(method != null)
                    yield return method;
            }
        }
        static void Postfix(ushort segmentID, ref NetSegment data, ref float __result) {
            if(data.Info?.GetMetaData() is NetInfoExtionsion.Net infoExt && infoExt.TrackLaneCount > 0) {
                __result += 40E3f; //bigger than damAI has the biggest vanilla number which is 20E3.
            }
        }
    }
}