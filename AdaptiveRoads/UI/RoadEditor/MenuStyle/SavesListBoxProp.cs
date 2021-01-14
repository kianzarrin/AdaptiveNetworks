using AdaptiveRoads.DTO;
using System.Collections.Generic;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SavesListBoxProp : SavesListBoxT<PropTemplate> {
        public override IEnumerable<PropTemplate> LoadItems() => PropTemplate.LoadAllFiles();
        public override string GetName(PropTemplate item) => item.Name;
    }
}
