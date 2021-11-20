namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using HarmonyLib;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using KianCommons;
    using ColossalFramework.UI;
    using System;

    [HarmonyPatch(typeof(RoadEditorCrossImportPanel))]
    public static class RoadEditorCrossImportPanelPatch {
        [HarmonyPatch("HasModels")]
        [HarmonyPostfix]
        public static void HasModelsPostfix(NetInfo info, ref bool __result) {
            if(__result) return;

            var infoExt = info?.GetMetaData();
            if(infoExt == null) return;

            foreach(var track in infoExt.Tracks) {
                if(track.m_material?.shader) {
                    __result = true;
                    return;
                }
            }
        }

        public static HashSet<NetInfo.Segment> TrackModels = new HashSet<NetInfo.Segment>();
        [HarmonyPatch("PopulateModelList")]
        [HarmonyPostfix]
        public static void PopulateModelListPostfix(
            UIListBox ___m_ModelListbox,
            UIListBox ___m_RoadListbox,
            UIButton ___m_ImportButton,
            ref NetInfo[] ___m_FilteredRoads,
            ref RoadEditorCrossImportPanel.ModelWrapper[] ___m_FilteredModels) {
            NetInfo netInfo = ___m_FilteredRoads[___m_RoadListbox.selectedIndex];

            var infoExt = netInfo?.GetMetaData();
            if((infoExt?.Tracks).IsNullorEmpty()) return;

            TrackModels.Clear();
            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.m_material.shader ) {
                    var segmentInfo = new NetInfo.Segment {
                        m_material = trackInfo.m_material,
                        m_mesh = trackInfo.m_mesh,
                        m_lodMaterial = trackInfo.m_lodMaterial,
                        m_lodMesh = trackInfo.m_lodMesh,
                    };
                    TrackModels.Add(segmentInfo);
                    ___m_FilteredModels.AddItem(new RoadEditorCrossImportPanel.ModelWrapper(segmentInfo));
                }
            }

            var oldItems = ___m_ModelListbox.items;
            ___m_ModelListbox.items = new string[___m_FilteredModels.Length+TrackModels.Count];
            Array.Copy(oldItems, ___m_ModelListbox.items, oldItems.Length);
            for(int i = oldItems.Length; i < ___m_ModelListbox.items.Length; ++i) {
                var trackWrapper = ___m_FilteredModels[i];
                ___m_ModelListbox.items[i] = trackWrapper.Name + " (Track)";
            }

            if(___m_ModelListbox.items.Length > 0) {
                ___m_ModelListbox.selectedIndex = 0;
                ___m_ImportButton.Enable();
            }
        }

    }
}
