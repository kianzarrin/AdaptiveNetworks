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
        public static void CloseAll() {
            foreach (var panel in FindObjectsOfType<MenuPanelBase>())
                Destroy(panel?.gameObject);
        }


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
            var panel = container.AddUIComponent<UIPanel>();
            panel.autoFitChildrenHorizontally = true;
            panel.autoFitChildrenVertically = true;
            panel.autoLayout = true;
            panel.autoLayoutPadding = new RectOffset(0, PAD, 0, 0);
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            if (!container.autoLayout)
            {
                void FixPos() => panel.relativePosition =
                    container.size - panel.size - new Vector2(PAD,PAD);
                container.eventSizeChanged += (_, __) => FixPos();
                panel.eventSizeChanged += (_, __) => FixPos();
                FixPos();
                
            }

            return panel;
        }

#if DEBUG
        public override void OnDestroy() {
            Log.Debug($"{this}.OnDestroy() was called");
            base.OnDestroy();
        }
#endif
    }
}
