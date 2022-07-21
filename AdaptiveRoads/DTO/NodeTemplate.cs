using AdaptiveRoads.Util;
using KianCommons;
using System;
using System.IO;
using System.Linq;

namespace AdaptiveRoads.DTO {
    public class NodeTemplate : TemplateBase<NodeTemplate> {
        public NetInfoDTO.Node[] Nodes { get; private set; }
        public NetInfo.Node[] GetNodes() =>
            Nodes.Select(_item => (NetInfo.Node)_item).ToArray();

        public override string Summary {
            get {
                string ret = Name + $"({Date})";
                if (!string.IsNullOrEmpty(Description))
                    ret += "\n" + Description;
                var summaries = GetNodes().Select(_item => _item.Summary());
                ret += "\n" + summaries.JoinLines();
                return ret;
            }
        }

        public static NodeTemplate Create(
            string name,
            NetInfo.Node[] nodes,
            string description) {
            return new NodeTemplate {
                Name = name,
                Nodes = nodes.Select(_item => (NetInfoDTO.Node)_item).ToArray(),
                Description = description,
            };
        }
    }
}
