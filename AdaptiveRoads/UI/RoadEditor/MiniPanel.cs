using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Diagnostics;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using System.Reflection;
using HarmonyLib;
using System.Linq;
using KianCommons.UI.Helpers;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.UI.RoadEditor {
    public class MiniPanel: UIPanel {
        public UILabel Label;

        public static void CloseAll() {
            var panels = UIView.GetAView().GetComponents<MiniPanel>();
            Log.Debug("CloseALL: open mini panel count: " + panels.Count());
            foreach (var panel in panels)
                Destroy(panel);
        }

        public static MiniPanel Display() {
            Log.Debug($"MiniPanel.Display() called");
            CloseAll();
            return UIView.GetAView().AddUIComponent(typeof(MiniPanel)) as MiniPanel;
        }

        public override void Awake() {
            base.Awake();
            name = "MiniPanel";
            autoLayout = true;
            autoSize = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            atlas = TextureUtil.Ingame;
            backgroundSprite = "GenericPanel";
            isVisible = true;
        }

        bool started = false;
        public override void Start() {
            base.Start();
            position = Input.mousePosition;
            Refresh();
            started = true;
        }

        public override void Update() {
            base.Update();
            if (Input.GetMouseButtonDown(1)) {
                Log.Debug("MiniPanel.Update: rigt click => close this");
                Destroy(this);
            }
        }

        public UIButton AddButton(string label, string hint, Action action) {
            var btn = AddUIComponent<MiniPanelButton>();
            btn.Action = action;
            btn.Hint = hint;
            btn.text = label;
            if (started) Refresh();
            return btn;
        }

        public void Refresh() {
            this.FitChildren();
        }

        public class MiniPanelButton : UIButtonExt {
            public Action Action;
            public string Hint;

            protected override void OnClick(UIMouseEventParameter p) {
                base.OnClick(p);
                Action?.Invoke();
                MiniPanel.CloseAll();
            }
        }
    }
}
