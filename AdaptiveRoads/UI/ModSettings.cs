using AdaptiveRoads.Manager;
using AdaptiveRoads.Util;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using KianCommons;

namespace AdaptiveRoads.UI {
    public static class ModSettings {
        public const string FILE_NAME = nameof(AdaptiveRoads);
        public static SavedBool SavedBool(string key, bool def) => new SavedBool(key, FILE_NAME, def, true);

        public const string SEGMENT_VANILLA_NODE = "Segment.VanillaNode";
        public const string SEGMENT_SEGMENT_END = "Segment.SegmentEnd";
        public const string NODE_SEGMENT = "Node.Segment";
        public const string LANE_SEGMENT = "Lane.Segment";
        public const string LANE_SEGMENT_END = "Lane.SegmentEnd";
        public const string LANE_NODE = "Lane.Node";
        public const string AR_META_DATA = "ArMetaData";

        //public static readonly SavedBool InLineLaneInfo = SavedBool("InLineLaneInfo", true);
        //public static readonly SavedBool InLineLaneInfo = SavedBool("InLineLaneInfo", true);

        public static readonly SavedBool HideIrrelavant = SavedBool("HideIrrelavant", true);
        public static readonly SavedBool HideHints = SavedBool("HideHints", false);

        public static readonly SavedBool Segment_Node = SavedBool(SEGMENT_VANILLA_NODE, false);
        public static readonly SavedBool Segment_SegmentEnd = SavedBool(SEGMENT_SEGMENT_END, true);
        public static readonly SavedBool Node_Segment = SavedBool(NODE_SEGMENT, false);
        public static readonly SavedBool Lane_Segment = SavedBool(LANE_SEGMENT, true);
        public static readonly SavedBool Lane_SegmentEnd = SavedBool(LANE_SEGMENT_END, false);
        public static readonly SavedBool Lane_Node = SavedBool(LANE_NODE, false);

        public static readonly SavedBool ARMode = SavedBool(AR_META_DATA, true);
        public static bool VanillaMode => !ARMode;
        public static readonly SavedBool DefaultScale100 = SavedBool(AR_META_DATA, false);

        public static UICheckBox VanillaModeToggle;

        public static SavedBool GetOption(string key) {
            foreach (var field in typeof(ModSettings).GetFields()) {
                if (field.FieldType != typeof(SavedBool)) continue;
                SavedBool ret = field.GetValue(null) as SavedBool;
                if (ret.name == key)
                    return ret;
            }
            throw new System.ArgumentException($"option key:`{key}` does not exist.");
        }

        public static UICheckBox AddSavedToggle(this UIHelperBase helper, string label, SavedBool savedBool) {
            return helper.AddCheckbox(label, savedBool, delegate (bool value) {
                savedBool.value = value;
                Log.Debug($"option {label} is set to " + value);
                RoadEditorUtils.RefreshRoadEditor();
            }) as UICheckBox;
            Log.Debug($"option {label} is set to " + savedBool.value);
        }

        static ModSettings() {
            // Creating setting file - from SamsamTS
            if (GameSettings.FindSettingsFileByName(FILE_NAME) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FILE_NAME } });
            }
        }

        public static void OnSettingsUI(UIHelperBase helper) {
            var general = helper.AddGroup("General");
            VanillaModeToggle = general.AddCheckbox("Vanilla mode", !ARMode, delegate (bool vanillaMode) {
                if (ARMode == !vanillaMode) // happens after rejecting confirmation message
                    return; // no change is necessary
                if (vanillaMode)
                    OnConfimRemoveARdata(); // set to vanilla mode
                else
                    OnRefreshARMode(); // set to ARMode

            }) as UICheckBox;

            general.AddSavedToggle("hide irrelevant flags", HideIrrelavant);
            general.AddSavedToggle("hide floating hint box", HideHints);
            general.AddSavedToggle("Set default scale to 100", DefaultScale100);

            //var export = helper.AddGroup("import/export:");
            //export.AddButton("export edited road", null);
            //export.AddButton("import to edited road", null);

            var extensions = helper.AddGroup("UI components visible in asset editor:");
            var segment = extensions.AddGroup("Segment");
            segment.AddSavedToggle("Node flags", Segment_Node);
            segment.AddSavedToggle("Segment End flags", Segment_SegmentEnd);

            var node = extensions.AddGroup("Node");
            node.AddSavedToggle("Segment and Segment-extension flags", Node_Segment);

            var laneProp = extensions.AddGroup("Lane prop");
            laneProp.AddSavedToggle("Segment and Segment-extension flags", Lane_Segment);
            //laneProp.AddSavedToggle("Node flags", Lane_Node);
            laneProp.AddSavedToggle("Segment End flags", Lane_SegmentEnd);
        }

        public static void OnConfimRemoveARdata() {
            if (Helpers.InStartupMenu) {
                OnRefreshARMode();
                return;
            }

            string msg =
                "this operation removes all AR metada. " +
                "Are you sure you want to continue?";

            UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", CallbackFunc)
                .SetMessage("Remove AR Metadata?", msg);

            void CallbackFunc(UIComponent comp, int accepted) {
                if (accepted == 1) {
                    OnRefreshARMode();
                } else {
                    VanillaModeToggle.isChecked = false;
                }
            }
        }

        public static void OnRefreshARMode() {
            ARMode.value = !VanillaModeToggle.isChecked;
            Log.Debug($"Vanilla Mode toggle =  {VanillaModeToggle.isChecked}. ARMode = {ARMode.value}");

            if (Helpers.InStartupMenu)
                return;
            if (!ARMode)
                NetInfoExtionsion.UndoExtend_EditedNetInfos();
            else
                NetInfoExtionsion.EnsureExtended_EditedNetInfos();
            RoadEditorUtils.RefreshRoadEditor();
        }
    }
}
