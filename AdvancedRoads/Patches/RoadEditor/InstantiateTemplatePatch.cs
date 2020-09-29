namespace AdvancedRoads.Patches.RoadEditor {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using KianCommons;
    using System;
    //using Log = KianCommons.Log;
    using KianCommons.Patches;
    using System.Reflection;
    using ColossalFramework.UI;

    // ColossalFramework.UI.UITemplateManager
    // private UIComponent Instantiate(string name)
    //[HarmonyPatch(typeof(UITemplateManager))]
    //public static class InstantiateTemplatePatch {
    //    [HarmonyPatch("Instantiate")]
    //    static void Postfix(string name, UIComponent __result) {
    //        if(name== RoadEditorPanel.kSidePanel) {
    //            __result.width += 200; 
    //        }
    //    }
    //}
}

