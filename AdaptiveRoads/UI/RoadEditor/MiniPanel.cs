using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Linq;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor {
    public class MiniPanel : UIPanel {
        public static void CloseAll() {
            var panels = UIView.GetAView().GetComponentsInChildren<MiniPanel>();
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
            Log.Debug("MiniPanel.Start() called");
            base.Start();
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

        private void SetPosition() {
            var uiView = GetUIView();
            var mouse = Input.mousePosition;
            var mouse2 = uiView.ScreenPointToGUI(mouse / uiView.inputScale);
            relativePosition = mouse2;
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
            Log.Debug("MiniPanel.Refresh() called");
            SetPosition();
            FitChildren();
            Invalidate();
        }
        public override void OnDestroy() {
            var buttons = GetComponentsInChildren<MiniPanelButton>();
            Log.Debug($"MiniPanel.OnDestroy() called. {buttons.Length} buttons found");
            foreach (var btn in buttons) {
                btn.Hint = null;
                btn.Action = null;
            }
            base.OnDestroy();
        }
        public class MiniPanelButton : UIButtonExt {
            public Action Action;
            public string Hint;
            public override void OnDestroy() {
                Log.Debug("MiniPanelButton.OnDestroy() called");
                base.OnDestroy();
            }

            protected override void OnClick(UIMouseEventParameter p) {
                base.OnClick(p);
                Action?.Invoke();
                MiniPanel.CloseAll();
            }
        }
    }
}
