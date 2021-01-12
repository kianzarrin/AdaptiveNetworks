using AdaptiveRoads.UI.RoadEditor.Templates;
using ColossalFramework.UI;
using HarmonyLib;
using KianCommons;
using System;
using UnityEngine;
using System.Linq;
using KianCommons;

namespace AdaptiveRoads.Patches.RoadEditor {
    [HarmonyPatch(typeof(SaveAssetPanel), "Refresh", new Type[] { })]
    public static class SaveAssetPanel_Refresh {
        public static void Postfix(SaveAssetPanel __instance, UITextField ___m_AssetName) {
            Log.Debug("SaveAssetPanel.Refresh.Postfix() called");
            var parent = ___m_AssetName.parent;
            if (!parent.GetComponent<MenuButton>()) {
                var c2 = parent.GetComponent(typeof(MenuButton).FullName);
                if (c2) {
                    GameObject.DestroyImmediate(c2.gameObject); // for hot-reload.
                    c2 = parent.GetComponent(typeof(MenuButton).FullName);
                    if (c2)
                        Log.Debug("failed to destroy old button");
                    else
                        Log.Debug("sucessfully deleted old button");

                }

                var renameButton = parent.AddUIComponent<MenuButton>();
                renameButton.relativePosition =
                    ___m_AssetName.relativePosition -
                    new Vector3(0,10); // mvoe up by 10
                renameButton.text = "Rename Road";
                renameButton.eventClick += (_, __) => RenameRoadPanel.Display(__instance);
                ___m_AssetName.enabled = false;
            }

        }
        public static void Finalizer(Exception __exception) {
            SimulationManager.instance.ForcedSimulationPaused = false;
            if (__exception != null)
                Log.Exception(__exception);
        }
    }
}
