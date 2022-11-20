namespace AdaptiveRoads.UI.RoadEditor {
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class EditorCheckboxPanel : UIPanel, IDataUI {
        public event EventHandler EventDestroy;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;
        public EditorCheckbox Checkbox;
        public RefChain<bool> RefChain;
        string hint_;

        public override void OnDestroy() {
            EventDestroy?.Invoke(this, null);
            SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        public static EditorCheckboxPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            RefChain<bool> refChain) {
            Log.Debug($"ButtonPanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent<EditorCheckboxPanel>();
            subPanel.RefChain = refChain;
            subPanel.Enable();
            subPanel.Checkbox.Label = label;
            subPanel.hint_ = hint;
            container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);
            subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;

            return subPanel;
        }

        public override void Awake() {
            try {
                Log.Called();
                base.Awake();
                //backgroundSprite = "GenericPanelWhite";
                //color = Color.white;

                size = new Vector2(370, 36);
                atlas = TextureUtil.Ingame;
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Horizontal;
                padding = new RectOffset(3, 3, 3, 3);

                Checkbox = AddUIComponent<EditorCheckbox>();
            } catch(Exception ex) { ex.Log(); }
        }

        public override void Start() {
            try {
                base.Start();
                Refresh();
                Checkbox.eventCheckChanged += OnCheckedChanged;
            } catch (Exception ex) { ex.Log(); }
        }

        private void OnCheckedChanged(UIComponent component, bool value) {
            if(RefChain.Value != value) {
                RefChain.Value = value;
                EventPropertyChanged?.Invoke();
            }
        }

        public virtual void Refresh() =>  Checkbox.isChecked = RefChain.Value;
        
        public string GetHint() => hint_;
        public bool IsHovered() => containsMouse;
    }
}
