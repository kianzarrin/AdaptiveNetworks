namespace AdaptiveRoads.Patches {
    using AdaptiveRoads.Manager;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;
    using static Util.DPTHelpers;

    [HarmonyPatch]
    public static class DPT_Refresh {
        static MethodBase TargetMethod() => GetMethod(DPTType, "Refresh");
        static void Postfix(object __instance, object ___m_TargetElement, UIButton ___m_SelectButton) {
            try {
                string title = GetTitle(___m_TargetElement);
                if (!string.IsNullOrEmpty(title)) {
                    ___m_SelectButton.text = title;
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static string GetTitle(object obj) {
            if (obj is NetInfo.Node node) {
                return node.GetMetaData()?.Title;
            } else {
                return null;
            }
        }
    }
}

