namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using HarmonyLib;
    using KianCommons;
    using System;
    using AdaptiveRoads.Manager;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), nameof(AssetEditorRoadUtils.GetMeshName))]
    public static class GetMeshName {
        static void Postfix(object obj, ref string __result) {
            try {
                if(obj is NetInfoExtionsion.Track track) {
                    __result = track.m_mesh?.name;
                    if(__result.IsNullorEmpty())
                        __result = "New Track";
                } else if (obj is NetInfoExtionsion.TransitionProp prop) {
                    __result = prop.m_prop?.name ?? prop.m_tree?.name;
                    if (__result.IsNullorEmpty())
                        __result = "New Prop";
                }
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }

}
