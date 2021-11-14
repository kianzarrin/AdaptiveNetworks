namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using HarmonyLib;
    using KianCommons;
    using System;
    using AdaptiveRoads.Manager;
    using UnityEngine;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), nameof(AssetEditorRoadUtils.GetShaderName))]
    public static class GetMaterial {
        static void Postfix(object obj, ref Material __result) {
            try {
                if(obj is NetInfoExtionsion.Track track)
                    __result = track.m_material;
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
