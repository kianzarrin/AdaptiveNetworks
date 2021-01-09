using AdaptiveRoads.Manager;
using AdaptiveRoads.Util;
using ColossalFramework;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AdaptiveRoads.Util.DPTHelpers;

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
            byte intensity = 0;
            color = new Color32(intensity, intensity, intensity, 255);
            textColor = Color.white;
            textScale = 1f;
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
        /// description
        /// </summary>
        public string Hint1;

        // hotkeys description
        public string Hint2;

        // extra
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
            try {
                base.Update();
                if (ModSettings.HideHints)
                    return;
                GetHint();
                ShowInfo();
            } catch (Exception ex) {
                Log.Exception(ex);
            }

        }

        private void ShowInfo() {
            text = BuildText();
            isVisible = !text.IsNullorEmpty();
            if (!isVisible)
                return;

            var screenSize = ToolBase.fullscreenContainer?.size
                ?? GetUIView().GetScreenResolution();

            var pos = MouseGUIPosition() + new Vector3(25, 25);
            pos.x = ClampToScreen(pos.x, width, screenSize.x);
            pos.y = ClampToScreen(pos.y, height, screenSize.y);

            relativePosition = pos;
        }

#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
        public void GetHint() {
            try {

                position = Input.mousePosition;
                //Camera.main.ViewportToScreenPoint()

                //var t = FindObjectsOfType<REPropertySet>()
                //    .Select(re => Str(re)).ToSTR();
                Hint1 = Hint2 = Hint3 = null;
                foreach (var panel in FindObjectsOfType<UIPanel>()) {
                    if (panel is IDataUI dataUI && dataUI.IsHovered()) {
                        // note that it is possible for the panel
                        // not to contain mouse but for the dropdown to contain mouse.
                        Hint1 = dataUI.GetHint();
                        break;
                    } else if (panel.containsMouse) {
                        string h = "Click => toggle\n" +
                            "Control + Click => multi-select\n"+
                            "Left + Click => show more options";
                        var groupPanel = panel.GetComponent<RoadEditorCollapsiblePanel>();
                        if (groupPanel && groupPanel.LabelButton.containsMouse) {
                            var target = groupPanel.GetTarget();
                            string label = groupPanel.LabelButton.text;
                            if (label == "Props") {
                                Hint2 = h;
                            } else if (
                                groupPanel.GetArray() is NetInfo.Lane[] m_lanes
                                && m_lanes.Any(_lane => _lane.HasProps())
                                && target == NetInfoExtionsion.EditedNetInfo) {
                                Hint2 = h;
                            }
                        }
                        UICustomControl toggle = panel.GetComponent(DPTType) as UICustomControl;
                        if (toggle && GetDPTSelectButton(toggle).containsMouse) {
                            object element = GetDPTTargetElement(toggle);
                            var target = GetDPTTargetObject(toggle);
                            if (element is NetLaneProps.Prop prop) {
                                Hint1 = prop.Summary();
                                Hint2 = h;
                            } else if (element is NetInfo.Lane lane && lane.HasProps()
                                && target == NetInfoExtionsion.EditedNetInfo) {
                                Hint2 = h;
                            }
                        }
                        var proprefset = panel.GetComponent<RERefSet>();
                        if(proprefset && proprefset.m_SelectButton.containsMouse) {
                            Hint2 = "Click => display load prop panel\n" +
                                "Alt + Click => fast type in prop/tree name";
                        }
                    }
                }
            } catch (Exception e) {
                Hint1 = e.ToString();
                Log.DebugWait(Hint1);
            }
        }
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast


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
