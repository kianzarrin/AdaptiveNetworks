namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using AdaptiveRoads.Util;
    using PrefabMetadata.API;
    using AdaptiveRoads.UI.RoadEditor;
    using static KianCommons.ReflectionHelpers;
    using static KianCommons.Assertion;
    using static RoadEditorDynamicPropertyToggle_OnEnable;

    /// <summary>
    /// do no toggle if control is pressed.
    /// </summary>
    [HarmonyPatch]
    public static class RoadEditorDynamicPropertyToggle_OnSelectButtonClick {
        static Type tRoadEditorDynamicPropertyToggle =
            Type.GetType("RoadEditorDynamicPropertyToggle, Assembly-CSharp", throwOnError: true);

        static MethodBase TargetMethod() =>
            GetMethod(tRoadEditorDynamicPropertyToggle, "OnSelectButtonClick");

        static bool Prefix(UIComponent c) {
            if (!HelpersExtensions.ControlIsPressed)
                return true;
            OnSelectButtonCtrlClick(c);
            return false;
        }

        public static void OnSelectButtonCtrlClick(UIComponent component) {
            Log.Debug("OnSelectButtonCtrlClick() called");

            var instance = component.GetComponentInParent(tRoadEditorDynamicPropertyToggle);
            var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
            var roadEditor = component.GetComponentInParent<RoadEditorPanel>();
            Log.Debug($"instance={instance} " +
                $"collapsiblePanel={groupPanel} " +
                $"roadEditor={roadEditor}");

            object target = m_TargetObject(instance);
            object element = m_TargetElement(instance);
            if (target is NetLaneProps netLaneProps
                && element is NetLaneProps.Prop prop) {
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
                var propExt = prop as IInfoExtended<NetLaneProps.Prop>;
                AssertNotNull(propExt, "clonableProp");
                panel.AddButton("Duplicate", null, delegate () {
                    var newProp = propExt.Clone();
                    AddArrayElement(
                        roadEditor, groupPanel,
                        target, f_props, newProp);
                });

                if (prop.CanInvert()) {
                    panel.AddButton(
                        "Inverted duplicate",
                        "swaps: required.Inverted<->foribdden.inverted start<->end left<->right\n" +
                        "negates: position.z offset angle",
                        delegate () {
                            var newProp = propExt.Clone();
                            newProp.Self.ToggleRHT_LHT();
                            AddArrayElement(
                                roadEditor, groupPanel,
                                target, f_props, newProp);
                        });
                }
                panel.AddButton("Copy", null, delegate () {
                    ClipBoard.SetData(prop);
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

