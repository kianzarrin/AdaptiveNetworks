using AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class SaveTransitionPropTemplatePanel : SaveTemplatePanel<SaveListBoxTransitionProp, TransitionPropTemplate> {
        public List<TransitionProp> Props;

        public static SaveTransitionPropTemplatePanel Display(IEnumerable<TransitionProp> props) {
            Log.Called();
            if (props.IsNullorEmpty()) return null;
            var ret = UIView.GetAView().AddUIComponent<SaveTransitionPropTemplatePanel>();
            ret.Props = props.ToList();
            return ret;
        }

        public override string Title => "Save Prop Template";
        public override string GetItemsSummary() => Props?.Summary();
        public override ISerialziableDTO CreateTemplate() {
            return TransitionPropTemplate.Create(
                NameField.text,
                Props.ToArray(),
                DescriptionField.text);
        }
    }
}
