using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using System.Reflection;
using HarmonyLib;
using KianCommons.UI.Helpers;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Util;

namespace AdaptiveRoads.UI.RoadEditor.Templates {
    public class SavesListBoxT : ListBox {
        List<PropTemplate> Saves = new List<PropTemplate>();

        public override void Awake() {
            base.Awake();
            Saves = PropTemplate.LoadAll().ToList();
            items = Saves.Select(item => item.Name).ToArray();
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public PropTemplate SelectedTemplate =>
            selectedIndex > 0 ? Saves[selectedIndex] : null;

        public int IndexOf(string name) {
            for (int i = 0; i < Saves.Count; ++i) {
                if (Saves[i].Name == name)
                    return i;
            }
            return -1;
        }
            

        
    }
}
