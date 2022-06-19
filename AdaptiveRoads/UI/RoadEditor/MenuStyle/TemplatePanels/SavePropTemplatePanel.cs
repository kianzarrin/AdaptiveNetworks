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
    public class SavePropTemplatePanel : SaveTemplatePanel<SaveListBoxProp, PropTemplate> {
        public List<NetLaneProps.Prop> Props;

        public static SavePropTemplatePanel Display(IEnumerable<NetLaneProps.Prop> props) {
            Log.Debug($"SaveTemplatePanel.Display() called");
            if (props.IsNullorEmpty()) {
                return null;
            }
            var ret = UIView.GetAView().AddUIComponent<SavePropTemplatePanel>();
            ret.Props = props.ToList();
            return ret;
        }

        public override string Title => "Save Prop Template";
        public override string GetItemsSummary() => Props?.Summary();
        public override ISerialziableDTO CreateTemplate() {
            return PropTemplate.Create(
                NameField.text,
                Props.ToArray(),
                DescriptionField.text);
        }
    }
}
