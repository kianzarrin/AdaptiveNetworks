using AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SaveNodeTemplatePanel : SaveTemplatePanel<SaveListBoxNode, NodeTemplate> {
        public List<NetInfo.Node> Nodes;

        public static SaveNodeTemplatePanel Display(IEnumerable<NetInfo.Node> nodes) {
            Log.Called();
            if (nodes.IsNullorEmpty()) {
                return null;
            }
            var ret = UIView.GetAView().AddUIComponent<SaveNodeTemplatePanel>();
            ret.Nodes = nodes.ToList();
            return ret;
        }

        public override string Title => "Save Prop Template";
        public override string GetItemsSummary() => Nodes?.Summary();
        public override ISerialziableDTO CreateTemplate() {
            return NodeTemplate.Create(
                NameField.text,
                Nodes.ToArray(),
                DescriptionField.text);
        }
    }
}
