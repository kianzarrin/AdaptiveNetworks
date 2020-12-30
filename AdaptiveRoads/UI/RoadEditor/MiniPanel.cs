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
    public class MiniPanel: UIAutoSizePanel {
        public UILabel Label;

        public static void CloseAll() {
            foreach (var panel in UIView.GetAView().GetComponents<MiniPanel>())
                Destroy(panel);
        }

        public static MiniPanel Display() {
            Log.Debug($"MiniPanel.Display() called");
            CloseAll();
            Input.GetMouseButtonDown(1); //consume. is this neccessary?
            var panel = UIView.GetAView().AddUIComponent(typeof(MiniPanel)) as MiniPanel;
            return panel;
        }

        public override void Awake() {
            base.Awake();
            AutoSize2 = true;
            isVisible = true;
            name = "MiniPanel";
            backgroundSprite = "GenericPanel";
            absolutePosition = Input.mousePosition;
        }

        bool started = false;
        public override void Start() {
            base.Start();
            started = true;
            RefreshSizeRecursive();
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
            if (started) RefreshSizeRecursive();
            return btn;
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
