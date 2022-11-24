namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using KianCommons;
    using System;
    using UnityEngine;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), "CustomCreateDummyInstance")]
    static class CustomCreateDummyInstance {
        public static void Postfix(Type type, ref object obj, ref bool __result) {
            if(type == typeof(NetInfoExtionsion.Track)) {
                Shader shader = Shader.Find("Custom/Net/Road");
                var info = RoadEditorUtils.GetSelectedNetInfo(out var _);
                obj = new NetInfoExtionsion.Track(info) {
                    m_mesh = new Mesh(),
                    m_material = new Material(shader),
                    m_lodMesh = new Mesh(),
                    m_lodMaterial = new Material(shader)
                };
                __result = true;
            }
            if(obj is NetInfo.Node node) {
                node.m_tagsRequired = DynamicFlagsUtil.EMPTY_TAGS;
                node.m_tagsForbidden = DynamicFlagsUtil.EMPTY_TAGS;
                node.m_nodeTagsRequired = DynamicFlagsUtil.NONE;
                node.m_nodeTagsForbidden = DynamicFlagsUtil.NONE;
            }
        }
    }
}
