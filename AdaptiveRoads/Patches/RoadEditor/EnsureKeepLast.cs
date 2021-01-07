namespace AdaptiveRoads.Patches.RoadEditor.x {
    using AdaptiveRoads.UI.RoadEditor;
    using HarmonyLib;

    /// <summary>
    /// add from template
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel), "EnsureKeepLast")]
    public static class EnsureKeepLast {
        public static void Postfix(RoadEditorCollapsiblePanel __instance) {
            var buttons = __instance.m_Panel.GetComponentsInChildren<EditorButon>();
            foreach (var button in buttons) {
                if (button.text.StartsWith("Add"))
                    button.zOrder = int.MaxValue;
            }
        }

    }
}

