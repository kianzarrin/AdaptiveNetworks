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
    [HarmonyPatch(typeof(RERefSet), "SelectReference")]
    public static class SelectReference {
        public static bool Prefix(RERefSet __instance, object ___m_Target) {
            var laneProp = ___m_Target as NetLaneProps.Prop;
            if (laneProp == null) return true;
            if (!HelpersExtensions.AltIsPressed) return true;

            var sidePanel = __instance.GetComponentInParent<RoadEditorPanel>();
            var panel = MiniPanel.Display();
            var field = panel.AddTextField();
            field.width = 200;
            var btn = panel.AddButton("load prop", null, delegate () {
                string name = field.text;
                var prop = PrefabCollection<PropInfo>.FindLoaded(name);
                var tree = PrefabCollection<TreeInfo>.FindLoaded(name);
                if (prop) {
                    laneProp.m_prop = laneProp.m_finalProp = prop;
                    __instance.OnReferenceSelected(prop);
                } else if (tree) {
                    laneProp.m_tree = laneProp.m_finalTree = tree;
                    __instance.OnReferenceSelected(tree);
                } else {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                      .SetMessage(
                          title: "no prop/tree with the name exists",
                          message: "",
                          false);
                    
                }
            });
            return false;
        }
    }
}


