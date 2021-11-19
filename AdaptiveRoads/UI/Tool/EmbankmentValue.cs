namespace AdaptiveRoads.UI.Tool {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using UnityEngine;
    using ColossalFramework;
    using ColossalFramework.UI;

    public class EmbankmentValue : UITextField {
        ushort segmentID_;
        public static EmbankmentValue Add(UIPanel parent, ushort segmentID) {
            var ret = parent.AddUIComponent<EmbankmentValue>();
            ret.segmentID_ = segmentID;
            return ret;
        }

        public override string ToString() => GetType().Name + $"({name})";

        private string _postfix = ""; 
        public string PostFix {
            get => _postfix;
            set {
                if(value.IsNullOrWhiteSpace())
                    _postfix = "";
                else
                    _postfix = value;
            }
        }

    }
}
