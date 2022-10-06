namespace AdaptiveRoads.Patches.RoadEditor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KianCommons;
    using HarmonyLib;
    using ColossalFramework.UI;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using static ColossalFramework.Threading.ContextSwitch;

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

            void Click(UIComponent component, UIMouseEventParameter p) {
                Log.Called();
                if(!p.used && p.buttons == UIMouseButton.Right) {
                    PopupAction(component as UIButton);
                    p.Use();
                }
            }

            #region popup
            void PopupAction(UIButton button) {
                Log.Called(button);
                var panel = MiniPanel.Display();
                if (button.name != BASIC)
                    panel.AddButton("Remove", "Remove this elevation", () => RemoveElevation(button));

                if (SelectedNetInfo is NetInfo selectedBasicInfo) {
                    // if the selected prefab has a elevation that edit prefab does not then we can add elevation[s].
                    foreach (string fieldName in elevationFields) {
                        if (!TryGetElevation(fieldName) && TryGetElevation(selectedBasicInfo, fieldName)) {
                            panel.AddButton(
                                "Add",
                                "Add new elevation from the network selected in road tool",
                                PopupAddSelectedElevation);
                            break;
                        }
                    }
                    {
                        string fieldName = GetAIElevationFieldName(button);
                        if(TryGetElevation(selectedBasicInfo, fieldName) is NetInfo elevationInfo) {
                            panel.AddButton(
                                "Change AI",
                                "Change AI of this elevation to the one from the network selected in road tool.\n" +
                                "Only influences AI properties.",
                                () => ChangeAI(button));
                        }
                    }
                }
            }

            void PopupAddSelectedElevation() {
                if (SelectedNetInfo is NetInfo selectedBasicInfo) {
                    var panel = MiniPanel.Display();
                    // if the selected prefab has a elevation that edit prefab does not then we can add elevation[s].
                    foreach (string fieldName in elevationFields) {
                        if (!TryGetElevation(fieldName) && TryGetElevation(selectedBasicInfo, fieldName)) {
                            panel.AddButton(
                                GetElevationNameByAIField(fieldName),
                                "from the network selected in road tool",
                                () => TrySetElevation(fieldName, selectedBasicInfo));
                        }
                    }
                }
            }

            #endregion

            #region Util
            const string BASIC = "BASIC";
            static string[] elevationFields = new[] { "m_info", "m_elevatedInfo", "m_bridgeInfo", "m_slopeInfo", "m_tunnelInfo" };

            static NetInfo EditNetInfo => NetInfoExtionsion.EditedNetInfo;
            static NetInfo SelectedNetInfo => NetUtil.netTool.Prefab;
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
                TryGetElevation(EditNetInfo, fieldName);


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
                TrySetElevation(fieldName, null);
            }

            /// <summary>
            /// sets elevation of edit prefab for given fieldName to the given elevationInfo
            /// </summary>
            /// <param name="elevationInfo">target elevation</param>
            public void TrySetElevation(string fieldName, NetInfo elevationInfo) {
                if (fieldName == null) throw new ArgumentNullException("button");
                Log.Called(fieldName, elevationInfo);
                NetAI editAI = EditNetInfo.m_netAI;
                if(elevationInfo != null) {
                    elevationInfo = AssetEditorRoadUtils.InstantiatePrefab(elevationInfo);
                }
                editAI.GetType().GetField(fieldName)?.SetValue(editAI, elevationInfo);

                Reset();
            }

            public void ChangeAI(UIButton button) {
                string fieldName = GetAIElevationFieldName(button);
                NetAI targetAI = TryGetElevation(SelectedNetInfo, fieldName).m_netAI;
                NetInfo sourceInfo = TryGetElevation(EditNetInfo, fieldName);
                if (sourceInfo && targetAI) {
                    ChangeAI(sourceInfo, targetAI);
                    Reset();
                }
            }

            public static void ChangeAI(NetInfo sourceInfo, NetAI targetAI) {
                NetAI SourceAI = sourceInfo.m_netAI;
                targetAI = UnityEngine.Object.Instantiate(targetAI); // clone
                CopyAIProperties(SourceAI, targetAI); // NetAI -> NetInfo [+elevations]
                sourceInfo.m_netAI = targetAI; // NetInfo -> NetAI
            }

            public static void CopyAIProperties(NetAI sourceAI, NetAI targetAI) {
                foreach (string fieldName in elevationFields) {
                    try {
                        var sourceField = ReflectionHelpers.GetField(sourceAI, fieldName, throwOnError: false);
                        var sourceValue = sourceField?.GetValue(sourceAI);
                        var targetField = ReflectionHelpers.GetField(targetAI, fieldName, throwOnError: false);
                        targetField?.SetValue(targetAI, sourceValue);
                    } catch (Exception ex) { ex.Log(); }
                }
            }

            void Reset() {
                roadEditorMainPanel_.Reset(EditNetInfo);
            }
            #endregion
        }
    }
}


