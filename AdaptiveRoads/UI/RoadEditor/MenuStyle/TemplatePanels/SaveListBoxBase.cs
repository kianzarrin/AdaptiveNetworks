using AdaptiveRoads.DTO;
using KianCommons;
using System.Collections.Generic;
using System.Linq;
using static KianCommons.ReflectionHelpers;
using System;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SaveListBoxSegment : SaveListBoxBase<SegmentTemplate> {
        public override IEnumerable<SegmentTemplate> LoadAll() => SegmentTemplate.LoadAllFiles();
    }
    public class SaveListBoxNode : SaveListBoxBase<NodeTemplate> {
        public override IEnumerable<NodeTemplate> LoadAll() => NodeTemplate.LoadAllFiles();
    }
    public class SaveListBoxProp : SaveListBoxBase<PropTemplate> {
        public override IEnumerable<PropTemplate> LoadAll() => PropTemplate.LoadAllFiles();
    }
    public class SaveListBoxTransitionProp : SaveListBoxBase<TransitionPropTemplate> {
        public override IEnumerable<TransitionPropTemplate> LoadAll() => TransitionPropTemplate.LoadAllFiles();
    }
    public class SaveListBoxTrack : SaveListBoxBase<TrackTemplate> {
        public override IEnumerable<TrackTemplate> LoadAll() => TrackTemplate.LoadAllFiles();
    }
    public class SaveListBoxRoad : SaveListBoxBase<RoadAssetInfo> {
        public override IEnumerable<RoadAssetInfo> LoadAll() => RoadAssetInfo.LoadAllFiles();

    }

    public abstract class SaveListBoxBase<T> : ListBox
        where T : class, ISerialziableDTO {
        public List<T> Saves = new List<T>();
        public override void Awake() {
            base.Awake();
            Populate();
        }

        public abstract IEnumerable<T> LoadAll();
        public void LoadItems() {
            try {
                var saves = LoadAll();
                Saves.Clear();
                foreach (T save in saves)
                    Saves.Add(save);
            } catch(Exception ex) { ex.Log(); }
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
