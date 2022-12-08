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
using AdaptiveRoads.UI.VBSTool;

namespace AdaptiveRoads.UI {
    public static class ModSettings {
        public const string FILE_NAME = nameof(AdaptiveRoads);
        public static SavedBool SavedBool(string key, bool def) => new SavedBool(key, FILE_NAME, def, true);
        public static SavedInt SavedInt(string key, int def) => new SavedInt(key, FILE_NAME, def, true);
        public static SavedFloat SavedFloat(string key, float def) => new SavedFloat(key, FILE_NAME, def, true);

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

        public static readonly SavedInt SpeedUnit = SavedInt("SpeedUnit", (int)SpeedUnitType.KPH);

        public static readonly SavedFloat QuayRoadsPanelX = SavedFloat("QuayRoadsPanelX", 87);
        public static readonly SavedFloat QuayRoadsPanelY = SavedFloat("QuayRoadsPanelY", 58);


        public static SavedInputKey Hotkey = new SavedInputKey(
            "AR_HotKey", FILE_NAME,
            key: KeyCode.A, control: true, shift: false, alt: true, true);

        public static SavedInputKey VBSHotkey = new SavedInputKey(
            "VBS_HotKey", FILE_NAME,
            key: KeyCode.V, control: true, shift: false, alt: true, true);

        #region ThinWires
        public static SavedBool ThinWires = SavedBool("ThinWires", false);
        public static SavedFloat WireScale = SavedFloat("WireScale", 3.5f);

        public static class RLWY {
            private static bool ModEnabled => PluginUtil.GetPlugin("RailwayMod", searchOptions: PluginUtil.AssemblyEquals).IsActive();
            private static SavedBool ThinWires => new SavedBool("enableWires", "RailwayModSettings", ModEnabled, true);
            public static bool UseThinWires =>
                (ModEnabled && ThinWires.value);
        }
        #endregion


        public static UICheckBox VanillaModeToggle;
        public static UIComponent WireScaleComponent;

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
            try {
                bool inAssetEditor = HelpersExtensions.InAssetEditor;

#if DEBUG
                {
                    helper.AddUpdatingCheckbox("Verbose log", val => Log.VERBOSE = val, () => Log.VERBOSE);
                    helper.AddUpdatingCheckbox("Buffered log", val => Log.Buffered = val, () => Log.Buffered);
                }
#endif
                var general = helper.AddGroup("General") as UIHelper;

                ANWhatsNew.Instance.AddSettings(general);

                var keymappingsPanel = general.AddKeymappingsPanel();
                keymappingsPanel.AddKeymapping("Hotkey", Hotkey);

                if (inAssetEditor || Helpers.InStartupMenu) {
                    keymappingsPanel.AddKeymapping("VBS Hotkey", VBSHotkey);

                    VanillaModeToggle = general.AddCheckbox("Vanilla mode", !ARMode, delegate (bool vanillaMode) {
                        if (ARMode == !vanillaMode) // happens after rejecting confirmation message
                            return; // no change is necessary
                        if (vanillaMode)
                            OnConfimRemoveARdata(); // set to vanilla mode
                        else
                            OnRefreshARMode(); // set to ARMode

                    }) as UICheckBox;
                }

                {
                    //thin wires
                    general.AddSavedToggle("Use all thin wires globally", ThinWires, val => {
                        if (WireScaleComponent != null) {
                            WireScaleComponent.parent.isVisible = val;
                        }
                        if (!Helpers.InStartupMenu) {
                            RoadUtils.SetupThinWires(force: true);
                        }
                    }).tooltip = "applies to all networks (not only AN networks)";
                    WireScaleComponent = general.AddSlider(
                        text: $"wire width: 1/{WireScale}" ,
                        min: 1, max: 10, step: 0.1f,
                        defaultValue: WireScale,
                        val => {
                            WireScale.value = val;
                            Log.Info("wire scale changed to " + val);
                            var label = WireScaleComponent.parent.Find<UILabel>("Label");
                            label.text = $"wire width: 1/{val}";
                        }) as UIComponent;
                    WireScaleComponent.parent.isVisible = ThinWires.value;
                    WireScaleComponent.eventMouseUp += (_, __) => RoadUtils.SetupThinWires(); // on slider released

                    if (!Helpers.InStartupMenu) {
                        var toggle = general.AddCheckbox("Left Hand Traffic", NetUtil.LHT, RoadUtils.SetDirection) as UICheckBox;
                        toggle.eventVisibilityChanged += (_, __) => toggle.isChecked = NetUtil.LHT;
                    }
                }

                if (inAssetEditor) {
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
                }

                if (!Helpers.InStartupMenu) {
#if DEBUG
                    general.AddButton("Rebuild lods", () => NetManager.instance.RebuildLods());
#endif
                    general.AddButton("Refresh AN networks", RefreshARNetworks);
                }

                if (inAssetEditor) {
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
            } catch (Exception ex) {
                ex.Log();
            }

        }

        public static void OnConfimRemoveARdata() {
            if (Helpers.InStartupMenu) {
                OnRefreshARMode();
                return;
            }

            string msg =
                "this operation removes all AN metadata. " +
                "Are you sure you want to continue?";

            UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", CallbackFunc)
                .SetMessage("Remove AN Metadata?", msg);

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
            NetInfoExtionsion.Ensure_EditedNetInfos(recalculate:true);
            RoadEditorUtils.RefreshRoadEditor();
        }

        public static void RefreshARNetworks() =>
            NetworkExtensionManager.RawInstance?.UpdateAllNetworkFlags();
        
    }
}
