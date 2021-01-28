namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using AdaptiveRoads.Util;
    using System.Reflection;
    using System.Linq;
    using KianCommons.UI.Helpers;
    using AdaptiveRoads.Manager;


    public class EnumSetHint : IHint  {
        readonly REEnumSet enumSet_;
        UIDropDown dd => enumSet_.m_DropDown;
        UIPanel panel_;

        public EnumSetHint(REEnumSet enumSet) {
            enumSet_ = enumSet;
            panel_ = enumSet_.GetComponentInChildren<UIPanel>();
        }

        public bool IsHovered() {
            if (panel_.containsMouse)
                return true;
            if (dd.GetHoverIndex() >= 0)
                return true;
            return false;
        }

        public string GetHint() {
            Type enumType = enumSet_.GetTargetField().FieldType;
            int i = dd.GetHoverIndex();
            if (i >= 0) {
                string itemName = dd.items[i];
                return HintExtension.GetEnumMappedHint(enumType, itemName);
            } else if (dd.containsMouse) {
                Type enumType2 = HintExtension.GetMappedEnumWithHints(enumType);
                return enumType2.GetHints().JoinLines();
            }
            return null;
        }
    }
}
