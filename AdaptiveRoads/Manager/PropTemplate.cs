using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveRoads.Util;
using KianCommons;
using static KianCommons.Assertion;
using System.IO;
using ColossalFramework.IO;
using UnityEngine;

namespace AdaptiveRoads.Manager {
    [Serializable]
    public class PropTemplateItem {
        public NetLaneProps.Prop Prop;
        public NetInfoExtionsion.LaneProp propExt;
        public string Name;
        public string PropInfoName;
        public string TreeInfoName;


        public static PropTemplateItem Create(NetLaneProps.Prop prop) {
            prop = prop.Clone();
            var ret = new PropTemplateItem {
                Prop = prop,
                propExt = prop.GetMetaData(),
                PropInfoName = prop.m_prop?.name,
                TreeInfoName = prop.m_tree?.name,
            };
            ret.ClearProps();
            return ret;
        }

        public void LoadProps() {
            if (!string.IsNullOrEmpty(PropInfoName)) {
                Prop.m_prop = GameObject.FindObjectsOfType<PropInfo>()
                    .FirstOrDefault(_item => _item.name == PropInfoName);
            }
            if (!string.IsNullOrEmpty(TreeInfoName)) {
                Prop.m_tree = GameObject.FindObjectsOfType<TreeInfo>()
                    .FirstOrDefault(_item => _item.name == TreeInfoName);
            }
            Prop.m_finalProp = Prop.m_prop;
            Prop.m_finalTree = Prop.m_tree;
        }

        public void ClearProps() {
            Prop.m_finalProp = Prop.m_prop = null;
            Prop.m_finalTree = Prop.m_tree = null;
        }
    }

    [Serializable]
    public class PropTemplate {
        public PropTemplateItem[] Props { get; private set; }
        public string Name { get; private set; }

        public static string Dir => Path.Combine(DataLocation.localApplicationData, "ARTemplates");
        public static string FilePath(string name) => Path.Combine(Dir, name + ".dat");

        public static PropTemplate Create(string name, NetLaneProps.Prop[] props) {
            var ret = new PropTemplate {
                Name = name.Trim(Path.GetInvalidFileNameChars()),
                Props = props.Select(_prop => PropTemplateItem.Create(_prop)).ToArray(),
            };
            return ret;
        }

        public static PropTemplate Create(string name, NetLaneProps.Prop prop) =>
            Create(name, new[] { prop });

        public void Save() {
            if (Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new Exception($"Name:{Name} contains invalid characters");

            using (FileStream fs = File.Create(FilePath(Name))) {
                fs.Write(this.VersionOf());
                fs.Write(SerializationUtil.Serialize(this));
            }
        }

        public static PropTemplate Load(string name) {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException($"name:{name} contains invalid characters");
            
            using (FileStream fs = File.OpenRead(FilePath(name))) {
                var version = fs.ReadVersion();
                var data = fs.ReadToEnd();
                var data2 = SerializationUtil.Deserialize(data, version);
                AssertNotNull(data2, "data2");
                var propTemplate = data2 as PropTemplate;
                AssertNotNull(propTemplate, "propTemplate");
                return propTemplate;
            }
        }
    }
}
