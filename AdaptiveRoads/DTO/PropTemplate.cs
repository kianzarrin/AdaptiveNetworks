using AdaptiveRoads.Util;
using KianCommons;
using KianCommons.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AdaptiveRoads.DTO {
    public class PropTemplate : ISerialziableDTO {
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

        public NetInfoDTO.Prop[] Props { get; private set; }
        public NetLaneProps.Prop[] GetProps() =>
            Props.Select(_item => (NetLaneProps.Prop)_item).ToArray();

        public string Summary {
            get {
                string ret = Name + $"({Date})";
                if (!string.IsNullOrEmpty(Description))
                    ret += "\n" + Description;
                var summaries = GetProps().Select(_prop => _prop.Summary());
                ret += "\n" + summaries.JoinLines();
                return ret;
            }

        }

        public static PropTemplate Create(
            string name,
            NetLaneProps.Prop[] props,
            string description) {
            var ret = new PropTemplate {
                Name = name.RemoveChars(Path.GetInvalidFileNameChars()),
                Props = props.Select(_prop => (NetInfoDTO.Prop)_prop).ToArray(),
                Description = description,
            };
            ret.Date = DateTime.UtcNow;
            ret.Version = ret.VersionOf().ToString();
            return ret;
        }

        private static MultiSerializer<PropTemplate> Serializer = new MultiSerializer<PropTemplate>("ARTemplates");
        public void Save() => Serializer.Save(Name, this);
        public void OnLoaded(FileInfo file) => Name = file.Name.RemoveExtension();
        public static IEnumerable<PropTemplate> LoadAllFiles() => Serializer.LoadAllFiles();
    }
}
