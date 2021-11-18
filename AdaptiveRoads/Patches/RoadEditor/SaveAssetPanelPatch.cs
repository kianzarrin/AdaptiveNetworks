using AdaptiveRoads.UI.RoadEditor.MenuStyle;
using ColossalFramework.UI;
using HarmonyLib;
using KianCommons;
using System;
using UnityEngine;

namespace AdaptiveRoads.Patches.RoadEditor {
    [HarmonyPatch(typeof(SaveAssetPanel), "Refresh", new Type[] { })]
    public static class SaveAssetPanel_Refresh {
        public static void Postfix(SaveAssetPanel __instance, UITextField ___m_AssetName) {
            if (ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo) {
                Log.Debug("SaveAssetPanel.Refresh.Postfix() called");
                var parent = ___m_AssetName.parent;
                if (!parent.GetComponent<MenuButton>()) {
                    var renameButton = parent.AddUIComponent<MenuButton>();
                    renameButton.relativePosition =
                        ___m_AssetName.relativePosition -
                        new Vector3(0, 10); // move up by 10
                    renameButton.text = "Rename Road";
                    renameButton.eventClick += (_, __) => RenameRoadPanel.Display(__instance);
                    renameButton.eventVisibilityChanged += (_c, _val) => {
                        if (!_val) GameObject.Destroy(_c); // auto destroy
                    };
                }
                ___m_AssetName.enabled = false;
            } else {
                ___m_AssetName.enabled = true;
                ___m_AssetName.isVisible = __instance.component.isVisible;
            }

        }
        public static void Finalizer(Exception __exception) {
            if(__exception != null) {
                SimulationManager.instance.ForcedSimulationPaused = false;
                Log.Exception(__exception);
            }
        }
    }
}
