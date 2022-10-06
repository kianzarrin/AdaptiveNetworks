namespace AdaptiveRoads.Patches.RoadEditor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using HarmonyLib;
    using ColossalFramework.UI;
    using AdaptiveRoads.UI.RoadEditor;
    using static ColossalFramework.UI.UIInput;
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using UnityEngine.UI;
    using System.Xml.Linq;
    using System.Reflection;
    using AdaptiveRoads.UI.RoadEditor.Bitmask;

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
            public string GetHint() =>
                "Right-click => more actions.\n" +
                "to add elevation first select a road in the road tool.";
            public bool IsHovered() => m_Buttons.Any(b => b.containsMouse);

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
                    PopupAction(component as UIButton);
                    p.Use();
                }
            }

            #region Util
            const string BASIC = "BASIC";
            static string[] elevationFields = new[] { "m_info", "m_elevatedInfo", "m_bridgeInfo", "m_slopeInfo", "m_tunnelInfo" };

            public static string GetAIElevationFieldName(UIButton button) => button.name switch {
                BASIC => "m_info",
                "Elevated" => "m_elevatedInfo",
                "Bridge" => "m_bridgeInfo",
                "Slope" => "m_slopeInfo",
                "Tunnel" => "m_tunnelInfo",
                _ => throw new NotImplementedException(button.name),
            };
            public static string GetElevationNameByAIField(string fieldName) => fieldName switch {
                "m_info" => BASIC,
                "m_elevatedInfo" => "Elevated",
                "m_bridgeInfo" => "Bridge",
                "m_slopeInfo" => "Slope",
                "m_tunnelInfo" => "Tunnel",
                _ => throw new NotImplementedException(fieldName),
            };

            public static IEnumerable<string> GetElevationFields() {
                foreach (var fieldName in elevationFields) {
                    if (TryGetElevation(fieldName) != null)
                        yield return fieldName;
                }
                yield break;
            }
            private static NetInfo TryGetElevation(string fieldName) =>
                TryGetElevation(NetInfoExtionsion.EditedNetInfo, fieldName);


            private static NetInfo TryGetElevation(NetInfo info, string fieldName) {
                if (fieldName.IsNullorEmpty()) throw new ArgumentException("fieldName:" + fieldName);
                NetAI baseAI = info?.m_netAI;
                if (baseAI != null) {
                    return ReflectionHelpers.GetField(baseAI, fieldName, throwOnError: false)
                        ?.GetValue(baseAI) as NetInfo;
                } else {
                    return null;
                }
            }
            public void RemoveElevation(UIButton button) {
                if (button == null) throw new ArgumentNullException("button");
                Log.Called(button.name);
                string fieldName = GetAIElevationFieldName(button);
                SetElevation(fieldName, null);
            }

            /// <summary>
            /// sets elevation of edit prefab for given fieldName to the given elevationInfo
            /// </summary>
            /// <param name="elevationInfo">target elevation</param>
            public void SetElevation(string fieldName, NetInfo elevationInfo) {
                if (fieldName == null) throw new ArgumentNullException("button");
                Log.Called(fieldName, elevationInfo);
                NetInfo baseInfo = NetInfoExtionsion.EditedNetInfo;
                NetAI baseAI = baseInfo.m_netAI;
                baseAI.GetType().GetField(fieldName)?.SetValue(baseAI, elevationInfo);
                roadEditorMainPanel_.Reset(baseInfo);
            }
            #endregion

            #region popup
            public void PopupAction(UIButton button) {
                Log.Called(button);
                var panel = MiniPanel.Display();
                if (button.name != BASIC)
                    panel.AddButton("Remove", "Remove this elevation", () => RemoveElevation(button));

                if (NetUtil.netTool.Prefab is NetInfo selectedBasicInfo) {
                    // if the selected prefab has a elevation that edit prefab does not then we can add elevation[s].
                    foreach (string fieldName in elevationFields) {
                        if (!TryGetElevation(fieldName) && TryGetElevation(selectedBasicInfo, fieldName)) {
                            panel.AddButton("Add", "Add new elevation", PopupAddSelectedElevation);
                            break;
                        }
                    }
                }
                //panel.AddButton("Change AI", "Chane AI for this elevation", () => { });
            }

            public void PopupAddSelectedElevation() {
                if (NetUtil.netTool.Prefab is NetInfo selectedBasicInfo) {
                    var panel = MiniPanel.Display();
                    // if the selected prefab has a elevation that edit prefab does not then we can add elevation[s].
                    foreach (string fieldName in elevationFields) {
                        if (!TryGetElevation(fieldName) && TryGetElevation(selectedBasicInfo, fieldName)) {
                            panel.AddButton(
                                GetElevationNameByAIField(fieldName),
                                null,
                                () => SetElevation(fieldName, selectedBasicInfo));
                        }
                    }
                }
            }

            //public void PopupAddElevation(UIButton button) {
            //    if (button == null) throw new ArgumentNullException("button");
            //    Log.Called(button.name);
            //    var panel = MiniPanel.Display();
            //    foreach(string fieldName in elevationFields) {
            //        if(!TryGetElevation(fieldName)) {
            //            panel.AddButton(
            //                GetElevationNameByAIField(fieldName),
            //                null,
            //                () => PopupChooseElevation(fieldName));
            //        }
            //    }
            //}

            //public void PopupChooseElevation(string fieldName) {
            //    if (fieldName == null) throw new ArgumentNullException("button");
            //    Log.Called(fieldName);
            //    MiniPanel minipanel = MiniPanel.Display();
            //    var dd = minipanel.AddUIComponent<EditorDropDown>();
            //    Dictionary<int, NetInfo> addedNetInfos = new(1000);
            //    foreach (NetInfo info in RoadUtils.IterateLoadedNetInfos()) {
            //        if (TryGetElevation(info, fieldName) is NetInfo elevationInfo) {
            //            if (!addedNetInfos.ContainsValue(elevationInfo)) {
            //                dd.AddItem(elevationInfo.name);
            //                int last = dd.items.Length - 1;
            //                addedNetInfos[last] = elevationInfo;
            //            }
            //        }
            //    }
            //    dd.eventSelectedIndexChanged += (_, selectedIndex) => {
            //        if (selectedIndex != -1) {
            //            SetElevation(fieldName, addedNetInfos[selectedIndex]);
            //            minipanel.Close();
            //        }
            //    };
            //}
            #endregion



        }
    }
}


