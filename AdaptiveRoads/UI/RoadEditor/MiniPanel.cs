using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdaptiveRoads.UI.RoadEditor {
    public class MiniPanel : UIPanel, IHint {
        public IEnumerable<UIButton> Buttons => GetComponentsInChildren<UIButton>();
        public IEnumerable<UIComponent> Components =>
            GetComponentsInChildren<UIComponent>()
            .Where(c => c != this);
        public static void CloseAll() {
            var panels = UIView.GetAView().GetComponentsInChildren<MiniPanel>();
            Log.Debug("CloseALL: open mini panel count: " + panels.Count() /*+ Environment.StackTrace*/);
            foreach (var panel in panels)
                panel.Close();
        }

        public void Close() => Destroy(gameObject);

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
            color = Color.black;
        }

        bool started = false;
        public override void Start() {
            Log.Debug("MiniPanel.Start() called");
            base.Start();
            Refresh();
            started = true;
        }

        public override void Update() {
            isVisible = isVisible; // trick fbsbooster to ignore this update.
            base.Update();
            if (Input.GetMouseButtonDown(1)) {
                Log.Debug("MiniPanel.Update: rigt click => close this");
                Close();
            }
            if (Input.GetMouseButtonDown(0) && !containsMouse) {
                Log.Debug("MiniPanel.Update: left click outside panel => close this");
                // escape if contains mouse
                // OnClick will destroy the panel
                foreach(var button in Buttons) {
                    if (button.containsMouse)
                        return;
                }
                Close();
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

        public MiniPanelNumberField AddNumberField() {
            var field = AddUIComponent<MiniPanelNumberField>();
            if(started) Refresh();
            return field;
        }
        public MiniPanelFloatField AddFloatField() {
            var field = AddUIComponent<MiniPanelFloatField>();
            if(started) Refresh();
            return field;
        }
        public MiniPanelTextField AddTextField() {
            var field = AddUIComponent<MiniPanelTextField>();
            if (started) Refresh();
            return field;
        }

        public void Refresh() {
            Log.Debug("MiniPanel.Refresh() called");
            SetPosition();
            FitChildren();
            foreach (var item in Components) {
                item.autoSize = false;
                item.width = width;
            }
            Invalidate();
            GetComponentInChildren<UITextField>()?.Focus();
        }

        public bool IsHovered() => containsMouse;


        public string GetHint() {
            foreach(var c in Components) {
                if(c is IHint h && h.IsHovered()) {
                    return h.GetHint();
                }
            }
            return null;
        }

        public class MiniPanelButton : UIButtonExt, IHint {
            public Action Action;
            public string Hint;
            public string GetHint() => Hint;
            public bool IsHovered() => containsMouse;

            public override void Awake() {
                base.Awake();
                this.textHorizontalAlignment = UIHorizontalAlignment.Center;
            }

            public override void OnDestroy() {
                Hint = null;
                Action = null;
                base.OnDestroy();
            }

            protected override void OnClick(UIMouseEventParameter p) {
                try {
                    base.OnClick(p);
                    Assertion.AssertNotNull(Action);
                    Log.Info($"`{text}` clicked. Invoking {Action} ...", true);
                    Action();
                    GetComponentInParent<MiniPanel>().Close();
                }catch(Exception ex) {
                    Log.Exception(ex);
                }
            }
        }

        public class MiniPanelNumberField : MiniPanelTextField {
            public override void Awake() {
                base.Awake();
                text = "0";
                width = 80;

                numericalOnly = true;
                allowFloats = false;
                allowNegative = true;
            }

            public int Number {
                get {
                    if (int.TryParse(text, out int ret))
                        return ret;
                    return 0;
                }
            }
        }

        public class MiniPanelFloatField : MiniPanelNumberField {
            public override void Awake() {
                base.Awake();
                allowFloats = true;
            }

            new public float Number {
                get {
                    if(float.TryParse(text, out float ret))
                        return ret;
                    return 0;
                }
            }
        }

        public class MiniPanelTextField : UITextField, IHint {
            public string Hint;
            public string GetHint() => Hint;
            public bool IsHovered() => containsMouse;

            public override void OnDestroy() {
                Hint = null;
                base.OnDestroy();
            }

            public override void Awake() {
                base.Awake();
                atlas = TextureUtil.Ingame;
                height = 20;
                width = 200;
                padding = new RectOffset(0, 0, 3, 3);
                builtinKeyNavigation = true;
                horizontalAlignment = UIHorizontalAlignment.Center;
                verticalAlignment = UIVerticalAlignment.Middle;
                selectionSprite = "EmptySprite";
                selectionBackgroundColor = new Color32(0, 172, 234, 255);
                normalBgSprite = "TextFieldPanelHovered";
                textColor = new Color32(0, 0, 0, 255);
                disabledTextColor = new Color32(80, 80, 80, 128);
                color = new Color32(255, 255, 255, 255);
                useDropShadow = true;
                selectOnFocus = true;
            }
            public override void Start() {
                base.Start();
                eventTooltipTextChanged += (_, __) => RefreshTooltip();
            }
        }
    }
}
