using ColossalFramework.UI;
using ICities;
using ColossalFramework;

namespace AdvancedRoads.UI {
    public static class ModSettings {
        public const string FILE_NAME = nameof(AdvancedRoads);

        public static readonly SavedBool InLineLaneInfo = new SavedBool(
            "InLineLaneInfo",
            FILE_NAME,
            true,
            true);

        static ModSettings() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(FILE_NAME) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FILE_NAME } });
            }
        }

        public static void OnSettingsUI(UIHelperBase helper) {
            UIHelper group = helper.AddGroup("Network Detective") as UIHelper;
            UIPanel panel = group.self as UIPanel;

            //var keymappings = panel.gameObject.AddComponent<UIKeymappingsPanel>();
            //keymappings.AddKeymapping("Activation Shortcut", );
        }
    }
}
