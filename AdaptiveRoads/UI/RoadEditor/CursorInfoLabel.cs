using ColossalFramework;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using JetBrains.Annotations;
using System.Reflection;

namespace AdaptiveRoads.UI.RoadEditor {
    public class CursorInfoLabel : MonoBehaviour {
        static CursorInfoLabel _instance;
        public static CursorInfoLabel Create() =>
            _instance = UIView.GetAView().gameObject.AddComponent<CursorInfoLabel>();
        public static void Release() => DestroyImmediate(_instance);

        #region INFO

        private void Info() {
            var position = MouseGUIPosition();

            //var isToolTipEnable = Settings.ShowToolTip || Mode.Type == ToolModeType.SelectNode;
            //var isPanelHover = Panel.isVisible && new Rect(Panel.relativePosition, Panel.size).Contains(position);
            string txt = BuildText();

            if (txt.IsNullorEmpty())
                ToolBase.cursorInfoLabel.isVisible = false;
            else
                ShowToolInfo(txt, position);
        }
        private void ShowToolInfo(string text, Vector3 pos) {
            
            if (ToolBase.cursorInfoLabel == null) {
                Log.DebugWait("cursorInfoLabel->null");
                return;
            }
            ToolBase.cursorInfoLabel.isVisible = true;
            Log.DebugWait($"cursorInfoLabel: " +
                $"visible={ToolBase.cursorInfoLabel.isVisible} " +
                $"enabled={ToolBase.cursorInfoLabel.isEnabled} " +
                $"opacity={ToolBase.cursorInfoLabel.opacity}");

            ToolBase.cursorInfoLabel.text = text ?? "";

            UIView uIView = ToolBase.cursorInfoLabel.GetUIView();

            var screenSize = ToolBase.fullscreenContainer?.size ?? uIView.GetScreenResolution();
            Log.DebugWait($"pos={pos} | screenSize={screenSize} cursorInfoLabel.size={ToolBase.cursorInfoLabel.size}");

            pos += new Vector3(25, 25);
            pos.x = ClampToScreen(pos.x, ToolBase.cursorInfoLabel.width, screenSize.x);
            pos.y = ClampToScreen(pos.y, ToolBase.cursorInfoLabel.height, screenSize.y);
            ToolBase.cursorInfoLabel.relativePosition = pos;

            static float ClampToScreen(float pos, float size, float screen) {
                float max = screen - size;
                if (max <= 0) return 0;
                return Mathf.Clamp(pos, 0, max);
            }
        }
        private Vector3 MouseGUIPosition() {
            var uiView = ToolBase.cursorInfoLabel.GetUIView();
            return uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
        }

        #endregion

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
            AssemblyTypeExtensions.GetDeclaredFieldValue(fieldName, target);


        static string Str(REPropertySet re) =>
            $"{re.GetType().Name}[" +
            $"target:{FieldValue("m_Target", re).GetType()}, " +
            $"field:{FieldValue("m_TargetField", re)}]";


        [UsedImplicitly]
        void Update() {
            try {

                //var t = FindObjectsOfType<REPropertySet>().Select(re => Str(re)).ToSTR();

                Hint1 = MouseGUIPosition().ToString();

                Info();

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
    }

}
