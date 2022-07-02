namespace AdaptiveRoads.UI {
    using ColossalFramework;
    using KianCommons.UI.MessageBox.WhatsNew;
    using System;

    internal class ANWhatsNew : WhatsNew<ANWhatsNew> {
        public override string ModName { get; } = "Adaptive Networks";
        public override SavedString SavedVersion => new SavedString("WhatsNewVersion", ModSettings.FILE_NAME, def: "0.0.0", autoUpdate: true);
        public override WhatsNewEntry[] Messages { get; } = new WhatsNewEntry[] {
            new WhatsNewEntry {
                version = new Version(3, 8, 5),
                messages = new string[] {
                    "Feature: Export nodes to xml templates",
                    "Hint: display summary hintbox when you hover over nodes",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 8, 4),
                messages = new string[] {
                    "Fixed: direction of track mesh",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 8),
                messages = new string[] {
                    "Feature: custom selectors for segments (its like having more custom flags)",
                    "Feature: DC nodes/tracks can now use asym forward/backward flags on intersections (it won't show on network detective because its between two lanes/segments and not the whole node).",
                    "Feature: Support for surface/asphalt models in tracks",
                    "Fixed: support new LSM mod (track textures can now be cached again)",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 7),
                messages = new string[] {
                    "Feature: bike tracks",
                    "Fixed: tracks support IMT (markups rendered under the tracks)",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 6),
                messages = new string[] {
                    "Feature: Sharp corners",
                    "Pillars now shift if road is shifted",
                }
            },


            new WhatsNewEntry {
                version = WhatsNewEntry.PriorVersion,
                messages = new string[] {
                    "Feature: reorder everything using drag and drop (props/lanes/...)",
                    "Hint: you can show/hide AN features from options.",
                    "Feature: Tiling",
                    "Feature: Tracks/Tilt",
                    "Feature: Quay roads",
                    "Feature: Expressions"
                }
            },

        };
    }
}

