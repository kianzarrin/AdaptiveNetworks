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
using System.IO;
using System.Drawing.Imaging;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class Drag : UIDragHandle {
        public void Init(string caption) {
            width = parent.width;
            height = 40;
            relativePosition = Vector2.zero;
            target = parent;

            var label = AddUIComponent<UILabel>();
            label.textScale = 1.5f;
            label.text = caption;
            label.autoHeight = true;
            label.autoSize = true;
            label.relativePosition = new Vector2(0,6);
            label.anchor = UIAnchorStyle.CenterHorizontal;
        }
    }
}
