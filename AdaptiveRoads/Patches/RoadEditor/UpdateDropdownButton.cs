namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using AdaptiveRoads.UI.RoadEditor.Bitmask;
    using AdaptiveRoads.Util;

    /// <summary>
    /// do not print mixed int the flags field.
    /// </summary>
    // private void REEnumBitmaskSet.UpdateDropdownButton()
    [HarmonyPatch(typeof(REEnumBitmaskSet), "UpdateDropdownButton")]
    public static class UpdateDropdownButton {
        public static bool Prefix(REEnumBitmaskSet __instance, FieldInfo ___m_TargetField) {
            try {
                Type fieldType = ___m_TargetField.FieldType;
                if (__instance.RequiresUserFlag(fieldType))
                    return true;

                int flags = __instance.GetFlags();
                var text = Enum.Format(fieldType, flags, "G");
                BitMaskPanelBase.ApplyText(__instance.m_DropDown, text);

                return false;
            }
            catch (Exception e){
                Log.Exception(e);
                return true;
            }
        }
    }
}

