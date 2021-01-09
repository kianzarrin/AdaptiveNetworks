using AdaptiveRoads.Manager;
using AdaptiveRoads.UI.RoadEditor;
using AdaptiveRoads.UI.RoadEditor.Templates;
using ColossalFramework.UI;
using HarmonyLib;
using KianCommons;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AdaptiveRoads.Util.DPTHelpers;

namespace AdaptiveRoads.Util {
    internal static class RoadEditorUtils {
        public static List<UICustomControl> SelectedDPTs = new List<UICustomControl>();

        public static void OnToggleDPT(UICustomControl dpt) {
            VerifySelectedDPTs(dpt);
            if (SelectedDPTs.Contains(dpt))
                DeselectDPT(dpt);
            else
                SelectDPT(dpt);
        }

        /// <summary>
        /// verifies selected DPTs are active and match the input DPT.
        /// if not a match, it is removed from selection.
        /// </summary>
        public static void VerifySelectedDPTs(UICustomControl dpt) {
            bool predicateRemove(UICustomControl dpt2) {
                if (!dpt2 || !dpt2.isActiveAndEnabled) return true;
                var target1 = GetDPTTargetObject(dpt);
                var target2 = GetDPTTargetObject(dpt2);
                Log.Debug($"target1={target1} and target2={target2}");
                return target1 != target2;
            }
            var removeDPTs = SelectedDPTs.Where(predicateRemove).ToList();
            foreach (var dpt3 in removeDPTs)
                DeselectDPT(dpt3);
        }

        static void SelectDPT(UICustomControl dpt) {
            try {
                ToggleDPTColor(dpt, true);
                SelectedDPTs.Add(dpt);
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static void DeselectDPT(UICustomControl dpt) {
            try {
                ToggleDPTColor(dpt, false);
                SelectedDPTs.Remove(dpt);
            } catch { }
        }

        public static void DeselectAllDPTs() {
            foreach (var dpt in SelectedDPTs) {
                try {
                    ToggleDPTColor(dpt, false);
                } catch { }
            }
            SelectedDPTs.Clear();
        }

        public static void OnDPTMoreOptions(UICustomControl dpt) {
            Log.Debug("OnDPTMoreOptions() called");
            VerifySelectedDPTs(dpt);
            if (!SelectedDPTs.Contains(dpt)) {
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
            if (SelectedDPTs.Any())
                elements = SelectedDPTs.Select(_dpt => GetDPTTargetElement(_dpt));
            else
                elements = new object[] { element };

            if (target is NetLaneProps netLaneProps
                && element is NetLaneProps.Prop) {
                //int propIndex = netLaneProps.m_props.IndexOf(prop);
                //NetInfo netInfo = null; ;
                //int laneIndex = 0; ;
                //foreach (var netInfo2 in NetInfoExtionsion.EditedNetInfos) {
                //    for (int laneIndex2 = 0; laneIndex2 < netInfo2.m_lanes.Length; ++laneIndex2) {
                //        if(netInfo2.m_lanes[laneIndex2].m_laneProps == netLaneProps) {
                //            netInfo = netInfo2;
                //            laneIndex = laneIndex2;
                //        }
                //    }
                //}
                var panel = MiniPanel.Display();
                var f_props = typeof(NetLaneProps).GetField(nameof(NetLaneProps.m_props));
                var props = elements.Select(_p => (_p as NetLaneProps.Prop).Clone());
                string strAll = props.Count() > 1 ? " all" : "";

                panel.AddButton("Duplicate" + strAll, null, delegate () {
                    AddProps(groupPanel, props.ToArray());
                });

                if (props.Any(_p => _p.CanInvert())) {
                    panel.AddButton(
                        "Inverted duplicate" + strAll,
                        "swaps: required.Inverted<->foribdden.inverted start<->end left<->right\n" +
                        "negates: position.z offset angle",
                        delegate () {
                            foreach (var item in props)
                                item.ToggleRHT_LHT();
                            AddProps(groupPanel, props.ToArray());
                        });
                }
                panel.AddButton("Copy" + strAll, null, delegate () {
                    ClipBoard.SetData(props);
                });
                panel.AddButton("Copy" + strAll + " to other elevations", null, delegate () {
                    foreach (var item in props)
                        PropHelpers.CopyPropsToOtherElevations(item);
                });
                panel.AddButton("Add" + strAll + " to Template", null, delegate () {
                    SaveTemplatePanel.Display(props);
                });

            } else if (element is NetInfo.Lane lane && lane.HasProps()
                && target == NetInfoExtionsion.EditedNetInfo) {
                var panel = MiniPanel.Display();
                var m_lanes = NetInfoExtionsion.EditedNetInfo.m_lanes;
                var laneIndeces = elements.Select(_lane => m_lanes.IndexOf(_lane));
                panel.AddButton(
                    "Copy props to other elevation",
                    "appends props to equivalent lane on other elevations",
                    delegate () {
                        foreach (var laneIndex in laneIndeces) {
                            PropHelpers.CopyPropsToOtherElevations(
                                laneIndex: laneIndex, clear: false);
                        }
                    });
                panel.AddButton(
                    "replace props to other elevation",
                    "clears props from other elevations before\n" +
                    "copying props to equivalent lane on other elevations",
                    delegate () {
                        foreach (var laneIndex in laneIndeces) {
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
                Log.Debug("AddProps called");
                if (props == null || props.Length == 0) return;
                NetLaneProps.Prop[] m_props = groupPanel.GetArray() as NetLaneProps.Prop[];
                props = props.Select(_p => _p.Extend().Base).ToArray();
                var m_props2 = m_props.AddRangeToArray(props);

                var sidePanel = groupPanel.component.GetComponentInParent<RoadEditorPanel>();
                var arrayField = groupPanel.GetField();
                var target = groupPanel.GetTarget();

                Log.Debug($"Adding props {props.Length}+{m_props.Length}={m_props2.Length}");
                groupPanel.SetArray(m_props2);
                foreach (var prop in props) {
                    sidePanel.AddToArrayField(groupPanel, prop, arrayField, target);
                }
                sidePanel.OnObjectModified();
            } catch (Exception ex) {
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
            var numberField = panel.AddNumberField();
            panel.AddButton("Displace", null, () =>
                DisplaceAll(props, numberField.Number));
        }

        public static void DisplaceAll(IEnumerable<NetLaneProps.Prop> props, int z) {
            foreach (var prop in props)
                prop.Displace(z);
        }


    }




}


