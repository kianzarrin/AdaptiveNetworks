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
    using AdaptiveRoads.Util;
    using UnityEngine;

    /// <summary>
    /// add hints
    /// </summary>
    [HarmonyPatch(typeof(REPropertySet), nameof(REPropertySet.SetTarget))]
    public static class REPropertySet_SetTarget {
        public static void Postfix(REPropertySet __instance, FieldInfo targetField, object target) {
            if(__instance is REEnumSet enumSet) {
                Type enumType = targetField.FieldType;
                var enumType2 = HintExtension.GetMappedEnumWithHints(enumType);
                if (HasHint(enumType2)) {
                    var panel = enumSet.GetComponentInChildren<UIPanel>();
                    panel.objectUserData = new EnumSetHint(enumSet);
                }
            }
        }
        static bool HasHint(Type enumType) {
            if (enumType.HasAttribute<HintAttribute>())
                return true;
            return enumType.GetMembers().Any(mbox => mbox.HasAttribute<HintAttribute>());
        }
    }
}

