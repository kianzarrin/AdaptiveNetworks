using System;
using UnityEngine;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class LoadTransitionPropTemplatePanel : LoadTemplatePanel<SaveListBoxTransitionProp, TransitionPropTemplate> {
        public MenuTextFieldFloat Displacement;

        public delegate void OnLoadedHandler(TransitionProp[] props);
        public event OnLoadedHandler OnLoaded;
        protected override string Title => "Load Prop Template";

        public static LoadTransitionPropTemplatePanel Display(OnLoadedHandler handler) {
            Log.Called();
            var ret = UIView.GetAView().AddUIComponent<LoadTransitionPropTemplatePanel>();
            ret.OnLoaded = handler;
            return ret;
        }

        protected override void AddCustomUIComponents(UIPanel panel) {
            {
                UIPanel panel2 = panel.AddUIComponent<UIPanel>();
                panel2.autoLayout = true;
                panel2.autoLayoutDirection = LayoutDirection.Horizontal;
                panel2.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
                var lbl = panel2.AddUIComponent<UILabel>();
                lbl.text = "Displacement:";
                Displacement = panel2.AddUIComponent<MenuTextFieldFloat>();
                Displacement.width = panel.width - Displacement.relativePosition.x;
                Displacement.tooltip = "put a positive number to move props sideways.";
                lbl.height = Displacement.height;
                lbl.verticalAlignment = UIVerticalAlignment.Middle;
                panel2.FitChildren();
            }
        }

        public override void Load(TransitionPropTemplate template) {
            var props = template.GetProps();
            foreach (var prop in props) {
                if (Displacement.Number != 0) {
                    prop.Displace(Displacement.Number);
                }
            }
            OnLoaded(props);
        }
    }
}
