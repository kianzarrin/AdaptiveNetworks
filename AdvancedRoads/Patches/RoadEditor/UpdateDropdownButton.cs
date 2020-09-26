namespace AdvancedRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;

    /// <summary>
    /// do not print mixed int the flags field.
    /// </summary>
    // private void REEnumBitmaskSet.UpdateDropdownButton()
    [HarmonyPatch(typeof(REEnumBitmaskSet), "UpdateDropdownButton")]
    public static class UpdateDropdownButton {
        public static bool Prefix(REEnumBitmaskSet __instance) {
            int flags = __instance.GetFlags();
            UIButton uibutton = (UIButton)__instance.m_DropDown.triggerButton;
            if (flags == 0) {
                uibutton.text = "None";
            } else {
                Type fieldType = __instance.m_TargetField().FieldType;
                if (__instance.RequiresUserFlag(fieldType)) {
                    uibutton.text = __instance.GetUserFlagName(flags);
                } else {
                    uibutton.text = Enum.GetName(__instance.m_TargetField().FieldType, flags);
                }
            }
            return false;
        }
    }
}

