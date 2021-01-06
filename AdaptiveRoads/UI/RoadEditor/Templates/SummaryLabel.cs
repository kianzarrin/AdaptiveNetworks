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

namespace AdaptiveRoads.UI.RoadEditor.Templates {
    public class SummaryLabel : UILabel{
        public override void Awake() {
            base.Awake();
            wordWrap = true;
            autoSize = false;
            color = Color.black;
            textColor = Color.white;
            padding = new RectOffset(5, 5, 5, 5);
            atlas = TextureUtil.Ingame;
        }
    }
}
