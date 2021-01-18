namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using static AdaptiveRoads.Util.RoadEditorUtils;
    using static KianCommons.ReflectionHelpers;
    using static Util.DPTHelpers;
    using AdaptiveRoads.UI.RoadEditor;

    /// <summary>
    /// highlight lanes
    /// TODO highlight nodes/segments/props ...
    /// TODO add summary hint.
    /// TODO right-click to copy/duplicate/duplicate_invert
    /// </summary>
    [HarmonyPatch]
    internal static class DPT_OnEnable {
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
            object element = GetDPTTargetElement(instance);
            Overlay.HoveredInfo = element;
        }

        static void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam) {
            UICustomControl instance = component.parent.GetComponent(DPTType) as UICustomControl;
            object element = GetDPTTargetElement(instance);
            if(Overlay.HoveredInfo == element)
                Overlay.HoveredInfo = null;

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
