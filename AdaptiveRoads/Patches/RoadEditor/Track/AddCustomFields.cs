namespace AdaptiveRoads.Patches.RoadEditor {
    using PrefabMetadata.API;
    using System;
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using AdaptiveRoads.Manager;


    /// <summary>
    /// handle track
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel))]
    public static class AddCustomFields {
        [HarmonyPatch("AddCustomFields")]
        public static void Postfix(RoadEditorPanel __instance, object ___m_Target) {
            if (___m_Target == null) throw new ArgumentNullException("___m_Target");
            if (___m_Target is NetInfoExtionsion.Track) {
                // __instance.AddCrossImportField(); // TODO: add once this is supported
                __instance.AddModelImportField(true);
            } 
        }
    }
}

