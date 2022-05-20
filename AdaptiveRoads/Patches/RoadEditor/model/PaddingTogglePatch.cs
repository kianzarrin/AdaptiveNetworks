namespace AdaptiveRoads.Patches.RoadEditor.model {
    using AdaptiveRoads.UI.RoadEditor.MenuStyle;
    using ColossalFramework;
    using ColossalFramework.UI;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using UnityEngine;

    [HarmonyPatch(typeof(AssetImporterAssetImport), "SetDefaultScale")]
    public static class PaddingTogglePatch {
        public static SavedBool PadLod = new SavedBool("pad lod", AdaptiveRoads.UI.ModSettings.FILE_NAME, def: true, autoUpdate: true);

        [UsedImplicitly]
        static void Postfix(UITextField ___m_Scale) {
            Log.Called();
            var panel = ___m_Scale.parent as UIPanel;
            string name = "autopad lod";
            if (!panel.Find<MenuCheckbox>(name)) {
                var oldToggle = panel.Find(name);
                GameObject.Destroy(oldToggle?.gameObject); // hot reload

                var toggle = panel.AddUIComponent<MenuCheckbox>();
                toggle.name = name;
                toggle.Label = "autopad lod";
                toggle.tooltip = "turn off for importing dumped textures.";
                toggle.isChecked = PadLod.value;
                toggle.eventCheckChanged += CheckChanged;
                toggle.relativePosition = new UnityEngine.Vector2(toggle.relativePosition.x, 0);
            } 
        } 


        private static void CheckChanged(UIComponent component, bool value) {
            PadLod.value = value;
        }
    }
}