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

namespace AdaptiveRoads.UI.RoadEditor.Templates {
    public class PanelBase : UIPanel {
        public const int WIDTH_LEFT = 425;
        public const int WIDTH_RIGHT = 475;
        public const int PAD = 10;

        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            backgroundSprite = "MenuPanel";
            width = WIDTH_LEFT + WIDTH_RIGHT + PAD * 3;
            color = new Color32(49, 52, 58, 255);
        }

        public override void Start() {
            base.Start();
            Invalidate(); // TODO is this necessary?
            pivot = UIPivotPoint.MiddleCenter;
            anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
        }

        public UIDragHandle AddDrag(string caption) {
            var drag = AddUIComponent<Drag>();
            drag.Init(caption);
            return drag;
        }

        public UIPanel AddSubPanel() {
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.autoLayout = true;
            panel.autoLayoutDirection = LayoutDirection.Vertical;
            
            panel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
            panel.autoFitChildrenVertically = true;
            return panel;
        }

        public UIPanel AddLeftPanel() {
            var panel = AddSubPanel();
            panel.relativePosition = new Vector2(PAD, 46);
            panel.width = WIDTH_LEFT;
            return panel;
        }

        public UIPanel AddRightPanel() {
            var panel = AddSubPanel();
            panel.relativePosition = new Vector2(WIDTH_LEFT + PAD * 2, 46);
            panel.width = WIDTH_RIGHT;
            return panel;
        }
    }
}
