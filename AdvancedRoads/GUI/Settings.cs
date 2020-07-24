using ColossalFramework.UI;
using ICities;
using ColossalFramework;

namespace AdvancedRoads.GUI {
    using Tool;
    public static class Settings {
        public const string FileName = nameof(AdvancedRoads);
        static Settings() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(FileName) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FileName } });
            }
        }

        public static void OnSettingsUI(UIHelperBase helper) {
            UIHelper group = helper.AddGroup("Node Controller") as UIHelper;
            UIPanel panel = group.self as UIPanel;
            var keymappings = panel.gameObject.AddComponent<KeymappingsPanel>();
            keymappings.AddKeymapping("Activation Shortcut", AdvancedRoadsTool.ActivationShortcut);
        }
    }
}
