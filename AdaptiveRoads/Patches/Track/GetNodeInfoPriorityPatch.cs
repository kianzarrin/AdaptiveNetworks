namespace AdaptiveRoads.Patches {
    using AdaptiveRoads.Manager;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using KianCommons;
    using HarmonyLib;

    /// <summary>
    /// make sure netInfos with tracks get priority.
    /// </summary>
    [HarmonyPatch]
    [InGamePatch]
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
            if(data.Info.TrackLaneCount() > 0) {
                __result += 40E3f; // bigger than damAI. It has the biggest vanilla number which is 20E3.
            }
            if(Log.VERBOSE) Log.Called(data.Info, segmentID, $"result->{__result}");
        }
    }
}