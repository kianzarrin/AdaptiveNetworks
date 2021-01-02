namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.UI.RoadEditor;
    using ColossalFramework.UI;
    using HarmonyLib;

    /// <summary>
    /// Refresh all values.
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), nameof(RoadEditorPanel.Refresh))]
    public static class Refresh {
        public static void Postfix(RoadEditorPanel __instance) {
            foreach (UIComponent uicomponent in __instance.m_Container.components) {
                if (uicomponent is IDataUI dataUI) {
                    dataUI.Refresh();
                }
            }
        }
    }
}

