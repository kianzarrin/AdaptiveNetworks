namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using HarmonyLib;
    using KianCommons;
    using System;
    using AdaptiveRoads.Manager;
    using UnityEngine;

    [HarmonyPatch(typeof(REModelImport), "OnModelImported")]
    public static class OnModelImported {
        static void Prefix(REModelImport __instance) {
            try {
                if(__instance.m_Target is NetInfoExtionsion.Track track)
                    track.ReleaseModel();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}