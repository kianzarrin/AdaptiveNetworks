namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.Util;
    using static AdaptiveRoads.Util.DPTHelpers;

    /// <summary>
    /// if control is pressed, show more options instead of toggling the side panel.
    /// TODO multi-select
    /// </summary>
    [HarmonyPatch]
    public static class DPT_OnSelectButtonClick {
        static Type tRoadEditorDynamicPropertyToggle =
            Type.GetType("RoadEditorDynamicPropertyToggle, Assembly-CSharp", throwOnError: true);

        static MethodBase TargetMethod() =>
            GetMethod(tRoadEditorDynamicPropertyToggle, "OnSelectButtonClick");

        static bool Prefix(UIComponent c) =>
            !HelpersExtensions.ControlIsPressed; // skip if control is pressed (multi select mode)
    }
}

