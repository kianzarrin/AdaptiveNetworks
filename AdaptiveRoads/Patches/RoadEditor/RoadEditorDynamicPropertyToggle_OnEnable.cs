namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using AdaptiveRoads.Manager;
    using PrefabMetadata.API;
    using AdaptiveRoads.UI.RoadEditor;
    using static KianCommons.ReflectionHelpers;
    using static KianCommons.Assertion;
    using static RoadEditorDynamicPropertyToggleHelpers;

    /// <summary>
    /// highlight lanes
    /// TODO highlight nodes/segments/props ...
    /// TODO add summary hint.
    /// TODO right-click to copy/duplicate/duplicate_invert
    /// </summary>
    [HarmonyPatch]
    internal static class RoadEditorDynamicPropertyToggle_OnEnable {
        internal static int LaneIndex, NodeIndex, SegmentIndex, PropIndex;
        internal static NetInfo Info;

        static MethodBase TargetMethod() =>
            GetMethod(ToggleType, "OnEnable");

        static void Postfix(UIButton ___m_SelectButton, UIButton ___m_DeleteButton, object __instance) {
            ___m_SelectButton.eventMouseEnter -= OnMouseEnter;
            ___m_DeleteButton.eventMouseEnter -= OnMouseEnter;
            ___m_SelectButton.eventMouseLeave -= OnMouseLeave;
            ___m_DeleteButton.eventMouseLeave -= OnMouseLeave;

            ___m_SelectButton.eventMouseEnter += OnMouseEnter;
            ___m_DeleteButton.eventMouseEnter += OnMouseEnter;
            ___m_SelectButton.eventMouseLeave += OnMouseLeave;
            ___m_DeleteButton.eventMouseLeave += OnMouseLeave;
        }

        static void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam) {
            UICustomControl instance = component.parent.GetComponent(ToggleType) as UICustomControl;
            object target =  GetToggleTargetObject(instance);
            object element = GetToggleTargetElement(instance);
            //Log.Debug("RoadEditorDynamicPropertyToggle_OnEnable.GetLaneIndex():" +
            //    $" m_TargetObject(instance)={m_TargetObject(instance)}" +
            //    $" m_TargetElement(instance)={m_TargetElement(instance)}");
            if (target is NetInfo netInfo && element is NetInfo.Lane laneInfo) {
                LaneIndex = netInfo.m_lanes.IndexOf(laneInfo);
                Info = netInfo;
                SegmentIndex = NodeIndex = PropIndex = 0;
            }
        }

        static void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam) {
            UICustomControl instance = component.parent.GetComponent(ToggleType) as UICustomControl;
            object target = GetToggleTargetObject(instance);
            object element = GetToggleTargetElement(instance);
            if (target is NetInfo netInfo && element is NetInfo.Lane laneInfo) {
                int laneIndex = netInfo.m_lanes.IndexOf(laneInfo);
                if (LaneIndex == laneIndex)
                    LaneIndex = -1;
                if (Info == netInfo)
                    Info = null;

            }
        }
    }
}

