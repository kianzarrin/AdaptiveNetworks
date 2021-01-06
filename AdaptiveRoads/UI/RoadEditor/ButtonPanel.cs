namespace AdaptiveRoads.UI.RoadEditor {
    using AdaptiveRoads.Patches.RoadEditor;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;

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
            subPanel.Button.eventClick += (_, __) => action();

            container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);

            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            //backgroundSprite = "GenericPanelWhite";
            //color = Color.white;

            size = new Vector2(370, 36);
            atlas = TextureUtil.Ingame;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            padding = new RectOffset(3, 3, 3, 3);

            Button = AddUIComponent<EditorButon>();
        }
    }
}
