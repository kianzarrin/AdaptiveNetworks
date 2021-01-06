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
using KianCommons;
using KianCommons.Math;

namespace AdaptiveRoads.Manager {
    [Serializable]
    public class PropSerializable {
        public NetLane.Flags m_flagsRequired;
        public int m_probability;
        public float m_cornerAngle;
        public float m_minLength;
        public float m_repeatDistance;
        public float m_segmentOffset;
        public float m_angle;
        public Vector3Serializable m_position;
        public NetLaneProps.ColorMode m_colorMode;
        public NetNode.Flags m_endFlagsForbidden;
        public NetNode.Flags m_endFlagsRequired;
        public NetNode.Flags m_startFlagsForbidden;
        public NetNode.Flags m_startFlagsRequired;
        public NetLane.Flags m_flagsForbidden;

        public PropSerializable(NetLaneProps.Prop prop) {
            ReflectionHelpers.CopyPropertiesForced<PropSerializable>(this, prop);
            m_position = prop.m_position;
        }
        public NetLaneProps.Prop ToProp() {
            var prop = new NetLaneProps.Prop();
            ReflectionHelpers.CopyPropertiesForced<PropSerializable>(prop, this);
            prop.m_position = m_position;
            return prop;
        }
    }

    [Serializable]
    public class PropTemplateItem {
        [NonSerialized] public NetLaneProps.Prop Prop;
        public PropSerializable Prop2;
        public NetInfoExtionsion.LaneProp PropData;
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
        public string Desciption => PropHelpers.Summary(Prop, PropData, FinalName);
        
        public static PropTemplateItem Create(NetLaneProps.Prop prop) {
            prop = prop.Clone();
            var propExt = prop.GetMetaData();
            if (prop is IInfoExtended<NetLaneProps.Prop> prop2)
                prop = prop2.UndoExtend();
            var ret = new PropTemplateItem {
                Prop = prop,
                Prop2 = new PropSerializable(prop),
                PropData = propExt,
                PropInfoName = prop.m_prop?.name,
                TreeInfoName = prop.m_tree?.name,
            };
            return ret;
        }


        public void LoadProp() {
            var propExt = Prop2.ToProp().Extend();
            propExt.SetMetaData(PropData);
            Prop = propExt.Base;

            if (!string.IsNullOrEmpty(PropInfoName))
                Prop.m_prop = PrefabCollection<PropInfo>.FindLoaded(PropInfoName);
            if (!string.IsNullOrEmpty(TreeInfoName))
                Prop.m_tree = PrefabCollection<TreeInfo>.FindLoaded(TreeInfoName);
            Prop.m_finalProp = Prop.m_prop;
            Prop.m_finalTree = Prop.m_tree;
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
            EnsureDir();
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
            EnsureDir();
            try {
                using (FileStream fs = File.OpenRead(path)) {
                    var version = fs.ReadVersion();
                    var data = fs.ReadToEnd();
                    var data2 = SerializationUtil.Deserialize(data, version);
                    AssertNotNull(data2, "data2");
                    var propTemplate = data2 as PropTemplate;
                    AssertNotNull(propTemplate, "propTemplate");
                    propTemplate.LoadAllProps();
                    return propTemplate;
                }
            } catch(Exception ex) {
                Log.Exception(ex, showInPanel: false);
                return null;
            }
        }

        public static IEnumerable<PropTemplate> LoadAllFiles() {
            EnsureDir();
            var dir = new DirectoryInfo(Dir);
            var files = dir.GetFiles("*.dat");
            foreach(var file in files) {
                var ret = LoadFile(file.FullName);
                if (ret != null)
                    yield return ret;
            }
                
        }

        public static void EnsureDir() {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
        }
    }
}
