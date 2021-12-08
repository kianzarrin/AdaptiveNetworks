using AdaptiveRoads.Manager;
using AdaptiveRoads.UI;
using AdaptiveRoads.UI.RoadEditor;
using AdaptiveRoads.UI.RoadEditor.MenuStyle;
using ColossalFramework.UI;
using HarmonyLib;
using KianCommons;
using PrefabMetadata.API;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static AdaptiveRoads.Util.DPTHelpers;
using static KianCommons.ReflectionHelpers;
using Object = UnityEngine.Object;

namespace AdaptiveRoads.Util {
    internal static class DPTDrag {
        static UIPanel Bar;

        public static void OnDragStart(UICustomControl dpt, UIDragEventParameter eventParam) {
            if(RoadEditorUtils.IsDPTSelected(dpt)) {
                LogCalled();
                eventParam.Use();
                eventParam.state = UIDragDropState.Dragging;

                var groupPanel = dpt.GetComponentInParent<RoadEditorCollapsiblePanel>();
                groupPanel.m_Panel.eventDragLeave -= OnLeave;
                groupPanel.m_Panel.eventDragEnter -= OnEnter;
                groupPanel.m_Panel.eventDragOver -= OnOver;
                groupPanel.m_Panel.eventDragDrop -= OnDrop;
                groupPanel.m_Panel.eventDragLeave += OnLeave;
                groupPanel.m_Panel.eventDragEnter += OnEnter;
                groupPanel.m_Panel.eventDragOver += OnOver;
                groupPanel.m_Panel.eventDragDrop += OnDrop;

                Log.Debug("creating `Add from template` button");
                Bar = groupPanel.Container.AddUIComponent<UIPanel>();
                Bar.width = groupPanel.m_Panel.width;
                Bar.height = 1;
                Bar.backgroundSprite = "TextFieldPanel";
                Bar.atlas = KianCommons.UI.TextureUtil.Ingame;
                Bar.color = Color.red;
                Bar.name = "ARDragPointer";
                Bar.Show();
                Bar.zOrder = CalculateZOrder(groupPanel, eventParam);
                Bar.zOrder = CalculateZOrder(groupPanel, eventParam);

                // disable moving dpts
                foreach(var selectedDPT in RoadEditorUtils.SelectedDPTs) {
                    selectedDPT.component.isEnabled = false;
                    selectedDPT.component.opacity = 0.5f;
                }
            }
        }

        static void OnLeave(UIComponent _, UIDragEventParameter __) => Bar?.Hide();
        static void OnEnter(UIComponent _, UIDragEventParameter __) => Bar?.Show();

        static void OnOver(UIComponent c, UIDragEventParameter e) {
            try {
                LogCalled();
                e.Use();
                var groupPanel = c.GetComponentInParent<RoadEditorCollapsiblePanel>();

                Bar.zOrder = CalculateZOrder(groupPanel, e);
                Bar.zOrder = CalculateZOrder(groupPanel, e);

                Bar.isVisible = true;
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        static void OnDrop(UIComponent c, UIDragEventParameter e) {
            try {
                LogCalled();
                var groupPanel = c.GetComponentInParent<RoadEditorCollapsiblePanel>();
                int z = Bar.zOrder;
                foreach(var dpt in RoadEditorUtils.SelectedDPTsSorted) {
                    dpt.component.zOrder = z;
                    z = dpt.component.zOrder; // put next dpt after this
                }

                RearrangeArray(groupPanel);
                e.Use();
                e.state = UIDragDropState.Dropped;
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void OnDragEnd(UICustomControl dpt, UIDragEventParameter e) {
            try {
                Assertion.NotNull(dpt, "dpt");
                LogCalled();
                e.Use();
                GameObject.Destroy(Bar?.gameObject);
                Bar = null;
                foreach(var selectedDPT in RoadEditorUtils.SelectedDPTsSorted) {
                    selectedDPT.component.isEnabled = true;
                    selectedDPT.component.opacity = 1;
                }

                var groupPanel = dpt.GetComponentInParent<RoadEditorCollapsiblePanel>();
                groupPanel.m_Panel.eventDragLeave -= OnLeave;
                groupPanel.m_Panel.eventDragEnter -= OnEnter;
                groupPanel.m_Panel.eventDragOver -= OnOver;
                groupPanel.m_Panel.eventDragDrop -= OnDrop;

            } catch(Exception ex) {
                Log.Exception(ex);
            }

        }

        /// <summary>
        /// rearange group's array based on zorder of the DPTs
        /// </summary>
        public static void RearrangeArray(RoadEditorCollapsiblePanel groupPanel) {
            try {
                var ar = groupPanel.GetArray().Clone() as Array; // shallow clone
                var sortedDPTs = SortedDPTs(groupPanel);
                for(int i = 0; i < sortedDPTs.Length; ++i) {
                    var element = GetDPTTargetElement(sortedDPTs[i]);
                    ar.SetValue(element, i);
                }
                groupPanel.SetArray(ar);

                RoadEditorUtils.RefreshAllNetworks();
            } catch(Exception ex) {
                Log.Exception(ex);
                throw ex;
            }
        }

        static int CalculateZOrder(RoadEditorCollapsiblePanel groupPanel, UIDragEventParameter _) {
            var dpts = SortedDPTs(groupPanel);
            float mouseY = RoadEditorUtils.MouseGUIPosition().y;
            //Log.Debug(ThisMethod + "mouseY = " + mouseY);
            foreach(var dpt in dpts) {
                float dptY = dpt.component.absolutePosition.y + dpt.component.height;
                if(mouseY < dptY)
                    return Mathf.Max(dpt.component.zOrder - 1, 0);
            }
            int lastZOrder = dpts[dpts.Length - 1].component.zOrder;
            return lastZOrder+1;
        }

        static UICustomControl[] SortedDPTs(RoadEditorCollapsiblePanel groupPanel) {
            var dpts = groupPanel.GetComponentsInChildren(DPTType).Cast<UICustomControl>();
            return dpts.OrderBy(_dpt => _dpt.component.zOrder).ToArray();
        }
    }

    internal static class RoadEditorUtils {
        public static NetInfo GetSelectedNetInfo(out RoadEditorPanel roadEditorPanel) {
            var mainPanel = UIView.GetAView().GetComponentInChildren<RoadEditorMainPanel>();
            var tabContainer = mainPanel.m_ElevationsTabstrip.tabContainer;
            roadEditorPanel = tabContainer.GetComponentsInChildren<RoadEditorPanel>().FirstOrDefault(item => item.component.isVisible);
            var netInfo = roadEditorPanel?.GetTarget() as NetInfo;
            return netInfo;
        }

        public static Vector3 MouseGUIPosition() {
            var uiView = UIView.GetAView();
            return uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
        }

        public static List<UICustomControl> SelectedDPTs = new List<UICustomControl>();
        public static IOrderedEnumerable<UICustomControl> SelectedDPTsSorted =>
            SelectedDPTs.OrderBy(_dpt => _dpt.component.zOrder);

        public static bool IsDPTSelected(UICustomControl dpt) => SelectedDPTs.Contains(dpt);

        public static void OnToggleDPT(UICustomControl dpt) {
            Assertion.NotNull(dpt, "dpt");
            VerifySelectedDPTs(dpt);
            if(IsDPTSelected(dpt))
                DeselectDPT(dpt);
            else
                SelectDPT(dpt);
        }

        /// <summary>
        /// verifies selected DPTs are active and match the input DPT.
        /// if not a match, it is removed from selection.
        /// </summary>
        public static void VerifySelectedDPTs(UICustomControl dpt) {
            Assertion.NotNull(dpt, "dpt");
            bool predicateRemove(UICustomControl dpt2) {
                if(!dpt2 || !dpt2.isActiveAndEnabled) return true;
                var field1 = GetDPTField(dpt);
                var field2 = GetDPTField(dpt2);
                Log.Debug($"field1={field1} and field2={field2}");
                return field1 != field2;
            }
            var removeDPTs = SelectedDPTs.Where(predicateRemove).ToList();
            foreach(var dpt3 in removeDPTs)
                DeselectDPT(dpt3);
        }

        static void SelectDPT(UICustomControl dpt) {
            try {
                Assertion.NotNull(dpt, "dpt");
                SetDPTColor(dpt, SELECT_COLOR);
                SelectedDPTs.Add(dpt);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        static void DeselectDPT(UICustomControl dpt) {
            try {
                Assertion.NotNull(dpt, "dpt");
                ToggleDPTColor(dpt, false);
                SelectedDPTs.Remove(dpt);
            } catch { }
        }

        public static void DeselectAllDPTs() {
            foreach(var dpt in SelectedDPTs) {
                try {
                    ToggleDPTColor(dpt, false);
                } catch { }
            }
            SelectedDPTs.Clear();
        }

        static Color SELECT_COLOR = new Color32(188, 255, 206, 255);
        public static void SetDPTColor(UICustomControl dpt, Color c) {
            Assertion.NotNull(dpt, "dpt");
            var m_SelectButton = GetDPTSelectButton(dpt);
            m_SelectButton.color = c;
            m_SelectButton.focusedColor = c;
            m_SelectButton.hoveredColor = c;
            m_SelectButton.pressedColor = c;
            m_SelectButton.disabledColor = c;
        }

        public static void OnDPTMoreOptions(UICustomControl dpt) {
            Assertion.NotNull(dpt, "dpt");
            Log.Debug("OnDPTMoreOptions() called");
            VerifySelectedDPTs(dpt);
            if(!SelectedDPTs.Contains(dpt)) {
                DeselectAllDPTs();
            }

            var groupPanel = dpt.GetComponentInParent<RoadEditorCollapsiblePanel>();
            var sidePanel = dpt.GetComponentInParent<RoadEditorPanel>();
            Log.Debug($"dpt={dpt} " +
                $"groupPanel={groupPanel} " +
                $"sidePanel={sidePanel}");

            object target = GetDPTTargetObject(dpt);
            object element = GetDPTTargetElement(dpt);
            IEnumerable<object> elements;
            if(SelectedDPTs.Any())
                elements = SelectedDPTs.Select(_dpt => GetDPTTargetElement(_dpt));
            else
                elements = new object[] { element };

            if(target is NetLaneProps netLaneProps
                && element is NetLaneProps.Prop) {
                var lane = sidePanel.GetTarget() as NetInfo.Lane;
                Assertion.AssertNotNull(lane, "sidePanel.target is lane");
                bool forward = lane.IsGoingForward();
                bool backward = lane.IsGoingBackward();
                bool unidirectional = forward || backward;

                var panel = MiniPanel.Display();
                var f_props = typeof(NetLaneProps).GetField(nameof(NetLaneProps.m_props));
                var original_props = elements.Select(_p => _p as NetLaneProps.Prop);
                var cloned_props = original_props.Select(_p => _p.Clone());
                string strAll = cloned_props.Count() > 1 ? " all" : "";

                panel.AddButton("Duplicate" + strAll, null, delegate () {
                    AddProps(groupPanel, cloned_props.ToArray());
                });

                if(cloned_props.Any(_p => _p.CanInvert())) {
                    string hint = HintExtension.GetHintSwichLHT_RHT(unidirectional);
                    panel.AddButton(
                        "LHT duplicate" + strAll,
                        hint,
                        delegate () {
                            try {
                                var arProsp = cloned_props.ToArray();
                                foreach(var item in arProsp)
                                    item.ToggleRHT_LHT(unidirectional);
                                AddProps(groupPanel, arProsp);
                            } catch(Exception ex) {
                                Log.Exception(ex);
                            }
                        });
                }
                panel.AddButton("Copy" + strAll, null, delegate () {
                    ClipBoard.SetData(cloned_props);
                });
                panel.AddButton("Copy" + strAll + " to other elevations", null, delegate () {
                    foreach(var item in cloned_props)
                        PropHelpers.CopyPropsToOtherElevations(item);
                });
                panel.AddButton("Add" + strAll + " to Template", null, delegate () {
                    SaveTemplatePanel.Display(cloned_props);
                });
                if(cloned_props.Count() >= 2) {
                    panel.AddButton("Displace all", null, delegate () {
                        DisplaceAll(original_props);
                    });
                }
            } else if(element is NetInfo.Lane lane && lane.HasProps()
                && target == NetInfoExtionsion.EditedNetInfo) {
                var panel = MiniPanel.Display();
                var m_lanes = NetInfoExtionsion.EditedNetInfo.m_lanes;
                var laneIndeces = elements.Select(_lane => Array.IndexOf(m_lanes, _lane));
                panel.AddButton(
                    "Copy props to other elevation",
                    "appends props to equivalent lane on other elevations",
                    delegate () {
                        foreach(var laneIndex in laneIndeces) {
                            PropHelpers.CopyPropsToOtherElevations(
                                laneIndex: laneIndex, clear: false);
                        }
                    });
                panel.AddButton(
                    "replace props to other elevation",
                    "clears props from other elevations before\n" +
                    "copying props to equivalent lane on other elevations",
                    delegate () {
                        foreach(var laneIndex in laneIndeces) {
                            PropHelpers.CopyPropsToOtherElevations(
                                laneIndex: laneIndex, clear: true);
                        }
                    });
            }
        }

        public static object AddArrayElement(
            RoadEditorPanel roadEditorPanel, RoadEditorCollapsiblePanel groupPanel,
            object target, FieldInfo arrayField, object newElement) {
            Log.Debug("RoadEditorDynamicPropertyToggle_OnEnable.AddArrayElement() called");
            newElement = AssetEditorRoadUtils.AddArrayElement(
                target, arrayField, newElement);
            roadEditorPanel.AddToArrayField(groupPanel, newElement, arrayField, target);
            return newElement;
        }

        public static void AddProps(
            RoadEditorCollapsiblePanel groupPanel,
            NetLaneProps.Prop[] props) {
            try {
                Log.Debug("AddProps called, props.count=" + props.Length);
                if(props == null || props.Length == 0) return;
                NetLaneProps.Prop[] m_props = groupPanel.GetArray() as NetLaneProps.Prop[];
                if(ModSettings.ARMode) {
                    // extend in AR mode
                    props = props.Select(_p => _p.Extend().Base).ToArray();
                } else {
                    // undo extend in Vanilla mode.
                    props = props.Select(_p => {
                        if(_p is IInfoExtended<NetLaneProps.Prop> propExt) {
                            return propExt.UndoExtend();
                        } else {
                            return _p;
                        }
                    }).ToArray();
                }
                var m_props2 = m_props.AddRangeToArray(props);

                var sidePanel = groupPanel.component.GetComponentInParent<RoadEditorPanel>();
                var arrayField = groupPanel.GetField();
                var target = groupPanel.GetTarget();

                Log.Debug($"Adding props {props.Length}+{m_props.Length}={m_props2.Length}");
                groupPanel.SetArray(m_props2);
                foreach(var prop in props) {
                    sidePanel.AddToArrayField(groupPanel, prop, arrayField, target);
                }
                sidePanel.OnObjectModified();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void DisplaceAllProps(NetInfo.Lane[] lanes) {
            var props = (from lane in lanes
                         from prop in lane.m_laneProps.m_props
                         select prop);
            DisplaceAll(props);
        }

        public static void DisplaceAll(IEnumerable<NetLaneProps.Prop> props) {
            var panel = MiniPanel.Display();
            var floatField = panel.AddFloatField();
            panel.AddButton("Displace", null, () =>
                DisplaceAll(props, floatField.Number));
        }

        public static void DisplaceAll(IEnumerable<NetLaneProps.Prop> props, float z) {
            Log.Debug(ThisMethod + $" props={props.ToSTR()} z={z}");
            foreach(var prop in props)
                prop.Displace(z);
            LogSucceeded();
        }

        public static void RefreshRoadEditor() {
            try {
                var mainPanel = Object.FindObjectOfType<RoadEditorMainPanel>();
                if(mainPanel) {
                    InvokeMethod(mainPanel, "OnObjectModified");
                    InvokeMethod(mainPanel, "Clear");
                    InvokeMethod(mainPanel, "Initialize");
                    InvokeMethod(mainPanel, "OnObjectModified");
                }
                MenuPanelBase.CloseAll();
                MiniPanel.CloseAll();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void RefreshAllNetworks() {
            if (!Helpers.InSimulationThread())
                SimulationManager.instance.m_ThreadingWrapper.QueueSimulationThread(RefreshAllNetworks);
            for(ushort nodeId =1; nodeId < NetManager.MAX_NODE_COUNT; ++nodeId) {
                if (NetUtil.IsNodeValid(nodeId)) {
                    NetManager.instance.UpdateNode(nodeId);
                }
            }
        }

        public static void SetTiling(this Material material, float tiling) {
            if(material) {
                material.mainTextureScale = new Vector2(1, tiling);
                // not sure if checksum changes if I change texture scale.to make sure checksum changes I also change the name.
                material.name = "NetworkTiling " + tiling.ToString("R"); 
            }
        }
    }
}


