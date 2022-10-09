namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using HarmonyLib;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using KianCommons;
    using ColossalFramework.UI;
    using System.Linq;
    using KianCommons.Patches;
    using NetworkSkins.GUI.UIFastList;
    using System.Reflection;
    using System.Reflection.Emit;
    using AdaptiveRoads.Util;
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

        [HarmonyPatch("PopulateModelList")]
        [HarmonyPostfix]
        public static void PopulateModelListPostfix(
            UIListBox ___m_ModelListbox,
            UIListBox ___m_RoadListbox,
            UIButton ___m_ImportButton,
            NetInfo[] ___m_FilteredRoads,
            ref RoadEditorCrossImportPanel.ModelWrapper[] ___m_FilteredModels) {
            NetInfo netInfo = ___m_FilteredRoads[___m_RoadListbox.selectedIndex];

            var infoExt = netInfo?.GetMetaData();
            if((infoExt?.Tracks).IsNullorEmpty()) return;

            var modelWrappers = new List<RoadEditorCrossImportPanel.ModelWrapper>(10);
            var listboxItems = new List<string>(10);
            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.m_material.shader ) {
                    var segmentInfo = new NetInfo.Segment {
                        m_material = trackInfo.m_material,
                        m_mesh = trackInfo.m_mesh,
                        m_lodMaterial = trackInfo.m_lodMaterial,
                        m_lodMesh = trackInfo.m_lodMesh,
                    };
                    var modelWrapper = new RoadEditorCrossImportPanel.ModelWrapper(segmentInfo);
                    modelWrappers.Add(modelWrapper);
                    listboxItems.Add(modelWrapper.Name + " (Track)");
                }
            }
            if(modelWrappers.Count == 0) return;

            ___m_FilteredModels = ___m_FilteredModels.Concat(modelWrappers).ToArray(); ;
            ___m_ModelListbox.items = ___m_ModelListbox.items.Concat(listboxItems).ToArray();

            if(___m_ModelListbox.items.Length > 0) {
                ___m_ModelListbox.selectedIndex = 0;
                ___m_ImportButton.Enable();
            }
        }

        [HarmonyPatch("PopulateRoadList")]
        [HarmonyPostfix]
        public static void PopulateRoadListPostfix(UIListBox ___m_RoadListbox) {
            Log.Called("SelectedInex=" + SelectedInex);
            if (SelectedInex < 0) return;
            if (___m_RoadListbox.items.Length > SelectedInex) {
                ___m_RoadListbox.selectedIndex = SelectedInex;
            } else {
                Log.Error($"Selected index is too large. " +
                    $"SelectedInex={SelectedInex} itemcount={___m_RoadListbox.items.Length} ");
            }
        }

        [HarmonyPatch("PopulateRoadList")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PopulateRoadListTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var instruction in instructions) {
                yield return instruction;
                if (instruction.Calls(nameof(FastList<NetInfo>.ToArray))) {
                    MethodInfo mSortRoads = typeof(RoadEditorCrossImportPanelPatch).GetMethod("SortRoads", throwOnError: true);
                    yield return new CodeInstruction(OpCodes.Call, mSortRoads);
                }
            }
        }

        public static int SelectedInex;
        public static NetInfo[] SortRoads(NetInfo[] roads) {
            Log.Called("input road count=" + roads.Length);
            var groups = RoadFamilyUtil.BuildFamilies(roads);

            HashSet<NetInfo> original = new HashSet<NetInfo>(roads);
            HashSet<NetInfo> result = new HashSet<NetInfo>();
            foreach (var family in groups) {
                foreach(NetInfo info in family) {
                    if (original.Contains(info)) {
                        result.Add(info);
                    }
                }
            }

            roads = result.ToArray();
            SelectedInex = roads.FindIndex(item => item == NetUtil.netTool.Prefab);
            Log.Info("output road count=" + roads.Length);
            return roads;
        }
    }
}
