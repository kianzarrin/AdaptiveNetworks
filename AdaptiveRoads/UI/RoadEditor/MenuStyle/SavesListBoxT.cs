using AdaptiveRoads.DTO;
using KianCommons;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using static KianCommons.ReflectionHelpers;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SavesListBoxT<T> : ListBox
    where T : class, ISerialziableDTO {
        public List<T> Saves = new List<T>();

        public override void Awake() {
            base.Awake();
            Populate();
        }

        public void LoadItems() {
            var saves = InvokeMethod(typeof(T), "LoadAllFiles") as IEnumerable;
            Saves.Clear();
            foreach (T save in saves)
                Saves.Add(save);
        }

        public void Populate() {
            LoadItems();
            items = Saves.Select(item => item.Name).ToArray();
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
                if (Saves[i].Name == name)
                    return i;
            }
            return -1;
        }

        public void Select(string name) {
            selectedIndex = IndexOf(name);
        }



    }
}
