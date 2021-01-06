using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using System.Reflection;
using AdaptiveRoads.Manager;
using TrafficManager.API.Traffic;
using System;

namespace AdaptiveRoads.UI.RoadEditor.Templates {
    public class AddTemplatebutton : EditorButon {
        public override void Awake() {
            base.Awake();
            width = 200;
            text = "Add from Template";
        }
    }
}
