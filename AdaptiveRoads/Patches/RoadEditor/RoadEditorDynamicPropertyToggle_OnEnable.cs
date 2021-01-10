namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using static AdaptiveRoads.Util.RoadEditorUtils;
    using static KianCommons.ReflectionHelpers;
    using static Util.DPTHelpers;
    using static AdaptiveRoads.UI.RoadEditor.Rendering;

    /// <summary>
    /// highlight lanes
    /// TODO highlight nodes/segments/props ...
    /// TODO add summary hint.
    /// TODO right-click to copy/duplicate/duplicate_invert
    /// </summary>
    [HarmonyPatch]
    internal static class RoadEditorDynamicPropertyToggle_OnEnable {
        static MethodBase TargetMethod() =>
            GetMethod(DPTType, "OnEnable");

        static void Postfix(UIButton ___m_SelectButton, UIButton ___m_DeleteButton, object __instance) {
            ___m_SelectButton.eventMouseEnter -= OnMouseEnter;
            ___m_DeleteButton.eventMouseEnter -= OnMouseEnter;
            ___m_SelectButton.eventMouseLeave -= OnMouseLeave;
            ___m_DeleteButton.eventMouseLeave -= OnMouseLeave;
            ___m_SelectButton.eventMouseDown -= OnMouseDown;

            ___m_SelectButton.eventMouseEnter += OnMouseEnter;
            ___m_DeleteButton.eventMouseEnter += OnMouseEnter;
            ___m_SelectButton.eventMouseLeave += OnMouseLeave;
            ___m_DeleteButton.eventMouseLeave += OnMouseLeave;

            ___m_SelectButton.eventMouseDown += OnMouseDown;
            ___m_SelectButton.buttonsMask |= UIMouseButton.Right;
        }

        static void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam) {
            UICustomControl instance = component.parent.GetComponent(DPTType) as UICustomControl;
            object target = GetDPTTargetObject(instance);
            object element = GetDPTTargetElement(instance);
            //Log.Debug("RoadEditorDynamicPropertyToggle_OnEnable.GetLaneIndex():" +
            //    $" m_TargetObject(instance)={m_TargetObject(instance)}" +
            //    $" m_TargetElement(instance)={m_TargetElement(instance)}");
            if (target is NetInfo netInfo && element is NetInfo.Lane laneInfo) {
                LaneIndex = netInfo.m_lanes.IndexOf(laneInfo);
                Info = netInfo;
                Prop = null;
            }else if (element is NetLaneProps.Prop prop) {
                Prop = prop;
            }
        }

        static void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam) {
            UICustomControl instance = component.parent.GetComponent(DPTType) as UICustomControl;
            object target = GetDPTTargetObject(instance);
            object element = GetDPTTargetElement(instance);
            if (target is NetInfo netInfo && element is NetInfo.Lane laneInfo) {
                int laneIndex = netInfo.m_lanes.IndexOf(laneInfo);
                if (LaneIndex == laneIndex)
                    LaneIndex = -1;
            } else if (element is NetLaneProps.Prop prop) {
                if(prop==Prop)
                    Prop = null;
            }
        }

        static void OnMouseDown(UIComponent component, UIMouseEventParameter eventParam) {
            try {
                UICustomControl dpt = GetDPTInParent(component);
                bool right = eventParam.buttons == UIMouseButton.Right;
                bool left = eventParam.buttons == UIMouseButton.Left;
                bool ctrl = HelpersExtensions.ControlIsPressed;

                if (ctrl && left) {
                    OnToggleDPT(dpt);
                }
                if (!ctrl && left) {
                    DeselectAllDPTs();
                }
                if (right) {
                    OnDPTMoreOptions(dpt);
                }
            }catch (Exception ex) {
                Log.Exception(ex);
            }

        }
    }
}

