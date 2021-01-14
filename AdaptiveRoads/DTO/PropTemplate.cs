using AdaptiveRoads.Util;
using AdaptiveRoads.Manager;
using KianCommons;
using KianCommons.Math;
using KianCommons.Serialization;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;


namespace AdaptiveRoads.DTO {
    public class PropSerializable {
        public NetLane.Flags m_flagsRequired;
        public int m_probability;
        public float m_cornerAngle;
        public float m_minLength;
        public float m_repeatDistance;
        public float m_segmentOffset;
        public float m_angle;
        public NetLaneProps.ColorMode m_colorMode;
        public NetNode.Flags m_endFlagsForbidden;
        public NetNode.Flags m_endFlagsRequired;
        public NetNode.Flags m_startFlagsForbidden;
        public NetNode.Flags m_startFlagsRequired;
        public NetLane.Flags m_flagsForbidden;

        public XmlVector3 m_position;
        public XmlPrefabInfo<PropInfo> m_prop;
        public XmlPrefabInfo<TreeInfo> m_tree;

        public PropSerializable() { } //xml constructor
        public PropSerializable(NetLaneProps.Prop prop) {
            ReflectionHelpers.CopyPropertiesForced<PropSerializable>(this, prop);
        }
        public NetLaneProps.Prop ToProp() {
            var prop = new NetLaneProps.Prop();
            ReflectionHelpers.CopyPropertiesForced<PropSerializable>(prop, this);
            prop.m_finalProp = prop.m_prop;
            prop.m_finalTree = prop.m_tree;
            return prop;
        }
    }

    public class PropTemplateItem  {
        [XmlIgnore] public NetLaneProps.Prop PropMain;
        public PropSerializable Prop;
        public NetInfoExtionsion.LaneProp ARMetaData;
        public string FinalName => PropMain.DisplayName();
        public string Summary => PropHelpers.Summary(PropMain, ARMetaData);

        public PropTemplateItem(NetLaneProps.Prop prop) {
            PropMain = prop.Clone();
            ARMetaData = PropMain.GetMetaData();
            Prop = new PropSerializable(prop);
        }

        public PropTemplateItem() { } //xml constructor

        public void LoadProp() {
            if (ARMetaData == null) {
                PropMain = Prop.ToProp();
            } else {
                var propExt = Prop.ToProp().Extend();
                propExt.SetMetaData(ARMetaData);
                PropMain = propExt.Base;
            }
        }
    }

    public class PropTemplate : ISerialziableDTO {
        public PropTemplateItem[] PropItems { get; private set; }
        public string Name { get; private set; }
        public string Description;
        public DateTime Date;
        public NetLaneProps.Prop[] GetProps() =>
            PropItems.Select(_item => _item.PropMain).ToArray();

        public string Summary {
            get {
                string ret = Name + $"({Date})";
                if (!string.IsNullOrEmpty(Description))
                    ret += "\n" + Description;
                var summaries = PropItems.Select(_item => _item.Summary);
                ret += "\n" + summaries.JoinLines();
                return ret;
            }

        }
        void LoadAllProps() {
            foreach (var item in PropItems)
                item.LoadProp();
        }

        public static PropTemplate Create(
            string name,
            NetLaneProps.Prop[] props,
            string description) {
            var ret = new PropTemplate {
                Name = name.RemoveChars(Path.GetInvalidFileNameChars()),
                PropItems = props.Select(_prop => new PropTemplateItem(_prop)).ToArray(),
                Description = description,
            };
            ret.Date = DateTime.UtcNow;
            return ret;
        }

        // public PropTemplate() { }

        private static MultiSerializer<PropTemplate> Serializer = new MultiSerializer<PropTemplate>("ARTemplates");
        public void Save() => Serializer.Save(Name, this);
        public void OnLoaded() => LoadAllProps();
        public static IEnumerable<PropTemplate> LoadAllFiles() => Serializer.LoadAllFiles();
    }
}
