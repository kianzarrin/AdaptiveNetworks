namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.UI.RoadEditor.MenuStyle;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
    using System.Linq;
    using System.Reflection;
    using AdaptiveRoads.Util;

    /// <summary>
    /// add from template
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), "CreateArrayField")]
    public static class CreateArrayField {
        public static void Postfix(
            string name, FieldInfo field, object target,
            RoadEditorPanel __instance, object ___m_Target) {
            target ??= ___m_Target;
            RoadEditorCollapsiblePanel groupPanel = __instance.GetGroupPanel(name);
            if (name == "Props") {
                Log.Debug("creating `Add from template` button");
                var button = groupPanel.m_Panel.AddUIComponent<EditorButon>();
                button.zOrder = int.MaxValue;
                button.text = "Add from template";
                button.width = 200;
                button.eventClicked += OnLoadFromPropTempalteClicked;
            }
            if (name == "Nodes") {
                Log.Debug("creating `Add from template` button");
                var button = groupPanel.m_Panel.AddUIComponent<EditorButon>();
                button.zOrder = int.MaxValue;
                button.text = "Add from template";
                button.width = 200;
                button.eventClicked += OnLoadFromNodeTempalteClicked;
            }

        }

        public static void OnLoadFromPropTempalteClicked(
            UIComponent component, UIMouseEventParameter eventParam) {
            try {
                var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
                var roadEditor = component.GetComponentInParent<RoadEditorPanel>();
                var lane = roadEditor.GetTarget() as NetInfo.Lane;
                Assertion.AssertNotNull(lane,"target is lane");
                bool unidirectional = lane.IsGoingForward() || lane.IsGoingBackward();
                bool suggestBackward = lane.m_laneType == NetInfo.LaneType.Pedestrian && lane.m_position < 0;
                LoadPropTemplatePanel.Display(
                    loadedProps => RoadEditorUtils.AddProps(groupPanel, loadedProps),
                    unidirectional:unidirectional,
                    suggestBackward: suggestBackward);
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void OnLoadFromNodeTempalteClicked(
            UIComponent component, UIMouseEventParameter eventParam) {
            try {
                var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
                var roadEditor = component.GetComponentInParent<RoadEditorPanel>();
                var netInfo = roadEditor.GetTarget() as NetInfo;
                Assertion.AssertNotNull(netInfo, "target is netInfo");
                LoadNodeTemplatePanel.Display(
                    loadedNodes => RoadEditorUtils.AddNodes(groupPanel, loadedNodes));
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }


    }
}

