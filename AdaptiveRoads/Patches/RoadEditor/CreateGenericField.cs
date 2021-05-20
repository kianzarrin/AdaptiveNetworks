namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Assertion;
    using static KianCommons.ReflectionHelpers;

    /// <summary>
    /// most of the fields are managed here:
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), "CreateGenericField")]
    public static class CreateGenericField {
        /// <summary>
        /// replace built-in fields
        /// </summary>
        public static bool Prefix(string groupName, FieldInfo field, object target,
            RoadEditorPanel __instance) {
            try {
                if (NetInfoExtionsion.EditedNetInfo == null)
                    return true; // ignore this outside of asset editor.
                if (RoadEditorPanelExtensions.RequiresUserFlag(field.FieldType))
                    return true;
                if (field.HasAttribute<BitMaskAttribute>() && field.HasAttribute<CustomizablePropertyAttribute>()) {
                    UIComponent container = __instance.m_Container;
                    if (!string.IsNullOrEmpty(groupName)) {
                        container = __instance.GetGroupPanel(groupName).Container;
                    }
                    var att = field.GetAttribute<CustomizablePropertyAttribute>();

                    var enumType = field.FieldType;
                    enumType = HintExtension.GetMappedEnumWithHints(enumType);

                    var hints = field.GetHints();
                    if (field.Name == "m_stopType")
                        hints.Add("set this for the pedestrian lane that contains the bus/tram stop.");
                    hints.AddRange(enumType.GetHints());
                    string hint = hints.JoinLines();
                    Log.Debug($"{field} -> hint is: " + hint);

                    var bitMaskPanel = BitMaskPanel.Add(
                        roadEditorPanel: __instance,
                        container: container,
                        label: att.name,
                        hint: hint,
                        flagData: new FlagDataT(
                            setValue: val => field.SetValue(target, val),
                            getValue: () => (int)field.GetValue(target),
                            enumType: enumType));
                    return false;
                }
                return true;
            } catch (Exception ex) {
                ex.Log();
                return false;
            }
        }

        /// <summary>
        /// Adds new custom fields after a built-in field.
        /// or modify the name of the built-in fields
        /// </summary>
        public static void Postfix(string groupName, FieldInfo field, object target, RoadEditorPanel __instance) {
            try {
                if (target is NetLaneProps.Prop prop) {
                    Log.Debug($"{__instance.name}.CreateGenericField.Postfix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);

                    ReplaceLabel(__instance, "Start Flags Required:", "Tail Flags Required:");
                    ReplaceLabel(__instance, "Start Flags Forbidden:", "Tail Flags Forbidden:");
                    ReplaceLabel(__instance, "End Flags Required:", "Head Flags Required:");
                    ReplaceLabel(__instance, "End Flags Forbidden:", "Head Flags Forbidden:");


                    if (field.Name == nameof(NetLaneProps.Prop.m_endFlagsForbidden)) {
                        Assert(prop.LocateEditProp(out _, out var lane), "could not locate prop");
                        bool forward = lane.IsGoingForward();
                        bool backward = lane.IsGoingBackward();
                        bool unidirectional = forward || backward;
                        if(ModSettings.ARMode) {
                            var metadata = prop.GetOrCreateMetaData();
                            foreach (var field2 in metadata.GetFieldsWithAttribute<CustomizablePropertyAttribute>()) {
                                CreateExtendedComponent(groupName, field2, metadata, __instance);
                            }
                        }
                        if(!unidirectional) {
                            ButtonPanel.Add(
                                roadEditorPanel: __instance,
                                container: __instance.m_Container,
                                label: "Switch Forward/Backward",
                                null,
                                action: () => {
                                    prop.ToggleForwardBackward();
                                    __instance.OnObjectModified();
                                    __instance.Reset();

                                });
                        }

                        ButtonPanel.Add(
                            roadEditorPanel: __instance,
                            container: __instance.m_Container,
                            label: "Switch RHT/LHT",
                            HintExtension.GetHintSwichLHT_RHT(unidirectional),
                            action: () => {
                                prop.ToggleRHT_LHT(unidirectional);
                                __instance.OnObjectModified();
                                __instance.Reset();
                            });
                    }
                } else if (target is NetInfo.Node node) {
                    Log.Debug($"{__instance.name}.CreateGenericField.Postfix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
                    if (ModSettings.ARMode) {
                        if (field.Name == nameof(NetInfo.Node.m_flagsForbidden)) {
                            var fields = typeof(NetInfoExtionsion.Node).GetFields()
                                .Where(_field => _field.HasAttribute<CustomizablePropertyAttribute>());
                            var node2 = node.GetOrCreateMetaData();
                            foreach (var field2 in fields) {
                                CreateExtendedComponent(groupName, field2, node2, __instance);
                            }
                        }
                    }
                } else if (target is NetInfo.Segment segment) {
                    Log.Debug($"{__instance.name}.CreateGenericField.Postfix({groupName}, {field}, {target})\n"/* + Environment.StackTrace*/);
                    if (ModSettings.ARMode) {
                        var segment2 = segment.GetOrCreateMetaData();
                        AssertNotNull(segment2, $"{segment}");
                        var fieldForward = typeof(NetInfoExtionsion.Segment).GetField(
                            nameof(NetInfoExtionsion.Segment.Forward));
                        if (field.Name == nameof(NetInfo.Segment.m_forwardForbidden)) {
                            CreateExtendedComponent(groupName, fieldForward, segment2, __instance);
                        } else if (field.Name == nameof(NetInfo.Segment.m_backwardForbidden)) {
                            var fields = segment2
                                .GetFieldsWithAttribute<CustomizablePropertyAttribute>()
                                .Where(_f => _f != fieldForward);
                            int totalCount = typeof(NetInfoExtionsion.Segment)
                                .GetFieldsWithAttribute<CustomizablePropertyAttribute>()
                                .Count();
                            foreach (var field2 in fields)
                                CreateExtendedComponent(groupName, field2, segment2, __instance);
                        }
                   }
                } else if (target is NetInfo netInfo) {
                    if (ModSettings.ARMode) {
                        // replace "Pavement Width" with Pavement Width Left
                        ReplaceLabel(__instance, "Pavement Width", "Pavement Width Left");
                        // inject our own field
                        if (field.Name == nameof(NetInfo.m_pavementWidth)) {
                            Log.Debug($"{__instance.name}.CreateGenericField.Postfix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
                            var net = netInfo.GetOrCreateMetaData();
                            AssertNotNull(net, $"{netInfo}");
                            var f = net.GetType().GetField(nameof(net.PavementWidthRight));
                            __instance.CreateGenericField(groupName, f, net);
                        }
                    }
                }
            } catch (Exception e) {
                Log.Exception(e);
            }
        }

        /// <summary>
        /// replaces a label with text oldLabel with new Label
        /// </summary>
        /// <param name="component">this component and all its children are searched</param>
        public static void ReplaceLabel(Component component, string oldLabel, string newLabel) {
            try {
                var labels = component.GetComponentsInChildren<UILabel>()
                    .Where(_lbl => _lbl.text == oldLabel);
                if (labels == null) return;
                foreach (var label in labels)
                    label.text = newLabel;
            } catch (Exception ex) {
                ex.Log();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="target"></param>
        /// <param name="instance"></param>
        /// <param name="prefix"></param>
        public static void CreateExtendedComponent(
            string groupName, FieldInfo fieldInfo, object target, RoadEditorPanel instance, string prefix = "") {
            try {
                //Assert(string.IsNullOrEmpty(groupName), "groupName is empty");
                UIComponent container = instance.m_Container;  //instance.component.GetComponentInChildren<UIScrollablePanel>();
                if (!string.IsNullOrEmpty(groupName)) {
                    container = instance.GetGroupPanel(groupName).Container;
                }

                AssertNotNull(container, "container");
                Log.Debug("CreateExtendedComponent():container=" + container);

                Assert(fieldInfo.HasAttribute<CustomizablePropertyAttribute>(), "HasAttribute:CustomizablePropertyAttribute");
                AssertNotNull(target, "target");
                AssertNotNull(target, "fieldInfo");
                AssertNotNull(target, "RoadEditorPanel instance");
                Log.Debug(
                    $"CreateExtendedComponent(groupName={groupName}, fieldInfo={fieldInfo}, target={target}, instance={instance.name}) called",
                    false);

                var att = fieldInfo.GetAttribute<CustomizablePropertyAttribute>();
                var optionals = fieldInfo.GetAttributes<OptionalAttribute>();
                var optionals2 = target.GetType().GetAttributes<OptionalAttribute>();
                foreach (var optional in optionals.Concat(optionals2)) {
                    if (optional != null && !ModSettings.GetOption(optional.Option)) {
                        Log.Debug($"Hiding {target.GetType().Name}::`{att.name}` because {optional.Option} is disabled");
                        return;
                    }
                }

                var hints = fieldInfo.GetHints();
                hints.AddRange(fieldInfo.FieldType.GetHints());
                string hint = hints.JoinLines();
                Log.Debug("hint is " + hint);

                if (fieldInfo.FieldType.HasAttribute<FlagPairAttribute>()) {
                    IConvertible GetRequired() {
                        object subTarget = fieldInfo.GetValue(target);
                        return (int)GetFieldValue(subTarget, "Required");
                    }
                    void SetRequired(IConvertible flags) {
                        var subTarget = fieldInfo.GetValue(target);
                        SetFieldValue(target: subTarget, fieldName: "Required", value: flags);
                        fieldInfo.SetValue(target, subTarget);
                    }
                    IConvertible GetForbidden() {
                        object subTarget = fieldInfo.GetValue(target);
                        return (int)GetFieldValue(subTarget, "Forbidden");
                    }
                    void SetForbidden(IConvertible flags) {
                        var subTarget = fieldInfo.GetValue(target);
                        SetFieldValue(target: subTarget, fieldName: "Forbidden", value: flags);
                        fieldInfo.SetValue(target, subTarget);
                    }

                    Type enumType = fieldInfo.FieldType.GetField("Required").FieldType;
                    enumType = HintExtension.GetMappedEnumWithHints(enumType);

                    var panel0 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Required",
                        hint: hint,
                        flagData: new FlagDataT(
                            setValue: SetRequired,
                            getValue: GetRequired,
                            enumType: enumType));
                    var panel1 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Forbidden",
                        hint: hint,
                        flagData: new FlagDataT(
                            setValue: SetForbidden,
                            getValue: GetRequired,
                            enumType: enumType));
                } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.Range) &&
                           fieldInfo.Name.ToLower().Contains("speed")) {
                    var panel = SpeedRangePanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name,
                        target: target,
                        fieldInfo: fieldInfo);
                } else {
                    Log.Error($"CreateExtendedComponent: Unhandled field: {fieldInfo} att:{att.name} ");
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }
    }
}

