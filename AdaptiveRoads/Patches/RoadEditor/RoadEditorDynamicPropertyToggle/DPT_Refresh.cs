namespace AdaptiveRoads.Patches {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using static AdaptiveRoads.Util.RoadEditorUtils;
    using static KianCommons.ReflectionHelpers;
    using static Util.DPTHelpers;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.Util;
    using AdaptiveRoads.Manager;

    [HarmonyPatch]
    public static class DPT_Refresh {
        static MethodBase TargetMethod() => GetMethod(DPTType, "OnEnable");
        public static void Postfix(object target, ref bool __result) {
        }
    }
}

