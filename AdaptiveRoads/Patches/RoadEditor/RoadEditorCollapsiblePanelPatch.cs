namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.UI.RoadEditor.MenuStyle;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using static Util.DPTHelpers;
    using static AdaptiveRoads.Util.RoadEditorUtils;
    using static AdaptiveRoads.Manager.NetInfoExtionsion;

    //[HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    //[HarmonyPatch("OnButtonClick")]
    //internal static class RoadEditorCollapsiblePanel_OnButtonClick {

    //    static bool Prefix(UIComponent component) =>
    //        !HelpersExtensions.ControlIsPressed;
    //}

    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    [HarmonyPatch("OnEnable")]
    internal static class RoadEditorCollapsiblePanel_OnEnable {
        static void Postfix(RoadEditorCollapsiblePanel __instance) {
            var button = __instance.LabelButton;
            button.buttonsMask |= UIMouseButton.Right;
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
                static string Str(string item, int count) => count == 1 ? item : $"all {item}s";
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
                            panel.AddButton("Save Template", null, () => {
                                SavePropTemplatePanel.Display(m_props);
                            });
                        }
                        if (clipBoardHasData) {
                            string strItems = Str("prop", ClipBoard.Count);
                            panel.AddButton("Paste " + strItems, null,
                                () => PasteAllProps(groupPanel));
                        }
                        panel.AddButton(
                            "Displace all",
                            null,
                            () => DisplaceAll(m_props));
                    }
                } else if (groupPanel.GetArray() is TransitionProp[] tprops) {
                    bool hasItems = tprops.Length > 0;
                    bool clipBoardHasData = ClipBoard.HasData<TransitionProp>();
                    if (hasItems || clipBoardHasData) {
                        var panel = MiniPanel.Display();
                        if (hasItems) {
                            panel.AddButton("Copy all props", null,
                            () => ClipBoard.SetData(tprops));
                            panel.AddButton("Clear all props", null,
                                () => ClearAll(groupPanel));
                            panel.AddButton("Save Template", null, () => {
                                SaveTransitionPropTemplatePanel.Display(tprops);
                            });
                        }
                        if (clipBoardHasData) {
                            string strItems = Str("prop", ClipBoard.Count);
                            panel.AddButton("Paste " + strItems, null,
                                () => PasteAllTransitionProps(groupPanel));
                        }
                        panel.AddButton(
                            "Displace all",
                            null,
                            () => DisplaceAll(tprops));
                    }
                } else if (groupPanel.GetArray() is NetInfo.Node[] m_nodes) {
                    bool hasItems = !m_nodes.IsNullorEmpty();
                    bool clipBoardHasData = ClipBoard.HasData<NetInfo.Node>();
                    if (hasItems || clipBoardHasData) {
                        var panel = MiniPanel.Display();
                        if (hasItems) {
                            panel.AddButton("Copy all nodes", null,
                            () => ClipBoard.SetData(m_nodes));
                            panel.AddButton("Clear all nodes", null,
                                () => ClearAll(groupPanel));
                            panel.AddButton("Save Template", null, () => {
                                SaveNodeTemplatePanel.Display(m_nodes);
                            });
                        }
                        if (clipBoardHasData) {
                            string strItems = Str("node", ClipBoard.Count);
                            panel.AddButton("Paste " + strItems, null,
                                () => PasteAllNodes(groupPanel));
                        }
                    }
                } else if (groupPanel.GetArray() is NetInfo.Segment[] m_segments) {
                    bool hasItems = !m_segments.IsNullorEmpty();
                    bool clipBoardHasData = ClipBoard.HasData<NetInfo.Segment>();
                    if (hasItems || clipBoardHasData) {
                        var panel = MiniPanel.Display();
                        if (hasItems) {
                            panel.AddButton("Copy all segments", null,
                            () => ClipBoard.SetData(m_segments));
                            panel.AddButton("Clear all segments", null,
                                () => ClearAll(groupPanel));
                            panel.AddButton("Save Template", null, () => {
                                SaveSegmentTemplatePanel.Display(m_segments);
                            });
                        }
                        if (clipBoardHasData) {
                            string strItems = Str("segment", ClipBoard.Count);
                            panel.AddButton("Paste " + strItems, null,
                                () => PasteAllSegments(groupPanel));
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
        static void PasteAllTransitionProps(RoadEditorCollapsiblePanel groupPanel) {
            Log.Called();
            AddTransitionProps(groupPanel, ClipBoard.GetTransitionProps(), ClipBoard.SourceTrack);
        }

        static void PasteAllProps(RoadEditorCollapsiblePanel groupPanel) {
            Log.Called();
            AddProps(groupPanel, ClipBoard.GetProps(), ClipBoard.SourceInfo, ClipBoard.SourceLane);
        }
        static void PasteAllNodes(RoadEditorCollapsiblePanel groupPanel) {
            Log.Called();
            AddNodes(groupPanel, ClipBoard.GetNodes(), ClipBoard.SourceInfo);
        }
        static void PasteAllSegments(RoadEditorCollapsiblePanel groupPanel) {
            Log.Called();
            AddSegments(groupPanel, ClipBoard.GetSegments(), ClipBoard.SourceInfo);
        }
    }
}

