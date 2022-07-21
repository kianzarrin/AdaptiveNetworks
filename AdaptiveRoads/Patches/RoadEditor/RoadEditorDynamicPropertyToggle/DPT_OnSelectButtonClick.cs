namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;

    /// <summary>
    /// if control is pressed, go to multi select mode instead of toggling the side panel.
    /// </summary>
    [HarmonyPatch]
    public static class DPT_OnSelectButtonClick {
        static Type tRoadEditorDynamicPropertyToggle =
            Type.GetType("RoadEditorDynamicPropertyToggle, Assembly-CSharp", throwOnError: true);

        static MethodBase TargetMethod() =>
            GetMethod(tRoadEditorDynamicPropertyToggle, "OnSelectButtonClick");

        static bool Prefix(UIComponent c) {
#if DEBUG
            if (!HelpersExtensions.ControlIsPressed)
                Log.Debug("DPT clicked.");
#endif
            return !HelpersExtensions.ControlIsPressed; // skip if control is pressed (multi select mode)
        }
    }
}

