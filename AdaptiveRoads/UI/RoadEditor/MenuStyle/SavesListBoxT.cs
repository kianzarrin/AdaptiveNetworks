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

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public abstract class SavesListBoxT<T> : ListBox where T : class {
        public List<T> Saves = new List<T>();

        public abstract string GetName(T item);
        public abstract IEnumerable<T> LoadItems();

        public override void Awake() {
            base.Awake();
            Populate();
        }

        public void Populate() {
            Saves = LoadItems().ToList();
            items = Saves.Select(item => GetName(item)).ToArray();
            selectedIndex = -1;
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public T SelectedTemplate =>
            selectedIndex >= 0 ? Saves[selectedIndex] : null;

        public int IndexOf(string name) {
            for (int i = 0; i < Saves.Count; ++i) {
                if (GetName(Saves[i]) == name)
                    return i;
            }
            return -1;
        }

        public void Select(string name) {
            selectedIndex = IndexOf(name);
        }
            

        
    }
}
