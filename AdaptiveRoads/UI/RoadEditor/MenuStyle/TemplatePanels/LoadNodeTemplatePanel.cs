using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using UnityEngine;
using AdaptiveRoads.DTO;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class LoadNodeTemplatePanel : LoadTemplatePanel<SaveListBoxNode, NodeTemplate> {
        public delegate void OnLoadedHandler(NetInfo.Node[] props);
        public event OnLoadedHandler OnLoaded;
        protected override string Title => "Load Node Template";

        public static LoadNodeTemplatePanel Display(OnLoadedHandler handler) {
            Log.Called();
            var ret = UIView.GetAView().AddUIComponent<LoadNodeTemplatePanel>();
            ret.OnLoaded = handler;
            return ret;
        }

        protected override void AddCustomUIComponents(UIPanel panel) { }

        public override void Load(NodeTemplate template) {
            var nodes = template.GetNodes();
            OnLoaded(nodes);
        }
    }
}
