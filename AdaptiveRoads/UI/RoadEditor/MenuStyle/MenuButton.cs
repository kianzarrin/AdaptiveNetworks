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

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class MenuButton : UIButton {
        public override void Awake() {
            base.Awake();
            width = 153;
            height = 47;
            textScale = 1.3f;
            //textPadding = new RectOffset(10, 10, 10, 10);

            horizontalAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;

            hoveredTextColor = new Color32(7,132,255,255);
            disabledColor = new Color32(153, 153, 153, 255);
            disabledTextColor = new Color32(46, 46, 46, 255);
            atlas = TextureUtil.Ingame;
            normalBgSprite = "ButtonMenu";
        }
    }
}
