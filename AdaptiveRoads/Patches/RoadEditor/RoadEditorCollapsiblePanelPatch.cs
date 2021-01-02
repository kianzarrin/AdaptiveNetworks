namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using AdaptiveRoads.Manager;
    using PrefabMetadata.API;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.Util;
    using static KianCommons.ReflectionHelpers;
    using static KianCommons.Assertion;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// highlight lanes
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    internal static class RoadEditorCollapsiblePanelPatch {
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        static void OnEnablePostfix(RoadEditorCollapsiblePanel __instance) {
            var btn = __instance.LabelButton;
            btn.eventClick -= OnClick;
            btn.eventClick += OnClick;

            var addbutton = __instance.m_Panel
                .GetComponentInChildren<RoadEditorAddButton>();


            if (__instance.GetArray() is NetLaneProps.Prop[]) {
                string tooltip = ". CTRL+Click for more options";
                if (!btn.tooltip.Contains(tooltip))
                    btn.tooltip += tooltip;
            }
        }


        static void OnClick(UIComponent component, UIMouseEventParameter eventParam) {
            var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
            if (groupPanel.GetArray() is NetLaneProps.Prop[] m_props) {
                var panel = MiniPanel.Display();
                panel.AddButton("Copy all props", null,
                    () => ClipBoard.Data = m_props.Clone());
                panel.AddButton("Paste all props", null,
                    () => PasteAll(groupPanel));
                panel.AddButton("Clear all props", null,
                    () => ClearAll(groupPanel)); 
                    
            }
        }

        static void ClearAll(RoadEditorCollapsiblePanel instance) {
            instance.SetArray(null);
            var roadEditor = instance.component.GetComponentInParent<RoadEditorPanel>();
            var sidePanel = roadEditor.GetSidePanel();
            if(sidePanel !=null && sidePanel.GetTarget() is NetLaneProps.Prop)
                roadEditor.DestroySidePanel();

            var toggleType = RoadEditorDynamicPropertyToggle_OnEnable.tRoadEditorDynamicPropertyToggle;
            var toggles = instance.m_Panel.GetComponentsInChildren(toggleType);
            foreach (UICustomControl toggle in toggles) {
                toggle.component.parent.RemoveUIComponent(toggle.component);
                UnityEngine.Object.Destroy(toggle.gameObject);
            }
            roadEditor.OnObjectModified();
        }

        static void PasteAll(RoadEditorCollapsiblePanel groupPanel) {
            NetLaneProps.Prop[] m_props = null;
            if (ClipBoard.Data is IEnumerable<NetLaneProps.Prop> props) {
                m_props = props.Select(_prop => _prop.Clone()).ToArray();
            } else if (ClipBoard.Data is NetLaneProps.Prop prop) {
                m_props = new[] { prop };
            }

            var roadEditor = groupPanel.component.GetComponentInParent<RoadEditorPanel>();
            var sidePanel = roadEditor.GetSidePanel();
            var arrayField = groupPanel.GetField();
            var target = roadEditor.GetTarget();
            groupPanel.SetArray(m_props);
            foreach (var prop in m_props) {
                roadEditor.AddToArrayField(groupPanel, prop, arrayField, target);
            }
            roadEditor.OnObjectModified();
        }

    }
}

