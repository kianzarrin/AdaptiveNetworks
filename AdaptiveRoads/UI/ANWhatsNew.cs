namespace AdaptiveRoads.UI {
    using ColossalFramework;
    using KianCommons.UI.MessageBox.WhatsNew;
    using System;

    internal class ANWhatsNew : WhatsNew<ANWhatsNew> {
        public override string ModName { get; } = "Adaptive Networks";
        public override SavedString SavedVersion => new SavedString("WhatsNewVersion", ModSettings.FILE_NAME, def: "0.0.0", autoUpdate: true);
        public override WhatsNewEntry[] Messages { get; } = new WhatsNewEntry[] {
            new WhatsNewEntry {
                version = new Version(3, 16, 17),
                messages = new string[] {
                    "Feature: Track template",
                }
            },
            new WhatsNewEntry {
                version = new Version(3, 16, 16),
                messages = new string[] {
                    "Feature: Transition Prop Template",
                    "Bug-fix: cleanup destroyed selector UI",
                }
            },
            new WhatsNewEntry {
                version = new Version(3, 16, 14),
                messages = new string[] {
                    "Feature: Track connect groups",
                    "Feature: Global in game thin wire slider",
                    "Bug-fix: track lane tags",
                    "Bug-fix: track transition prop position.x",
                    "Bug-fix: track transition prop upgrade trees",
                    "Bug-fix: null version",
                    "Bug-fix: backward compatibility with tags",
                }
            },
            new WhatsNewEntry {
                version = new Version(3, 16, 8),
                messages = new string[] {
                    "Feature: Has Unbroken median flags for node and transition",
                    "Bug-fix: blue void on load",
                    "Bug-fix: Custom Connect Group matching"
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 16, 4),
                messages = new string[] {
                    "Feature: Prop junction distance (prevents repeating props from getting too close to the junctions)",
                    "Feature: Forbid Any Tags (forbids all other tags)" ,
                    "Bug-fix: Lane Tags"
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 15, 2),
                messages = new string[] {
                    "Feature: new lane transition flag for tracks: Near-Curb, Wind-Wires",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 14, 2),
                messages = new string[] {
                    "Feature: prop seed",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 14, 1),
                messages = new string[] {
                    "Feature: jump to selected road in model cross import panel",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 14, 0),
                messages = new string[] {
                    "Feature: add/remove elevations",
                    "Feature: change AI",
                }
            },


            new WhatsNewEntry {
                version = new Version(3, 13, 7),
                messages = new string[] {
                    "Fixed: missing track connections when lane count mismatches.",
                    "Fixed: check all flags including extra AN flags when rendering roads before placement (while drawing roads).",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 13, 6),
                messages = new string[] {
                    "Fixed: Asymmetrical pavement distortions at nodes",
                    "Feature: copying props/nodes/segments to other lane or elevations will copy custom flag names too.",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 13, 1),
                messages = new string[] {
                    "Feature: Lane Tags",
                    "Feature: Fence (tracks that use None lanes and lane tags)",
                    "Feature: Track props (on nodes only)",
                }
            },


            new WhatsNewEntry {
                version = new Version(3, 12, 4),
                messages = new string[] {
                    "Fixed: Virtual bus stops did not work on props",
                    "Feature: Virtual bus stop tool highlights affected lane on hover",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 12, 3),
                messages = new string[] {
                    "Feature: Node tags are more powerful than connect groups(AN exposes this new CS feature)",
                    "Obsolete: Custom connect groups - use node tags instead (Still works for backward compatibility)",
                    "Fixed: Parking angle",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 10, 3),
                messages = new string[] {
                    "Hint: AN tool supports using Page down/up keys to select underground/overground networks.",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 10, 2),
                messages = new string[] {
                    "Feature: Segment template.",
                }
            },
            new WhatsNewEntry {
                version = new Version(3, 10, 0),
                messages = new string[] {
                    "fixed: saving track embankment in game.",
                    "fixed: props can use Segment user data.",
                }
            },
            new WhatsNewEntry {
                version = new Version(3, 9, 0),
                messages = new string[] {
                    "Feature: Anti-flickering for DC nodes.",
                    "Feature: Improve Column direction for AN networks (improvement over vanilla).",
                    "Compatibility: LSMR support (old LSM still supported).",
                }
            },

            new WhatsNewEntry {
                version = new Version(3, 8, 6),
                messages = new string[] {
                    "Feature: Export nodes to xml templates",
                    "Hint: display summary hintbox when you hover over nodes",
                    "Feature: Node/prop/track can use segment user flags",
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

