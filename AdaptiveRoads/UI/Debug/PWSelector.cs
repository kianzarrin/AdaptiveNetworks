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

    public class PWSelector  : UIPanel {
        [Conditional("DEBUG")]
        public static void Create() => UIView.GetAView().AddUIComponent<PWSelector>();
        [Conditional("DEBUG")]
        public static void Release() =>
            DestroyImmediate(UIView.GetAView().FindUIComponent<PWSelector>(nameof(PWSelector))?.gameObject);

        public override void Awake() {
            base.Awake();
            try {
                name = nameof(PWSelector);
                atlas = TextureUtil.Ingame;
                relativePosition = default;

                for (int row = 0; row < Operations.GetLength(0); ++row) {
                    for (int col = 0; col < Operations.GetLength(1); ++col) {
                        var dd = AddUIComponent<DD>();
                        dd.relativePosition = new Vector2(105 * col + 3, 30 * row + 3);
                        dd.col = col;
                        dd.row = row;
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
            absolutePosition = new Vector2(70, 5);
        }

        internal class DD: UIDropDownExt {
            public override void Awake() {
                try {
                    base.Awake();
                    size = new Vector2(100, 25);
                    items = Enum.GetNames(typeof(Operation));
                }catch(Exception ex) {
                    ex.Log();
                }
            }
            public int row, col;

            public override void Start() {
                try {
                    base.Start();
                    selectedIndex = (int)Operations[row, col];
                }catch (Exception ex) {
                    ex.Log();
                }
            }

            protected override void OnSelectedIndexChanged() {
                base.OnSelectedIndexChanged();
                LogCalled();
                Operations[row, col] = (Operation)selectedIndex;
                SimulationManager.instance.AddAction(Refresh);
            }
        }

        static void Refresh() {
            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                if (!NetUtil.IsSegmentValid(segmentID)) continue;
                if (!segmentID.ToSegment().Info.IsAdaptive()) continue;
                NetManager.instance.UpdateSegment(segmentID);
            }
        }
    }
}
