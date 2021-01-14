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
            target = target ?? ___m_Target;
            RoadEditorCollapsiblePanel groupPanel = __instance.GetGroupPanel(name);
            if (name == "Props") {
                Log.Debug("creating `Add from template` button");
                var button = groupPanel.m_Panel.AddUIComponent<EditorButon>();
                button.zOrder = int.MaxValue;
                button.text = "Add from template";
                button.width = 200;
                button.eventClicked += OnLoadFromTempalteClicked;
            }
        }

        public static void OnLoadFromTempalteClicked(
            UIComponent component, UIMouseEventParameter eventParam) {
            try {
                var groupPanel = component.GetComponentInParent<RoadEditorCollapsiblePanel>();
                LoadTemplatePanel.Display(loadedProps => RoadEditorUtils.AddProps(groupPanel, loadedProps));
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }


    }
}

