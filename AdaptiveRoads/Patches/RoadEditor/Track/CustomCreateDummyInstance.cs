namespace AdaptiveRoads.Patches.RoadEditor.Track {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using System;
    using UnityEngine;

    [HarmonyPatch(typeof(AssetEditorRoadUtils), "CustomCreateDummyInstance")]
    class CustomCreateDummyInstance {
        public void Postfix(Type type, ref object obj, ref bool __result) {
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
        }
    }
}
