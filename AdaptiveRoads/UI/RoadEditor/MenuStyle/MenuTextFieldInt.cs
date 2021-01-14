using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class MenuTextFieldInt : MenuTextField {
        public override void Awake() {
            base.Awake();
            allowFloats = false;
            numericalOnly = true;
            allowNegative = true;
            text = "0";
            height = 29;
            textScale = 1;
        }
        public int Number {
            get {
                if (int.TryParse(text, out int ret))
                    return ret;
                return 0;
            }
        }
    }
}

