using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using UnityEngine;
using AdaptiveRoads.DTO;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class LoadPropTemplatePanel : LoadTemplatePanel<SaveListBoxProp, PropTemplate> {
        public MenuCheckbox SwitchDir;
        public MenuCheckbox SwitchSide;
        public MenuTextFieldFloat Displacement;
        public bool UniDirectional;
        public bool SwitchBackward;

        public delegate void OnLoadedHandler(NetLaneProps.Prop[] props);
        public event OnLoadedHandler OnLoaded;
        protected override string Title => "Load Prop Template";

        public static LoadPropTemplatePanel Display(
            OnLoadedHandler handler, bool unidirectional, bool suggestBackward) {
            if(unidirectional && suggestBackward)
                throw new ArgumentException("switch backward cannot be applied to unidirectional lanes.");
            Log.Called();
            var ret = UIView.GetAView().AddUIComponent<LoadPropTemplatePanel>();
            ret.OnLoaded = handler;
            ret.UniDirectional = unidirectional;
            ret.SwitchBackward = suggestBackward;
            return ret;
        }

        protected override void AddCustomUIComponents(UIPanel panel) {
            {
                SwitchDir = panel.AddUIComponent<MenuCheckbox>();
                SwitchDir.Label = "Switch Forward/Backward";
                SwitchSide = panel.AddUIComponent<MenuCheckbox>();
                SwitchSide.Label = "Switch RHT/LHT";
            }
            {
                //Displacement = panel.AddUIComponent<TextFieldInt>();
                //Displacement.width = panel.width;

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

        public override void Start() {
            Log.Called();
            if(UniDirectional)
                SwitchDir.Hide();
            else if(SwitchBackward) 
                SwitchDir.isChecked = transform;
            base.Start();
        }
        public override void Load(PropTemplate template) {
            var props = template.GetProps();
            foreach (var prop in props) {
                if (SwitchDir.isChecked)
                    prop.ToggleForwardBackward();
                if (SwitchSide.isChecked)
                    prop.ToggleRHT_LHT(UniDirectional);
                if (Displacement.Number != 0) {
                    prop.Displace(Displacement.Number);
                }
            }
            OnLoaded(props);
        }
    }
}
