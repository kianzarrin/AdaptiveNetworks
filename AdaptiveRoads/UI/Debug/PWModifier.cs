namespace AdaptiveRoads.UI.Debug {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using KianCommons;
    using AdaptiveRoads.Patches.AsymPavements;
    using static AdaptiveRoads.Patches.AsymPavements.Util;
    using System.Diagnostics;
    using static KianCommons.ReflectionHelpers;

    /// <summary>
    /// debug pavement width problems
    /// </summary>
    public class PWModifier  : UIPanel {
        [Conditional("DEBUG")]
        public static void Create() => UIView.GetAView().AddUIComponent<PWModifier>();
        [Conditional("DEBUG")]
        public static void Release() =>
            DestroyImmediate(UIView.GetAView().FindUIComponent<PWSelector>(nameof(PWModifier))?.gameObject);

        public override void Awake() {
            Log.Called();
            base.Awake();
            try {
                name = nameof(PWSelector);
                atlas = TextureUtil.Ingame;
                relativePosition = default;

                for (int row = 0; row < Forced.GetLength(0); ++row) {
                    for (int col = 0; col < Forced.GetLength(1); ++col) {
                        var tb = AddUIComponent<TB>();
                        tb.relativePosition = new Vector2(105 * col + 3, 30 * row + 3);
                        tb.col = col;
                        tb.row = row;
                    }
                }

                color = Color.grey;
                backgroundSprite = "GenericPanelWhite";
                FitChildren(new Vector2(3,3));
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public override void Start() {
            base.Start();
            absolutePosition = new Vector2(500, 5);
        }

        internal class TB: UITextField {
            public override void Awake() {
                Log.Called();
                try {
                    base.Awake();
                    atlas = TextureUtil.Ingame;
                    builtinKeyNavigation = true;
                    readOnly = false;
                    horizontalAlignment = UIHorizontalAlignment.Center;
                    verticalAlignment = UIVerticalAlignment.Middle;
                    selectionSprite = "EmptySprite";
                    selectionBackgroundColor = new Color32(0, 172, 234, 255);
                    normalBgSprite = "TextFieldPanelHovered";
                    disabledBgSprite = "TextFieldPanelHovered";
                    textColor = new Color32(0, 0, 0, 255);
                    disabledTextColor = new Color32(80, 80, 80, 128);
                    color = new Color32(255, 255, 255, 255);
                    useDropShadow = true;
                    text = "0";

                    submitOnFocusLost = true;
                    selectOnFocus = true;
                    numericalOnly = true;
                    allowFloats = true;
                    allowNegative = true;

                    size = new Vector2(100, 25);
                }catch(Exception ex) {
                    ex.Log();
                }
            }
            public int row, col;

            public override void Start() {
                try {
                    base.Start();
                    text = Forced[row, col].ToString();
                }catch (Exception ex) {
                    ex.Log();
                }
            }
            protected override void OnTextChanged() {
                base.OnTextChanged();
                LogCalled();
                if (float.TryParse(text, out float value))
                    Forced[row, col] = value;
                SimulationManager.instance.AddAction(Refresh);
            }
        }

        static void Refresh() {
            Log.Called();
            for(ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                if (!NetUtil.IsSegmentValid(segmentID)) continue;
                if (!segmentID.ToSegment().Info.IsAdaptive()) continue;
                NetManager.instance.UpdateSegment(segmentID);
            }
        }
    }
}
