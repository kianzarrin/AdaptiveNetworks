namespace AdaptiveRoads.Patches.Track {
    using HarmonyLib;
    using UnityEngine;
    using AdaptiveRoads.Util;
    using AdaptiveRoads.Manager;

    /// <summary>
    /// changing types confuses OnImportFinished when importing node/segment models.
    /// this patch resolves that confusion by using the replaced types.
    /// TODO: move this patch to prefab indeces mod. (except for track)
    /// </summary>
    [HarmonyPatch(typeof(REModelCrossImportSet), "OnImportFinished")]
    public static class OnImportFinished {
        public static void Postfix(REModelCrossImportSet __instance, object ___m_Target,
            Mesh mesh, Mesh lodMesh, Material material, Material lodMaterial) {
            if(___m_Target is NetInfoExtionsion.Track track) {
                track.m_mesh = mesh;
                track.m_lodMesh = lodMesh;
                track.m_material = material;
                track.m_lodMaterial = lodMaterial;
                __instance.OnPropertyChanged();
            }
        }
    }
}