namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;
    using AdaptiveRoads.Patches.RoadEditor;
    using System.Reflection;
    using AdaptiveRoads.Manager;
    using TrafficManager.API.Traffic;
    using System;

    public class ButtonPanel : UIPanel {
        public UIButton Button;
        public override void OnDestroy() {
            Button = null;
            base.OnDestroy();
        }

        public static ButtonPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            Action action) {
            Log.Debug($"ButtonPanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(ButtonPanel)) as ButtonPanel;
            subPanel.Enable();
            subPanel.Button.text = label;
            subPanel.Button.tooltip = hint;
            subPanel.Button.eventClick += (_,__) => action();
            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            //backgroundSprite = "GenericPanelWhite";
            //color = Color.white;

            size = new Vector2(370, 27);
            atlas = TextureUtil.Ingame;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            padding = new RectOffset(0, 0, 3, 3);
            autoLayoutPadding = new RectOffset(0, 3, 0, 0);

            Button = AddUIComponent<UIButtonExt>();
            Button.size = new Vector2(360, 25);
        }
    }
}
