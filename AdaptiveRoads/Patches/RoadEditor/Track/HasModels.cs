namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using System;
    using HarmonyLib;
    using KianCommons;
    using AdaptiveRoads.Manager;
    using UnityEngine;

    [HarmonyPatch(typeof(RoadEditorCrossImportPanel), "HasModels")]
    public static class HasModels {
        static void Postfix(NetInfo info, ref bool __result) {
            try {
                var tracks = info.GetMetaData()?.Tracks;
                if(tracks != null) {
                    foreach(var track in tracks) {
                        if(track.m_material.shader) {
                            __result = true;
                            return;
                        }
                    }
                }
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
