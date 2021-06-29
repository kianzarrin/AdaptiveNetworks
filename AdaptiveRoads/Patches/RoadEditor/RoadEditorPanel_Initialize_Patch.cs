namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.UI.RoadEditor.Bitmask;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Assertion;
    using static KianCommons.ReflectionHelpers;
    using PrefabMetadata.API;


    /// <summary>
    /// enable vertical scrolling in sub panels
    /// </summary>
    // private void REEnumBitmaskSet.UpdateDropdownButton()
    [HarmonyPatch(typeof(RoadEditorPanel), nameof(RoadEditorPanel.Initialize))]
    public static class RoadEditorPanel_Initialize_Patch {
        public static void Postfix(RoadEditorPanel __instance) {
            try {
                __instance.m_Container.scrollWheelDirection = UIOrientation.Vertical;
            }
            catch (Exception e){
                Log.Exception(e);
            }
        }
    }
}

