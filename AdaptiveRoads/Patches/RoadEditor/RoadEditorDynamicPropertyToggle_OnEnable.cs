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

    /// <summary>
    /// highlight lanes
    /// TODO highlight nodes/segments/props ...
    /// TODO add summary hint.
    /// TODO right-click to copy/duplicate/duplicate_invert
    /// </summary>
    [HarmonyPatch]
    public static class RoadEditorDynamicPropertyToggle_OnEnable {
        public static int LaneIndex, NodeIndex, SegmentIndex, PropIndex;
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
            GetMethod(tRoadEditorDynamicPropertyToggle, "OnEnable");

        public static void Postfix(UIButton ___m_SelectButton, UIButton ___m_DeleteButton, object __instance) {
            ___m_SelectButton.eventMouseEnter -= OnMouseEnter;
            ___m_DeleteButton.eventMouseEnter -= OnMouseEnter;
            ___m_SelectButton.eventMouseLeave -= OnMouseLeave;
            ___m_DeleteButton.eventMouseLeave -= OnMouseLeave;

            ___m_SelectButton.eventMouseEnter += OnMouseEnter;
            ___m_DeleteButton.eventMouseEnter += OnMouseEnter;
            ___m_SelectButton.eventMouseLeave += OnMouseLeave;
            ___m_DeleteButton.eventMouseLeave += OnMouseLeave;

            string tooltip = ". right click for more options";
            if(!___m_SelectButton.tooltip.Contains(tooltip))
                ___m_SelectButton.tooltip += tooltip;

            ___m_SelectButton.eventMouseDown -= OnMouseDown;
            ___m_SelectButton.eventMouseDown += OnMouseDown;
        }

        static void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam) {
            object instance = component.parent.GetComponent(tRoadEditorDynamicPropertyToggle);
            object target = m_TargetObject(instance);
            object element = m_TargetElement(instance);
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
            object instance = component.parent.GetComponent(tRoadEditorDynamicPropertyToggle);
            object target = m_TargetObject(instance);
            object element = m_TargetElement(instance);
            if (target is NetInfo netInfo && element is NetInfo.Lane laneInfo) {
                int laneIndex = netInfo.m_lanes.IndexOf(laneInfo);
                if (LaneIndex == laneIndex)
                    LaneIndex = -1;
                if (Info == netInfo)
                    Info = null;

            }
        }

        static void OnMouseDown(UIComponent component, UIMouseEventParameter eventParam) {
            Log.Debug("RoadEditorDynamicPropertyToggle_OnEnable.OnMousDown()");

            // right click
            if (eventParam.buttons != UIMouseButton.Right) return;

            var instance = component.GetComponentInParent(tRoadEditorDynamicPropertyToggle);
            var groupPanel = component.GetComponent<RoadEditorCollapsiblePanel>();
            var sidePanel = component.GetComponent<RoadEditorPanel>();
            Log.Debug($"instance={instance} " +
                $"collapsiblePanel={groupPanel} " +
                $"sidePanel={sidePanel}");


            object target = m_TargetObject(instance);
            object element = m_TargetElement(instance);
            if ( target is NetLaneProps netLaneProps
                && element is NetLaneProps.Prop prop ) {
                int propIndex = netLaneProps.m_props.IndexOf(prop);
                //NetInfo netInfo = null; ;
                //int laneIndex = 0; ;
                //foreach (var netInfo2 in NetInfoExtionsion.EditedNetInfos) {
                //    for (int laneIndex2 = 0; laneIndex2 < netInfo2.m_lanes.Length; ++laneIndex2) {
                //        if(netInfo2.m_lanes[laneIndex2].m_laneProps == netLaneProps) {
                //            netInfo = netInfo2;
                //            laneIndex = laneIndex2;
                //        }
                //    }
                //}
                var panel = MiniPanel.Display();
                var f_props = typeof(NetLaneProps).GetField(nameof(NetLaneProps.m_props));
                var clonableProp = prop as ICloneable;
                AssertNotNull(clonableProp, "clonableProp");
                panel.AddButton("Duplicate", null, delegate() {
                    AddArrayElement(
                        sidePanel, groupPanel,
                        target, f_props, clonableProp);
                });
            }
        }
        public static object AddArrayElement(
            RoadEditorPanel roadEditorPanel, RoadEditorCollapsiblePanel groupPanel, 
            object target, FieldInfo arrayField, object newElement) {
            Log.Debug("RoadEditorDynamicPropertyToggle_OnEnable.AddArrayElement() called");
            newElement = AssetEditorRoadUtils.AddArrayElement(
                target, arrayField, newElement);
            roadEditorPanel.AddToArrayField(groupPanel, newElement, arrayField, target);
            return newElement;
        }
    }


}

