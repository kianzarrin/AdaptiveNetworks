namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using System.Linq;

    /// <summary>
    /// extend CreateArrayField to catch highlighted lanes
    /// </summary>
    [HarmonyPatch]
    public static class RoadEditorDynamicPropertyToggle_OnEnable {
        public static int LaneIndex;
        public static NetInfo Info;

        static Type tRoadEditorDynamicPropertyToggle =
            Type.GetType("RoadEditorDynamicPropertyToggle, Assembly-CSharp", throwOnError: true);

        static FieldInfo f_TargetObject =
            AccessTools.DeclaredField(tRoadEditorDynamicPropertyToggle, "m_TargetObject")
            ?? throw new Exception("f_TargetObject null");
        static object m_TargetObject(object instance) => f_TargetObject.GetValue(instance);

        static FieldInfo f_TargetElement =
            AccessTools.DeclaredField(tRoadEditorDynamicPropertyToggle, "m_TargetElement")
            ?? throw new Exception("f_TargetElement null");
        static object m_TargetElement(object instance) => f_TargetElement.GetValue(instance);

        static MethodBase TargetMethod() =>
            tRoadEditorDynamicPropertyToggle
            .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix(UIButton ___m_SelectButton, UIButton ___m_DeleteButton, object __instance) {
            ___m_SelectButton.eventMouseEnter += (_, __) => OnMouseEnter(__instance);
            ___m_DeleteButton.eventMouseEnter += (_, __) => OnMouseEnter(__instance);
            ___m_SelectButton.eventMouseLeave += (_, __) => OnMouseLeave(__instance);
            ___m_DeleteButton.eventMouseLeave += (_, __) => OnMouseLeave(__instance);
        }

        static void OnMouseEnter(object instance) {
            //Log.Debug("RoadEditorDynamicPropertyToggle_OnEnable.GetLaneIndex():" +
            //    $" m_TargetObject(instance)={m_TargetObject(instance)}" +
            //    $" m_TargetElement(instance)={m_TargetElement(instance)}");
            if ((m_TargetObject(instance) is NetInfo netInfo)
                && (m_TargetElement(instance) is NetInfo.Lane laneInfo)) {
                LaneIndex = netInfo.m_lanes.IndexOf(laneInfo);
                Info = netInfo;
            }
        }

        static void OnMouseLeave(object instance) {
            if ((m_TargetObject(instance) is NetInfo netInfo)
                && (m_TargetElement(instance) is NetInfo.Lane laneInfo)) {
                int laneIndex = netInfo.m_lanes.IndexOf(laneInfo);
                if (LaneIndex == laneIndex)
                    LaneIndex = -1;
                if (Info == netInfo)
                    Info = null;
            }
        }

    }
}

