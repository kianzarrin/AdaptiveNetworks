namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;


    /// <summary>
    /// enable vertical scrolling in sub panels
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), nameof(RoadEditorPanel.Initialize))]
    public static class RoadEditorPanel_Initialize_Patch {
        public static void Postfix(RoadEditorPanel __instance) {
            try {
                __instance.m_Container.scrollWheelDirection = UIOrientation.Vertical;
            } catch (Exception e) {
                Log.Exception(e);
            }
        }
    }
}

