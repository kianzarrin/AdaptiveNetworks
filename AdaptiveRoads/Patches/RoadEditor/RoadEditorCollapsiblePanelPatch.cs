namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using static RoadEditorDynamicPropertyToggleHelpers;

    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    [HarmonyPatch("OnButtonClick")]
    internal static class RoadEditorCollapsiblePanelPatch {
        [HarmonyPrefix]
        static bool OnButtonClickPrefix(UIComponent component) {
            if (!HelpersExtensions.ControlIsPressed)
                return true;
            OnButtonControlClick(component);
            return false;
        }

        static void OnButtonControlClick(UIComponent component) {
            try {
                var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
                Array array = groupPanel.GetArray();
                var target = groupPanel.GetTarget();
                if (groupPanel.GetArray() is NetLaneProps.Prop[] m_props) {
                    bool hasItems = m_props.Length > 0;
                    bool clipBoardHasData = ClipBoard.HasData<NetLaneProps.Prop>();
                    if (hasItems || clipBoardHasData) {
                        var panel = MiniPanel.Display();
                        if (hasItems) {
                            panel.AddButton("Copy all props", null,
                            () => ClipBoard.SetData(m_props));
                            panel.AddButton("Clear all props", null,
                                () => ClearAll(groupPanel));
                        }
                        if (clipBoardHasData) {
                            panel.AddButton("Paste all props", null,
                                () => PasteAll(groupPanel));
                        }
                    }
                } else if (
                    array is NetInfo.Lane[] m_lanes
                    && m_lanes.Any(_lane => _lane.HasProps())
                    && target == NetInfoExtionsion.EditedNetInfo) {
                    var panel = MiniPanel.Display();
                    panel.AddButton(
                        "Copy props to other elevation",
                        "appends props to other elevations",
                        () => PropHelpers.CopyPropsToOtherElevations(clear: false));
                    panel.AddButton(
                        "replace props to other elevation",
                        "clears props from other elevations before copying.",
                        () => PropHelpers.CopyPropsToOtherElevations(clear: true));
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static void ClearAll(RoadEditorCollapsiblePanel instance) {
            try {
                var roadEditor = instance.component.GetComponentInParent<RoadEditorPanel>();
                var sidePanel = roadEditor.GetSidePanel();
                if (sidePanel != null && sidePanel.GetTarget() is NetLaneProps.Prop) {
                    Log.Debug("Destroying prop side panel");
                    roadEditor.DestroySidePanel();
                }

                var toggles = instance.m_Panel.GetComponentsInChildren(ToggleType);
                foreach (UICustomControl toggle in toggles) {
                    toggle.component.parent.RemoveUIComponent(toggle.component);
                    UnityEngine.Object.Destroy(toggle.gameObject);
                }


                instance.SetArray(new NetLaneProps.Prop[0]);
                roadEditor.OnObjectModified();
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static void PasteAll(RoadEditorCollapsiblePanel groupPanel) {
            try {
                Log.Debug("PasteAll called");
                NetLaneProps.Prop[] props = ClipBoard.GetDataArray() as NetLaneProps.Prop[];
                if (props == null || props.Length == 0) return;
                NetLaneProps.Prop[] m_props = groupPanel.GetArray() as NetLaneProps.Prop[];
                var m_props2 = m_props.AddRangeToArray(props);

                var roadEditor = groupPanel.component.GetComponentInParent<RoadEditorPanel>();
                var sidePanel = roadEditor.GetSidePanel();
                var arrayField = groupPanel.GetField();
                var target = roadEditor.GetTarget();


                Log.Debug($"Pasting {props.Length}+{m_props.Length}={m_props2.Length}");
                groupPanel.SetArray(m_props2);
                foreach (var prop in props) {
                    roadEditor.AddToArrayField(groupPanel, prop, arrayField, target);
                }
                roadEditor.OnObjectModified();
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}

