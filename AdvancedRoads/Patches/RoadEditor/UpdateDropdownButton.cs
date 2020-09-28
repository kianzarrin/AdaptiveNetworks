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
            try {
                Type fieldType = __instance.m_TargetField().FieldType;
                if (__instance.RequiresUserFlag(fieldType))
                    return true;

                int flags = __instance.GetFlags();
                UIButton uibutton = (UIButton)__instance.m_DropDown.triggerButton;
                uibutton.text = Enum.Format(__instance.m_TargetField().FieldType, flags, "G");
                return false;
            }
            catch (Exception e){
                Log.Error(e.ToString());
                return true;
            }
        }
    }
}

