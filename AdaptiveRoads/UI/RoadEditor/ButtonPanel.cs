namespace AdaptiveRoads.UI.RoadEditor {
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class ButtonPanel : UIPanel, IHint {
        public UIButton Button;
        string hint_;
        // used by QuayRoads to destroy its popups when the button is destroyed
        public event EventHandler EventDestroy;

        public override void OnDestroy() {
            EventDestroy?.Invoke(this, null);
            SetAllDeclaredFieldsToNull(this);
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
            subPanel.hint_ = hint;
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

        public string GetHint() => hint_;
        public bool IsHovered() => containsMouse;
    }
}
