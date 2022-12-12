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
        public static HintBox Instance { get; private set; }
        const int SPACE = 25;
        public static HintBox Create() =>
            Instance = UIView.GetAView().AddUIComponent(typeof(HintBox)) as HintBox;
        public static void Release() =>
            DestroyImmediate(Instance);
        public override void OnDestroy() {
            Instance = null;
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
            isVisible = false; // prevent initial black box on screen.
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

        public override void Update() {
            isVisible = isVisible; // trick fbsbooster to ignore this update.
            try {
                base.Update();
                if (ModSettings.HideHints)
                    return;
                if(!containsMouse) // avoid flickering
                    GetHint();
                ShowHint();
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        private void ShowHint() {
            text = BuildText();
            isVisible = !text.IsNullorEmpty();
            if (!isVisible)
                return;
            var screenSize = ToolBase.fullscreenContainer?.size
                ?? GetUIView().GetScreenResolution();

            var pos = MouseGUIPosition() + new Vector3(SPACE, SPACE);
            pos.x = SwitchPivotAtScreenEdge(pos.x, width, screenSize.x);
            pos.y = SwitchPivotAtScreenEdge(pos.y, height, screenSize.y);

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

                var panels = UIView.GetAView().GetComponentsInChildren<UIPanel>();
                foreach (var panel in panels) {
                    if (!panel.isVisible) continue;
                    IHint dataUI = panel as IHint ?? panel.objectUserData as IHint;
                    if (dataUI != null && dataUI.IsHovered()) {
                        // note that it is possible for the panel
                        // not to contain mouse but for the dropdown to contain mouse.
                        Hint1 = dataUI.GetHint();
                        break;
                    } else if (panel.containsMouse) {
                        const string toggleHint = "Click => toggle";
                        const string selectHint = "Control + Click => multi-select";
                        const string menuHint = "Right-Click => show more options";
                        const string dptHint = toggleHint + "\n" + selectHint;
                        const string labelButtonHint = toggleHint + "\n" + menuHint;
                        var customControl = panel.GetComponent<UICustomControl>();
                        try {
                            if (customControl is RoadEditorCollapsiblePanel groupPanel
                                && groupPanel.LabelButton.containsMouse) {
                                // label button of a group panel.
                                var target = groupPanel.GetTarget();
                                string label = groupPanel.LabelButton.text;
                                if (label == "Nodes" || label == "Segments" || label == "Props" || label == "Transition Props") {
                                    // group button
                                    Hint2 = labelButtonHint;
                                } else if (
                                    groupPanel.GetArray() is NetInfo.Lane[] m_lanes
                                    && m_lanes.Any(_lane => _lane.HasProps()) // don't show if there are not props.
                                    && target == NetInfoExtionsion.EditedNetInfo) {
                                    // lanes group button for basic elevation
                                    Hint2 = labelButtonHint;
                                } 
                            } else if (
                                panel.GetComponent(DPTType) is UICustomControl toggle &&
                                GetDPTSelectButton(toggle).containsMouse) {
                                // any dpt button
                                Hint2 = dptHint;

                                object element = GetDPTTargetElement(toggle);
                                var target = GetDPTTargetObject(toggle);
                                if (element is NetLaneProps.Prop prop) {
                                    // prop dpt
                                    Hint1 = prop.Summary();
                                    Hint2 += "\n" + menuHint;
                                } else if (element is NetInfoExtionsion.TransitionProp tprop) {
                                    // node dpt
                                    Hint1 = tprop.Summary();
                                    Hint2 += "\n" + menuHint;
                                } else if (element is NetInfo.Node node) {
                                    // node dpt
                                    Hint1 = node.Summary();
                                    Hint2 += "\n" + menuHint;

                                } else if (element is NetInfo.Segment segment) {
                                    // segment dpt
                                    Hint1 = segment.Summary();
                                    Hint2 += "\n" + menuHint;
                                } else if (
                                    element is NetInfo.Lane lane &&
                                    lane.HasProps() && target == NetInfoExtionsion.EditedNetInfo) {
                                    // props on this lane can be copied to other elevations so we need menu.
                                    Hint2 += "\n" + menuHint;
                                }
                            } else if (customControl is REPropertySet propertySet) {
                                try {
                                    var field = propertySet.GetTargetField();
                                    if (field != null) {
                                        if (field.Name == "m_speedLimit") {
                                            Hint1 = "1 game unit is 50 kph (31.06856mph)";
                                        } else if (field.Name == nameof(NetInfo.m_terrainStartOffset)) {
                                            Hint1 = "change terrain height along segment (used for start of tunnel entrance)";
                                        } else if (field.Name == nameof(NetInfo.m_terrainEndOffset)) {
                                            Hint1 = "change terrain height along segment (used for end of tunnel entrance)";
                                        } else if (field.Name == nameof(NetInfo.Node.m_forbidAnyTags)) {
                                            Hint1 = "forbid all tags";
                                        } else {
                                            var hints = field.GetHints()
                                                .Concat(field.DeclaringType.GetHints())
                                                .Concat(field.FieldType.GetHints());
                                            Hint1 = hints.JoinLines();
                                        }
                                    }
                                } catch(Exception ex) {
                                    throw new Exception($"propertySet={propertySet}", ex);
                                }
                            } else if (customControl is RERefSet refset &&
                                refset.m_SelectButton.containsMouse) {
                                if (refset.GetTarget() is NetLaneProps.Prop) {
                                    Hint2 = "Click => open prop import panel\n" +
                                        "Alt + Click => enter prop name manually";
                                }
                            }
                        } catch (Exception ex) {
                            throw new Exception($"customControl={customControl}", ex);
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
        static float SwitchPivotAtScreenEdge(float pos, float size, float screen) {
            float max = screen - size;
            if (max <= 0) return 0;
            if(pos > max)
                pos = pos - size - SPACE*2;
            if(pos < 0)
                pos = 0;
            return pos;
        }
    }

}
