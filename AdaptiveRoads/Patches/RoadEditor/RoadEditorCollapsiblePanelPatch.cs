namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// highlight lanes
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    [HarmonyPatch("OnEnable")]
    internal static class RoadEditorCollapsiblePanelPatch {
        [HarmonyPostfix]
        static void OnEnablePostfix(RoadEditorCollapsiblePanel __instance) {
            var btn = __instance.LabelButton;
            btn.isTooltipLocalized = false;
            if (__instance.LabelButton.text == "Props") {
                string tooltip = ". CTRL+Click for more options";
                if (!btn.tooltip.Contains(tooltip))
                    btn.tooltip += tooltip;
            }
        }
    }

    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    [HarmonyPatch("OnButtonClick")]
    internal static class RoadEditorCollapsiblePanelPatch2 {
        [HarmonyPrefix]
        static bool OnButtonClickPrefix(UIComponent component) {
            if (!HelpersExtensions.ControlIsPressed)
                return true;
            OnButtonControlClick(component);
            return false;
        }

        static void OnButtonControlClick(UIComponent component) {
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
            if (sidePanel != null && sidePanel.GetTarget() is NetLaneProps.Prop)
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

