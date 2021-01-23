namespace AdaptiveRoads.Patches.metadata {
    using HarmonyLib;
    using UnityEngine;
    using System;

    /// <summary>
    /// changeing types confuses OnImportFinished when importing node/segment models.
    /// this patch resolves that confusion by using the replaced types.
    /// TODO move this pacth to prefab indeces mod.
    /// </summary>
    [HarmonyPatch(typeof(REModelCrossImportSet), "OnImportFinished")]
    public static class OnImportFinished {
        public static void Prefix(REModelCrossImportSet __instance, object ___m_Target,
            Mesh mesh, Mesh lodMesh, Material material, Material lodMaterial) {
            if (___m_Target == null) throw new ArgumentNullException("___m_Target");
            if (___m_Target is NetInfo.Segment segment) {
                segment.m_mesh = mesh;
                segment.m_lodMesh = lodMesh;
                segment.m_material = material;
                segment.m_lodMaterial = lodMaterial;
            } else if (___m_Target is NetInfo.Node node) {
                node.m_mesh = mesh;
                node.m_lodMesh = lodMesh;
                node.m_material = material;
                node.m_lodMaterial = lodMaterial;
            }

        }
    }
}