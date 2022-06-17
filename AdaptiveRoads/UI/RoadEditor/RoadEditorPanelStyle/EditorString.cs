namespace AdaptiveRoads.UI.RoadEditor {
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class EditorTextField : UITextField {
        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            selectionSprite = "EmptySprite";
            normalBgSprite = "TextFieldPanel";

            builtinKeyNavigation = true;
            readOnly = false;
            horizontalAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;
            useDropShadow = true;

            submitOnFocusLost = true;
            selectOnFocus = true;
        }
    }

    public class EditorStringPanel : UIPanel, IDataUI {
        public UITextField Field;
        public UILabel Label;
        string hint_;
        FieldInfo fieldInfo_;
        object target_;


        public override void OnDestroy() {
            SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        public static EditorStringPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            object target,
            FieldInfo fieldInfo) {
            Assertion.Assert(fieldInfo.FieldType == typeof(string));
            Log.Debug($"ButtonPanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent<EditorStringPanel>();
            subPanel.Enable();
            subPanel.Label.text = label + ":";
            subPanel.hint_ = hint;
            subPanel.fieldInfo_ = fieldInfo;
            subPanel.target_ = target;
            container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);

            subPanel.Field.eventTextSubmitted += (_, value) => fieldInfo.SetValue(target, value);
            subPanel.Field.eventTextSubmitted += (_, __) => roadEditorPanel.OnObjectModified();

            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            size = new Vector2(370, 27);
            atlas = TextureUtil.Ingame;
            padding = new RectOffset(3, 3, 3, 3);

            Label = AddUIComponent<UILabel>();
            Label.relativePosition = new Vector2(0, 4);
            Field = AddUIComponent<EditorTextField>();
            Field.size = new Vector2(318, 20);
            Field.relativePosition = new Vector2(50, 3);
        }

        public override void Start() {
            base.Start();
            Refresh();
        }

        public virtual void Refresh() { fieldInfo_.GetValue(target_); }
        public string GetHint() => hint_;
        public bool IsHovered() => containsMouse;
    }
}
