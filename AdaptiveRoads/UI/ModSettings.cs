using AdaptiveRoads.Manager;
using AdaptiveRoads.Util;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using KianCommons;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using static KianCommons.EnumBitMaskExtensions;
using KianCommons.Plugins;
using UnityEngine;
using KianCommons.UI;

namespace AdaptiveRoads.UI {
    public static class ModSettings {
        public const string FILE_NAME = nameof(AdaptiveRoads);
        public static SavedBool SavedBool(string key, bool def) => new SavedBool(key, FILE_NAME, def, true);
        public static SavedInt SavedInt(string key, int def) => new SavedInt(key, FILE_NAME, def, true);
        public static SavedFloat SavedFloat(string key, float def) => new SavedFloat(key, FILE_NAME, def, true);

        public static bool RailwayModEnabled => PluginUtil.GetPlugin("RailwayMod", searchOptions: PluginUtil.AssemblyEquals).IsActive();
        public static SavedBool ThinWires => new SavedBool("enableWires", "RailwayModSettings", RailwayModEnabled, true);
        public enum SpeedUnitType { KPH, MPH }

        public const string SEGMENT_NODE = "Segment.Node";
        public const string SEGMENT_SEGMENT_END = "Segment.SegmentEnd";
        public const string NODE_SEGMENT = "Node.Segment";
        public const string LANE_SEGMENT = "Lane.Segment";
        public const string LANE_SEGMENT_END = "Lane.SegmentEnd";
        public const string AR_MODE = "AR_MODE";

        //public static readonly SavedBool InLineLaneInfo = SavedBool("InLineLaneInfo", true);
        //public static readonly SavedBool InLineLaneInfo = SavedBool("InLineLaneInfo", true);

        public static readonly SavedBool HideIrrelavant = SavedBool("HideIrrelavant", true);
        public static readonly SavedBool HideHints = SavedBool("HideHints", false);

        public static readonly SavedBool Segment_Node = SavedBool(SEGMENT_NODE, false);
        public static readonly SavedBool Segment_SegmentEnd = SavedBool(SEGMENT_SEGMENT_END, true);
        public static readonly SavedBool Node_Segment = SavedBool(NODE_SEGMENT, false);
        public static readonly SavedBool Lane_Segment = SavedBool(LANE_SEGMENT, true);
        public static readonly SavedBool Lane_SegmentEnd = SavedBool(LANE_SEGMENT_END, false);

        public static readonly SavedBool ARMode = SavedBool(AR_MODE, true);
        public static bool VanillaMode => !ARMode;
        public static readonly SavedBool DefaultScale100 = SavedBool("DefaultScale100", false);

        public static readonly SavedInt SpeedUnit = SavedInt("SpeedUnit", (int)SpeedUnitType.KPH);

        public static readonly SavedFloat QuayRoadsPanelX = SavedFloat("QuayRoadsPanelX", 87);
        public static readonly SavedFloat QuayRoadsPanelY = SavedFloat("QuayRoadsPanelY", 58);


        public static SavedInputKey Hotkey = new SavedInputKey(
            "AR_HotKey", FILE_NAME,
            key: KeyCode.A, control: true, shift: false, alt: true, true);

        public static UICheckBox VanillaModeToggle;

        public static SavedBool GetOption(string key) {
            foreach (var field in typeof(ModSettings).GetFields()) {
                if (field.FieldType != typeof(SavedBool)) continue;
                SavedBool ret = field.GetValue(null) as SavedBool;
                if (ret.name == key)
                    return ret;
            }
            throw new ArgumentException($"option key:`{key}` does not exist.");
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
            bool inAssetEditor = HelpersExtensions.InAssetEditor;

            var general = helper.AddGroup("General") as UIHelper;

            var keymappingsPanel = general.AddKeymappingsPanel();
            keymappingsPanel.AddKeymapping("Hotkey", Hotkey);

            if(inAssetEditor || Helpers.InStartupMenu) {
                VanillaModeToggle = general.AddCheckbox("Vanilla mode", !ARMode, delegate (bool vanillaMode) {
                    if(ARMode == !vanillaMode) // happens after rejecting confirmation message
                        return; // no change is necessary
                    if(vanillaMode)
                        OnConfimRemoveARdata(); // set to vanilla mode
                    else
                        OnRefreshARMode(); // set to ARMode

                }) as UICheckBox;
            }

            if (!Helpers.InStartupMenu) {
                var btn = general.AddCheckbox("Left Hand Traffic", NetUtil.LHT, RoadUtils.SetDirection) as UICheckBox;
                btn.eventVisibilityChanged += (_, __) => btn.isChecked = NetUtil.LHT;
            }

            if(inAssetEditor) { 
                var dd = general.AddDropdown(
                    "preferred speed unit",
                    Enum.GetNames(typeof(SpeedUnitType)),
                    0, // kph
                    sel => {
                        var value = GetEnumValues<SpeedUnitType>()[sel];
                        SpeedUnit.value = (int)value;
                        Log.Debug("option 'preferred speed unit' is set to " + value);
                        RoadEditorUtils.RefreshRoadEditor();
                    });

                general.AddSavedToggle("hide irrelevant flags", HideIrrelavant);
                general.AddSavedToggle("hide floating hint box", HideHints);
                general.AddSavedToggle("Set default scale to 100", DefaultScale100);
            }

            if(!Helpers.InStartupMenu) {
                general.AddButton("Refresh AR networks", RefreshARNetworks);
            }

            if(inAssetEditor) {
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
                laneProp.AddSavedToggle("Segment End flags", Lane_SegmentEnd);
            }

        }

        public static void OnConfimRemoveARdata() {
            if (Helpers.InStartupMenu) {
                OnRefreshARMode();
                return;
            }

            string msg =
                "this operation removes all AR metadata. " +
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
            NetInfoExtionsion.Ensure_EditedNetInfos();
            RoadEditorUtils.RefreshRoadEditor();
        }

        public static void RefreshARNetworks() =>
            NetworkExtensionManager.RawInstance?.UpdateAllNetworkFlags();
        
    }
}
