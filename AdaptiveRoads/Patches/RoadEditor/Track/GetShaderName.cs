namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using HarmonyLib;
    using KianCommons;
    using System;
    using AdaptiveRoads.Manager;

    /// <summary>
    /// RefreshAssetListpatch has files with relative paths.
    /// this patch fixes filename and path.
    /// </summary>
    [HarmonyPatch(typeof(AssetEditorRoadUtils), nameof(AssetEditorRoadUtils.GetShaderName))]
    public static class GetShaderName {
        static void Postfix(object obj, ref string __result) {
            try {
                if(obj is NetInfoExtionsion.Track track)
                    __result = track.m_material.shader.name ?? "";
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }

}
