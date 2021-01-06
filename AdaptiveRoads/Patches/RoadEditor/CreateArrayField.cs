namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.UI.RoadEditor.Templates;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
    using System.Linq;
    using System.Reflection;

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
                LoadTemplatePanel.Display(loadedProps => AddProps(groupPanel, loadedProps));
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void AddProps(
            RoadEditorCollapsiblePanel groupPanel,
            NetLaneProps.Prop[] props) {
            try {
                Log.Debug("AddProps called");
                if (props == null || props.Length == 0) return;
                NetLaneProps.Prop[] m_props = groupPanel.GetArray() as NetLaneProps.Prop[];
                props = props.Select(_p => _p.Extend().Base).ToArray();
                var m_props2 = m_props.AddRangeToArray(props);

                var roadEditor = groupPanel.component.GetComponentInParent<RoadEditorPanel>();
                var sidePanel = roadEditor.GetSidePanel();
                var arrayField = groupPanel.GetField();
                var target = roadEditor.GetTarget();


                Log.Debug($"Adding props {props.Length}+{m_props.Length}={m_props2.Length}");
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

