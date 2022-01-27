namespace AdaptiveRoads.UI {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using UnityEngine;

    public class AutoSizeCheckbox : UICheckBoxExt {
        public override void Awake() {
            base.Awake();
            label.eventSizeChanged += Label_eventSizeChanged;
        }

        private void Label_eventSizeChanged(UIComponent component, Vector2 value) {
            Log.Called();
            width = label.relativePosition.x + label.width;
            (parent as IFittable)?.FitRoot();
        }
    }
}
