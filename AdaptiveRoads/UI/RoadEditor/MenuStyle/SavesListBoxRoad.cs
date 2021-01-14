using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using System.Reflection;
using HarmonyLib;
using KianCommons.UI.Helpers;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Util;
using AdaptiveRoads.DTO;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SavesListBoxRoad : SavesListBoxT<RoadAssetInfo> {
        public override IEnumerable<RoadAssetInfo> LoadItems() => RoadAssetInfo.LoadAllFiles();
        public override string GetName(RoadAssetInfo item) => item.Name;
    }
}
