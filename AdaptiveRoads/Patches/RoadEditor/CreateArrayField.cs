namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI;
    using AdaptiveRoads.UI.RoadEditor;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using System.Reflection;
    using static KianCommons.Assertion;
    using static KianCommons.ReflectionHelpers;

    /// <summary>
    /// TODO: add extra options when user right clicks the group label.
    /// </summary>
    //[HarmonyPatch(typeof(RoadEditorPanel), "CreateArrayField")]
    public static class CreateArrayField {
        public static void Postfix(
            string name, FieldInfo field, object target,
            RoadEditorPanel __instance, object ___m_Target) {
            target = target ?? ___m_Target;
            RoadEditorCollapsiblePanel groupPanel = __instance.GetGroupPanel(name);
        }
    }
}

