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
using PrefabMetadata.Helpers;
using PrefabMetadata.API;

namespace AdaptiveRoads.Manager {
    [Serializable]
    public class PropTemplateItem {
        public NetLaneProps.Prop Prop;
        public NetInfoExtionsion.LaneProp PropExt;
        public string Name;
        public string PropInfoName;
        public string TreeInfoName;

        public string FinalName {
            get {
                if (!string.IsNullOrEmpty(PropInfoName))
                    return PropInfoName;
                if (!string.IsNullOrEmpty(TreeInfoName))
                    return TreeInfoName;
                return "New Prop";
            }
        }
        public string Desciption => PropHelpers.Summary(Prop, PropExt, FinalName);
        
        public static PropTemplateItem Create(NetLaneProps.Prop prop) {
            prop = prop.Clone();
            var propExt = prop.GetMetaData();
            if (prop is IInfoExtended<NetLaneProps.Prop> prop2)
                prop = prop2.UndoExtend();
            var ret = new PropTemplateItem {
                Prop = prop,
                PropExt = propExt,
                PropInfoName = prop.m_prop?.name,
                TreeInfoName = prop.m_tree?.name,
            };
            ret.ClearProps();
            return ret;
        }

        public void LoadProp() {
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

            var prop2 = Prop.Extend();
            prop2.SetMetaData(PropExt);
            Prop = prop2.Base;
        }

        public void ClearProps() {
            Prop.m_finalProp = Prop.m_prop = null;
            Prop.m_finalTree = Prop.m_tree = null;
        }
    }

    [Serializable]
    public class PropTemplate {
        public PropTemplateItem[] PropItems { get; private set; }
        public string Name { get; private set; }
        public string Description;
        public DateTime Date;

        public string Summary {
            get {
                string ret = Name + $"({Date})";
                if (!string.IsNullOrEmpty(Description))
                    ret += "\n" + Description;
                ret += "\n" + PropItems.AsEnumerable()
                    .Select(_item => _item.Desciption);
                return ret;
            }

        }
        public NetLaneProps.Prop [] LoadAllProps() {
            foreach (var item in PropItems)
                item.LoadProp();
            return PropItems.Select(_item => _item.Prop).ToArray();
        }

        public static string Dir => Path.Combine(DataLocation.localApplicationData, "ARTemplates");
        public static string FilePath(string name) => Path.Combine(Dir, name + ".dat");

        public static PropTemplate Create(
            string name,
            NetLaneProps.Prop[] props,
            string description) {
            var ret = new PropTemplate {
                Name = name.Trim(Path.GetInvalidFileNameChars()),
                PropItems = props.Select(_prop => PropTemplateItem.Create(_prop)).ToArray(),
                Description = description,
                Date = DateTime.UtcNow,
            };
            return ret;
        }

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
            return Load(FilePath(name));
             
        }
        public static PropTemplate LoadFile(string path) {
            using (FileStream fs = File.OpenRead(path)) {
                var version = fs.ReadVersion();
                var data = fs.ReadToEnd();
                var data2 = SerializationUtil.Deserialize(data, version);
                AssertNotNull(data2, "data2");
                var propTemplate = data2 as PropTemplate;
                AssertNotNull(propTemplate, "propTemplate");
                return propTemplate;
            }
        }

        public static IEnumerable<PropTemplate> LoadAll() {
            var dir = new DirectoryInfo(Dir);
            var files = dir.GetFiles("*.dat");
            foreach(var file in files)
                yield return LoadFile(file.FullName);
        }
    }
}
