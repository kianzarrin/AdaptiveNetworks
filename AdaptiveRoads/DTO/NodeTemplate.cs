using AdaptiveRoads.Util;
using KianCommons;
using KianCommons.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AdaptiveRoads.DTO {
    public class NodeTemplate : ISerialziableDTO {
        [XmlIgnore]
        private Version version_;
        [XmlAttribute]
        public string Version {
            get => version_.ToString();
            set => version_ = new Version(value);
        }

        [XmlIgnore]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date;

        public NetInfoDTO.Node[] Nodes { get; private set; }
        public NetInfo.Node[] GetNodes() =>
            Nodes.Select(_item => (NetInfo.Node)_item).ToArray();

        public string Summary {
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
            var ret = new NodeTemplate {
                Name = name.RemoveChars(Path.GetInvalidFileNameChars()),
                Nodes = nodes.Select(_item => (NetInfoDTO.Node)_item).ToArray(),
                Description = description,
            };
            ret.Date = DateTime.UtcNow;
            ret.Version = ret.VersionOf().ToString();
            return ret;
        }

        private static MultiSerializer<NodeTemplate> Serializer = new MultiSerializer<NodeTemplate>("ARTemplates");
        public void Save() => Serializer.Save(Name, this);
        public void OnLoaded(FileInfo file) => Name = file.Name.RemoveExtension();
        public static IEnumerable<NodeTemplate> LoadAllFiles() => Serializer.LoadAllFiles();
    }
}
