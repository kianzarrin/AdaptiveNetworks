namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.UI.RoadEditor.Templates;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using static Util.DPTHelpers;
    using static AdaptiveRoads.Util.RoadEditorUtils;

    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    [HarmonyPatch("OnButtonClick")]
    internal static class RoadEditorCollapsiblePanel_OnButtonClick {

        static bool Prefix(UIComponent component) =>
            !HelpersExtensions.ControlIsPressed;
    }

    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    [HarmonyPatch("OnEnable")]
    internal static class RoadEditorCollapsiblePanel_OnEnablek {
        static void Postfix(RoadEditorCollapsiblePanel __instance) {
            var button = __instance.m_Button;
            button.eventMouseDown -= OnMouseDown;
            button.eventMouseDown += OnMouseDown;
        }

        static void OnMouseDown(UIComponent component, UIMouseEventParameter eventParam) {
            bool right = eventParam.buttons == UIMouseButton.Right;
            if (right) {
                var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
                OnShowOptions(groupPanel);
            }

        }

        static void OnShowOptions(RoadEditorCollapsiblePanel groupPanel) {
            try {
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
                            panel.AddButton("Save Tamplate", null, () => {
                                SaveTemplatePanel.Display(m_props);
                            });
                        }
                        if (clipBoardHasData) {
                            panel.AddButton("Paste all props", null,
                                () => PasteAll(groupPanel));
                        }
                        panel.AddButton(
                            "Displace all",
                            null,
                            () => DisplaceAll(m_props));
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
                    panel.AddButton(
                        "Displace all",
                        null,
                        () => RoadEditorUtils.DisplaceAllProps(m_lanes));
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

                var toggles = instance.m_Panel.GetComponentsInChildren(DPTType);
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
            Log.Debug("PasteAll called");
            NetLaneProps.Prop[] props = ClipBoard.GetDataArray() as NetLaneProps.Prop[];
            AddProps(groupPanel, props);
        }
    }
}

