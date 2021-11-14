namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using System;
    using HarmonyLib;
    using KianCommons;
    using AdaptiveRoads.Manager;
    using UnityEngine;

    [HarmonyPatch(typeof(RoadEditorCrossImportPanel), "HasModels")]
    public static class HasModels {
        static void Postfix(NetInfo info) {
            try {
                if(__instance.m_Target is NetInfoExtionsion.Track track)
                    track.ReleaseModel();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
