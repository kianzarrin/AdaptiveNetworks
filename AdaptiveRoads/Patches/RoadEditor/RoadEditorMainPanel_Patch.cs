namespace AdaptiveRoads.Patches.RoadEditor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KianCommons;
    using HarmonyLib;
    using ColossalFramework.UI;
    using AdaptiveRoads.UI.RoadEditor;
    using static ColossalFramework.UI.UIInput;
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using UnityEngine.UI;

    [HarmonyPatch(typeof(RoadEditorMainPanel), "AddTab")]
    static class RoadEditorMainPanel_AddTab_Patch {
        static void Postfix(RoadEditorMainPanel __instance, string name, NetInfo info) {
            if (info == null) return;
            RoadEditorMainPanel roadEditorMainPanel = __instance;
            if (roadEditorMainPanel.component.objectUserData is not MouseHandler)
                roadEditorMainPanel.component.objectUserData = new MouseHandler(roadEditorMainPanel);
            MouseHandler handler = roadEditorMainPanel.component.objectUserData as MouseHandler;

            UIButton button = roadEditorMainPanel.m_ElevationsTabstrip.Find<UIButton>(name);
            handler.Add(button);
        }

        class MouseHandler : IHint {
            List<UIButton> m_Buttons = new(10);
            RoadEditorMainPanel roadEditorMainPanel_;
            public MouseHandler(RoadEditorMainPanel roadEditorMainPanel) => roadEditorMainPanel_ = roadEditorMainPanel;
            public void Add(UIButton button) {
                if (button == null) throw new ArgumentNullException("button");
                Log.Called(button.name);
                button.buttonsMask = UIMouseButton.Left | UIMouseButton.Right;
                button.eventMouseUp += Click;
                m_Buttons.Add(button);
            }

            private void Click(UIComponent component, UIMouseEventParameter p) {
                Log.Called();
                if(!p.used && p.buttons == UIMouseButton.Right) {
                    PopupMiniPanelFor(component as UIButton);
                    p.Use();
                }
            }

            const string BASIC = "BASIC";
            public void PopupMiniPanelFor(UIButton button) {
                Log.Called(button);
                var panel = MiniPanel.Display();
                if (button.name != BASIC) {
                    panel.AddButton("Remove", "Remove this elevation", () => RemoveElevation(button));
                }
                //panel.AddButton("Add", "Add new elevation", () => { });
                //panel.AddButton("Change AI", "Chane AI for this elevation", () => { });
            }

            public void RemoveElevation(UIButton button) {
                if (button == null) throw new ArgumentNullException("button");
                string name = button?.name;
                Log.Called(name);
                var baseInfo = NetInfoExtionsion.EditedNetInfo;
                var baseAI = baseInfo.m_netAI;
                string fieldName = name switch {
                    BASIC => "m_info",
                    "Elevated" => "m_elevatedInfo",
                    "Bridge" => "m_bridgeInfo",
                    "Slope" => "m_slopeInfo",
                    "Tunnel" => "m_tunnelInfo",
                    _ => throw new NotImplementedException(name),
                };
                baseAI.GetType().GetField(fieldName)?.SetValue(baseAI, null);
                roadEditorMainPanel_.Reset(baseInfo);
            }

            public string GetHint() => "Right-click => more actions";

            public bool IsHovered() => m_Buttons.Any(b => b.containsMouse);
        }
    }
}


