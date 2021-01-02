using ColossalFramework;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using AdaptiveRoads.UI.RoadEditor;


namespace AdaptiveRoads.UI.RoadEditor {
    public class HintBox : UILabel {
        static HintBox _instance;
        public static HintBox Create() =>
            _instance = UIView.GetAView().AddUIComponent(typeof(HintBox)) as HintBox;
        public static void Release() =>
            DestroyImmediate(_instance);
        public override void OnDestroy() {
            _instance = null;
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        public override void Awake() {
            base.Awake();
            wordWrap = false;
            byte intensity = 0; //32;
            color = new Color32(intensity, intensity, intensity, 255 /*190*/);
            textColor = Color.white;
            textScale = 1f; //.8f
            padding = new RectOffset(5, 5, 5, 5);
            atlas = TextureUtil.Ingame;
            relativePosition = default;
        }

        public new float width {
            get => base.width;
            set {
                base.width = value;
                this.minimumSize = new Vector2(0, 0);
                this.maximumSize = new Vector2(value, height);
            }
        }

        public new float height {
            get => base.height;
            set {
                base.height = value;
                this.maximumSize = new Vector2(width, value);
            }
        }

        public new Vector2 size {
            get => base.size;
            set {
                base.size = value;
                minimumSize = new Vector2(0, 0);
                maximumSize = size;
            }
        }

        public override void Start() {
            base.Start();
            this.backgroundSprite = "GenericPanel";
            this.size = new Vector2(1000, 1000);
            this.autoSize = true;
            Invalidate();
        }

        public string hint1_, hint2_, hint3_;

        /// <summary>
        /// Controller hotkeys
        /// </summary>
        public string Hint1;

        // Controller description
        public string Hint2;

        // tool 
        public string Hint3;

        public static IEnumerable<RoadEditorPanel> GetRoadEditorPanel() {
            return FindObjectsOfType<RoadEditorPanel>().AsEnumerable();
        }

        public string BuildText() {
            bool h1 = !Hint1.IsNullOrWhiteSpace();
            bool h2 = !Hint2.IsNullOrWhiteSpace();
            bool h3 = !Hint3.IsNullOrWhiteSpace();
            const string nl = "\n";
            string ret = "";

            if (h1) ret += Hint1;
            if (h1 && h2) ret += nl;
            if (h2) ret += Hint2;
            if (h2 && h3) ret += nl;
            if (h3) ret += Hint3;

            return ret;
        }

        static object FieldValue(string fieldName, object target) =>
            ReflectionHelpers.GetFieldValue(fieldName, target);
        static string Str(REPropertySet re) =>
            $"{re.GetType().Name}(target:{FieldValue("m_Target", re)}, field:{FieldValue("m_Target", re)}";

        public override void Update() {
            base.Update();
            try {
                if (ModSettings.HideHints) return;

                position = Input.mousePosition;
                //Camera.main.ViewportToScreenPoint()

                //var t = FindObjectsOfType<REPropertySet>()
                //    .Select(re => Str(re)).ToSTR();
                Hint1 = Hint2 = Hint3 = null;
                foreach (var item in FindObjectsOfType<BitMaskPanel>()) {
                    if (item.IsHovered()) {
                        Hint1 = item.GetHint();
                        break;
                    }
                }
                ShowInfo();
                //Log.DebugWait(Hint1);


                //if (containsMouse)
                //    return; // prevent flickering on mouse hover

                //    string h1 = null, h2 = null;
                //    Component c = default;
                //    if (c != null) {
                //        //Log.DebugWait($"{component.name}-{c}@{rootname}");
                //        h1 = c.HintHotkeys;
                //        h2 = c.HintDescription;
                //    }
                //    // TODO get h3 from tool.
                //    var prev_h1 = Hint1;
                //    var prev_h2 = Hint2;
                //    var prev_h3 = Hint3;

                //    Hint1 = h1;
                //    Hint2 = h2;

                //    if (Hint1 != prev_h1 || Hint2 != prev_h2 || Hint3 != prev_h3) {
                //        RefreshValues();
                //    }
            }
            catch (Exception e) {
                Hint1 = e.ToString();
                Log.DebugWait(Hint1);
            }
        }


        private void ShowInfo() {
            text = BuildText();
            isVisible = !text.IsNullorEmpty();
            if (!isVisible)
                return;

            var screenSize = ToolBase.fullscreenContainer?.size
                ?? GetUIView().GetScreenResolution();

            var pos = MouseGUIPosition() +  new Vector3(25, 25);
            pos.x = ClampToScreen(pos.x, width, screenSize.x);
            pos.y = ClampToScreen(pos.y, height, screenSize.y);

            relativePosition = pos;
        }
        private Vector3 MouseGUIPosition() {
            var uiView = GetUIView();
            return uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
        }
        static float ClampToScreen(float pos, float size, float screen) {
            float max = screen - size;
            if (max <= 0) return 0;
            return Mathf.Clamp(pos, 0, max);
        }
    }

}
