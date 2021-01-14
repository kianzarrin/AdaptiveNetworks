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
    public class MenuPanelBase : UIPanel {
        public const int PAD = 10;
        public const int TITLE_HEIGHT = 46;

        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            backgroundSprite = "MenuPanel";
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
            panel.autoFitChildrenVertically = true;

            panel.autoLayoutPadding = new RectOffset(0, 0, 0, PAD);
            panel.relativePosition = new Vector2(PAD, TITLE_HEIGHT);
            return panel;
        }

        public static UIPanel AddBottomPanel(UIPanel container) {
            var ret = container.AddUIComponent<UIPanel>();
            ret.autoFitChildrenHorizontally = true;
            ret.autoFitChildrenVertically = true;
            ret.autoLayout = true;
            ret.autoLayoutPadding = new RectOffset(0, PAD, 0, 0);
            ret.autoLayoutDirection = LayoutDirection.Horizontal;

            //if (!container.autoLayout)
            {
                ret.pivot = UIPivotPoint.BottomRight;
                ret.anchor = UIAnchorStyle.Bottom | UIAnchorStyle.Right;
            }

            return ret;

        }
    }
}
