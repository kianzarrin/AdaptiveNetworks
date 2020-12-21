using ColossalFramework.UI;
using ICities;
using ColossalFramework;

namespace AdaptiveRoads.UI {
    public static class ModSettings {
        public const string FILE_NAME = nameof(AdaptiveRoads);
        public static SavedBool SavedBool(string key, bool def) => new SavedBool(key, FILE_NAME, def, true);

        public const string SEGMENT_VANILLA_NODE = "Segment.VanillaNode";
        public const string SEGMENT_SEGMENT_END = "Segment.SegmentEnd";
        public const string NODE_SEGMENT= "Node.Segment";
        public const string LANE_SEGMENT= "Lane.Segment";
        public const string LANE_SEGMENT_END = "Lane.SegmentEnd";
        public const string LANE_NODE = "Lane.Node";

        //public static readonly SavedBool InLineLaneInfo = SavedBool("InLineLaneInfo", true);
        //public static readonly SavedBool InLineLaneInfo = SavedBool("InLineLaneInfo", true);

        public static readonly SavedBool HideIrrelavant = SavedBool("HideIrrelavant", true);

        public static readonly SavedBool Segment_Node = SavedBool(SEGMENT_VANILLA_NODE, false);
        public static readonly SavedBool Segment_SegmentEnd = SavedBool(SEGMENT_SEGMENT_END, true);
        public static readonly SavedBool Node_Segment = SavedBool(NODE_SEGMENT, false);
        public static readonly SavedBool Lane_Segment = SavedBool(LANE_SEGMENT, true);
        public static readonly SavedBool Lane_SegmentEnd = SavedBool(LANE_SEGMENT_END, false);
        public static readonly SavedBool Lane_Node = SavedBool(LANE_NODE, false);

        public static SavedBool GetOption(string key) {
            foreach(var field in typeof(ModSettings).GetFields()) {
                if (field.FieldType != typeof(SavedBool)) continue;
                SavedBool ret = field.GetValue(null) as SavedBool;
                if (ret.name == key)
                    return ret;
            }
            throw new System.ArgumentException($"option key:`{key}` does not exist.");
        }


        public static UICheckBox AddToggle(this UIHelperBase helper, string label, SavedBool savedBool) {
            return helper.AddCheckbox(label, savedBool, delegate(bool value) {
                savedBool.value = value;
            }) as UICheckBox;
        }



        static ModSettings() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(FILE_NAME) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FILE_NAME } });
            }
        }

        public static void OnSettingsUI(UIHelperBase helper) {
            var general = helper.AddGroup("General");
            general.AddToggle("hide irrelevant flags", HideIrrelavant);

            var extensions = helper.AddGroup("UI components visible in asset editor:");
            var segment = extensions.AddGroup("Segment");
            segment.AddToggle("Node flags", Segment_Node);
            segment.AddToggle("Segment End flags", Segment_SegmentEnd);

            var node = extensions.AddGroup("Node");
            node.AddToggle("Segment and Segment-extension flags", Node_Segment);

            var laneProp = extensions.AddGroup("Lane prop");
            laneProp.AddToggle("Segment and Segment-extension flags", Lane_Segment);
            laneProp.AddToggle("Node flags", Lane_Node);
            laneProp.AddToggle("Segment End flags", Lane_SegmentEnd);
        }
    }
}
