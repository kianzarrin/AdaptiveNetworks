using AdaptiveRoads.Util;
using KianCommons;
using KianCommons.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AdaptiveRoads.DTO {
    public class PropTemplate : TemplateBase<PropTemplate> {
        public NetInfoDTO.Prop[] Props { get; private set; }
        public NetLaneProps.Prop[] GetProps() =>
            Props.Select(_item => (NetLaneProps.Prop)_item).ToArray();

        public override string Summary {
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
            return new PropTemplate {
                Name = name,
                Props = props.Select(_prop => (NetInfoDTO.Prop)_prop).ToArray(),
                Description = description,
            };
        }
    }
}
