namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.UI.RoadEditor.Bitmask;
    using AdaptiveRoads.Util;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Assertion;
    using static KianCommons.ReflectionHelpers;
    using PrefabMetadata.API;
    using AdaptiveRoads.UI.QuayRoads;
    using ColossalFramework;

    /// <summary>
    /// most of UI in road editor panels are managed here:
    /// </summary>
    [HarmonyPatch]
    public static class CreateField {
        static bool Merge => true;
        static bool InRoadEditor => NetInfoExtionsion.EditedNetInfo != null;

        static bool Is(this FieldInfo field, Type baseType, string name) {
            return field.DeclaringType.IsAssignableFrom(baseType) && field.Name == name;
        }

        /// <summary>
        /// replace built-in fields
        /// </summary>
        [HarmonyPatch(typeof(RoadEditorPanel), "CreateGenericField")]
        public static bool Prefix(RoadEditorPanel __instance,
            ref string groupName, FieldInfo field, object target) {
            //Log.Called(__instance, groupName, field, target);
            try {
                if (field.Is(typeof(NetInfo.Node), nameof(NetInfo.Node.m_directConnect)))
                    groupName = NetInfoExtionsion.Node.DC_GROUP_NAME;
                if (field.Is(typeof(NetInfo.Node), nameof(NetInfo.Node.m_connectGroup)))
                    groupName = NetInfoExtionsion.Node.DC_GROUP_NAME;

                // handle special track case.
                if (target is not IInfoExtended &&
                    CreateGenericComponentExt0(
                        roadEditorPanel: __instance,
                        groupName: groupName,
                        target: target,
                        metadata: target,
                        field))
                {
                    return false;
                }

                if(ModSettings.ARMode &&
                    field.FieldType == typeof(NetInfo.ConnectGroup)) {
                    CreateConnectGroupComponent(__instance, groupName, target, field);
                    return false;
                }

                if (IsUIReplaced(field)) {
                    if (VanillaCanMerge(field))
                        return false; // will be merged with AN drop-down later
                    var container = GetContainer(__instance, groupName);
                    var uidata = GetVanillaFlagUIData(field, target);

                    //Log.Info($"[P1] CreateGenericField.Prefix() : field:{field}, target:{target}, group:{groupName}");
                    var bitMaskPanel = BitMaskPanel.Add(
                        roadEditorPanel: __instance,
                        container: container,
                        label: uidata.Label,
                        hint: uidata.Hint,
                        flagData: uidata.FlagData);
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
        [HarmonyPatch(typeof(RoadEditorPanel), "CreateField")]
        public static void Postfix(FieldInfo field, object target, RoadEditorPanel __instance) {
            try {
                var cpt = field.GetAttribute<CustomizablePropertyAttribute>();
                string groupName = cpt.group;

                if (target is NetLaneProps.Prop prop) {
                    Log.Debug($"{__instance.name}.CreateField.Postfix({groupName},{field},{target})"/* + Environment.StackTrace*/);
                    if (ModSettings.ARMode) {
                        var metadata = prop.GetOrCreateMetaData();
                        foreach (var field2 in field.GetAfterFields(metadata)) {
                            CreateGenericComponentExt(
                                roadEditorPanel: __instance, groupName: groupName,
                                target: target, metadata: metadata, extensionField: field2);
                        }
                    }

                    if (typeof(NetInfoExtionsion.LaneProp).ComesAfter(field)) {
                        Assert(prop.LocateEditProp(out _, out var lane), "could not locate prop");
                        bool forward = lane.IsGoingForward();
                        bool backward = lane.IsGoingBackward();
                        bool unidirectional = forward || backward;
                        if (!unidirectional) {
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

                    ReplaceLabel(__instance, "Start Flags Required:", "Tail Node Flags Required:");
                    ReplaceLabel(__instance, "Start Flags Forbidden:", "Tail  Node Flags Forbidden:");
                    ReplaceLabel(__instance, "End Flags Required:", "Head  Node Flags Required:");
                    ReplaceLabel(__instance, "End Flags Forbidden:", "Head  Node Flags Forbidden:");
                } else if (target is NetInfo.Node node) {
                    Log.Debug($"{__instance.name}.CreateField.Postfix({groupName},{field},{target})"/* + Environment.StackTrace*/);
                    if (ModSettings.ARMode) {
                        var metadata = node.GetOrCreateMetaData();
                        foreach (var field2 in field.GetAfterFields(metadata)) {
                            CreateGenericComponentExt(
                                roadEditorPanel: __instance, groupName: groupName,
                                target: target, metadata: metadata, extensionField: field2);
                        }
                    }
                } else if (target is NetInfo.Segment segment) {
                    Log.Debug($"{__instance.name}.CreateField.Postfix({groupName}, {field}, {target})"/* + Environment.StackTrace*/);
                    if (ModSettings.ARMode) {
                        var metadata = segment.GetOrCreateMetaData();
                        AssertNotNull(metadata, $"{segment}");
                        foreach (var field2 in field.GetAfterFields(metadata)) {
                            CreateGenericComponentExt(
                                roadEditorPanel: __instance, groupName: groupName,
                                target: target, metadata: metadata, extensionField: field2);
                        }
                    }
                } else if (target is NetInfo netInfo) {
                    Log.Debug($"{__instance.name}.CreateField.Postfix({groupName}, {field}, {target})"/* + Environment.StackTrace*/);
                    if (field.Name == nameof(NetInfo.m_surfaceLevel)) {
                        ExposeField(
                            roadEditorPanel: __instance,
                            groupName: groupName,
                            target: target,
                            fieldName: nameof(NetInfo.m_terrainStartOffset),
                            label: "Terrain Start Offset");
                        ExposeField(
                            roadEditorPanel: __instance,
                            groupName: groupName,
                            target: target,
                            fieldName: nameof(NetInfo.m_terrainEndOffset),
                            label: "Terrain End Offset");
                    }
                    if (ModSettings.ARMode) {
                        ReplaceLabel(__instance, "Pavement Width", "Pavement Width Left");
                        var net = netInfo.GetOrCreateMetaData();
                        AssertNotNull(net, $"{netInfo}");
                        foreach(var field2 in net.GetFieldsWithAttribute<CustomizablePropertyAttribute>()) {
                            if(field2.ComesAfter(field)) {
                                Log.Debug($"calling {__instance.name}.CreateField({field2},{net}) ...");
                                __instance.CreateField(field2, net);
                            }
                        }
                        if (field.Name == nameof(NetInfo.m_surfaceLevel)) {
                            Log.Debug("adding QuayRoads button");
                            var qrButtonPanel = ButtonPanel.Add(
                            roadEditorPanel: __instance,
                            container: __instance.GetGroupPanel("Properties").m_Panel,
                            label: "Edit QuayRoads profile",
                            hint: "",
                            action: () => QuayRoadsPanel.GetOrOpen(netInfo, __instance));
                            qrButtonPanel.EventDestroy += (_, _) => { QuayRoadsPanel.CloseIfOpen(netInfo); };
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
        /// replaces the label of the UI component for the given field with new label.
        /// </summary>
        /// <param name="component">this component and all its children are searched</param>
        public static void SetLabel(Component component, FieldInfo fieldInfo, string newLabel) {
            try {
                var c = component.GetComponentsInChildren<REPropertySet>()
                    .FirstOrDefault(item => item.GetTargetField() == fieldInfo);
                var label = c?.GetComponentInChildren<UILabel>();
                if(label != null)
                    label.text = newLabel;
            } catch (Exception ex) {
                ex.Log();
            }
        }

        /// <summary>
        /// exposes a Vanilla field that does not have the CustomizablePropertyAttribute.
        /// </summary>
        public static void ExposeField(RoadEditorPanel roadEditorPanel, string groupName, object target, string fieldName,  string label) {
            var fieldInfo = GetField(target.GetType(), fieldName, throwOnError: true);
            roadEditorPanel.CreateGenericField(groupName, fieldInfo, target);
            SetLabel(roadEditorPanel, fieldInfo, label);
        }

        static IEnumerable<FieldInfo> GetAfterFields(this FieldInfo before, object target) {
            return target.GetFieldsWithAttribute<CustomizablePropertyAttribute>()
                .Where(field2 => field2.ComesAfter(before));
        }

        static bool ComesAfter(this MemberInfo after, FieldInfo before) {
            Assert(after != before, $"{after} != {before}");
            return after.AfterField() == before.Name;
        }

        static string AfterField(this MemberInfo field) {
            return
                field.GetAttribute<AfterFieldAttribute>()?.FieldName ??
                field.DeclaringType.GetAttribute<AfterFieldAttribute>()?.FieldName ??
                throw new Exception("could not find after field for " + field);
        }

        /// <returns>false if this field is not special case and needs to be handled normally.</returns>
        public static bool CreateGenericComponentExt0(
            RoadEditorPanel roadEditorPanel, string groupName,
            object target, object metadata, FieldInfo extensionField) {
            if(target != metadata && TryGetMerge(extensionField, target, out var vanillaRequired, out var vanillaForbidden)) {
                CreateMergedComponent(
                    roadEditorPanel: roadEditorPanel, groupName: groupName,
                    fieldInfo: extensionField, metadata: metadata,
                    mergedFieldRequired: vanillaRequired,
                    mergedFieldForbidden: vanillaForbidden,
                    mergedTarget: target);
            } else if(TryGetMerge(extensionField, metadata, out var vanilla)) {
                CreateMergedComponent(
                    roadEditorPanel: roadEditorPanel, groupName: groupName,
                    fieldInfo: extensionField, mergedField: vanilla, metadata: metadata);
            } else if(ARCanMerge(extensionField)) {
                // do not create because it will merge with something in future.
            } else {
                return CreateExtendedComponent(roadEditorPanel, groupName, extensionField, metadata);
            }
            return true; // handled
        }

        public static void CreateGenericComponentExt(
            RoadEditorPanel roadEditorPanel, string groupName,
            object target, object metadata, FieldInfo extensionField) {
            if(!CreateGenericComponentExt0(roadEditorPanel, groupName, target, metadata, extensionField)) {
                roadEditorPanel.CreateField(extensionField, metadata);
            }
        }

        public static bool CreateExtendedComponent(
            RoadEditorPanel roadEditorPanel, string groupName,
            FieldInfo fieldInfo, object metadata) {
            try {
                LogCalled(
                    $"instance={ roadEditorPanel?.name}",
                    $"groupName={ groupName}",
                    $"fieldInfo={fieldInfo}",
                    $"metadata={metadata}");
                AssertNotNull(roadEditorPanel, "RoadEditorPanel instance");

                var container = GetContainer(roadEditorPanel, groupName);
                var att = fieldInfo.GetAttribute<CustomizablePropertyAttribute>();

                if(fieldInfo.HasAttribute<BitMaskLanesAttribute>()) {
                    NetInfo netInfo =
                        (metadata as NetInfoExtionsion.Track)?.ParentInfo ??
                        RoadEditorUtils.GetSelectedNetInfo(out _);
                    BitMaskLanesPanel.Add(
                        roadEditorPanel: roadEditorPanel,
                        field: fieldInfo,
                        netInfo: netInfo,
                        container: container,
                        label: att.name,
                        hint: fieldInfo.GetHints().JoinLines());
                } else if(fieldInfo.FieldType.HasAttribute<FlagPairAttribute>()) {
                    var uidatas = GetARFlagUIData(fieldInfo, metadata);
                    if(uidatas != null) {
                        BitMaskPanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: uidatas[0].Label,
                            hint: uidatas[0].Hint,
                            flagData: uidatas[0].FlagData);
                        BitMaskPanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: uidatas[1].Label,
                            hint: uidatas[1].Hint,
                            flagData: uidatas[1].FlagData);
                    }
                } else if(fieldInfo.FieldType == typeof(NetInfoExtionsion.Range)) {
                    if(fieldInfo.Name.ToLower().Contains("speed")) {
                        var panel = SpeedRangePanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: att.name,
                            target: metadata,
                            fieldInfo: fieldInfo);
                    } else {
                        var panel = RangePanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: att.name,
                            target: metadata,
                            fieldInfo: fieldInfo);
                    }
                } else {
                    return false;
                }
                return true;
            } catch(Exception ex) {
                ex.Log();
                return false;
            }
        }

        public static void CreateConnectGroupComponent(
            RoadEditorPanel roadEditorPanel, string groupName, object target, FieldInfo fieldInfo) {
            Assert(fieldInfo.FieldType == typeof(NetInfo.ConnectGroup), "field type is connect group");
            Assert(fieldInfo.Name == nameof(NetInfo.m_connectGroup));
            Log.Called(roadEditorPanel, groupName, target, fieldInfo);
            object metadata = null;
            if (target is NetInfo.Node nodeInfo)
                metadata = nodeInfo.GetOrCreateMetaData();
            else if (target is NetInfo netInfo)
                metadata = netInfo.GetOrCreateMetaData();
            Assertion.NotNull(metadata,"metadata");
            var container = GetContainer(roadEditorPanel, groupName);
            var uidata = GetVanillaFlagUIData(fieldInfo, target);
            var customdata = new CustomFlagDataT(
                itemSource: ItemSource.GetOrCreate(fieldInfo.FieldType),
                selected: Traverse.Create(metadata).Field("ConnectGroups"));

            var bitMaskPanel = BitMaskPanelCustomisable.Add(
                roadEditorPanel: roadEditorPanel,
                container: container,
                label: uidata.Label,
                hint: uidata.Hint,
                flagData: uidata.FlagData,
                customFlagData: customdata);
        }

        public static void CreateMergedComponent(
             RoadEditorPanel roadEditorPanel, string groupName,
             FieldInfo fieldInfo, object metadata,
             FieldInfo mergedFieldRequired, FieldInfo mergedFieldForbidden, object mergedTarget) {
            try {
                LogCalled(
                    $"instance={ roadEditorPanel?.name}",
                    $"groupName={ groupName}",
                    $"fieldInfo={fieldInfo}",
                    $"metadata={metadata}",
                    $"mergedFieldRequired={mergedFieldRequired}",
                    $"mergedFieldForbidden={mergedFieldForbidden}",
                    $"mergedTarget={mergedTarget}");
                AssertNotNull(roadEditorPanel, "RoadEditorPanel instance");
                var container = GetContainer(roadEditorPanel, groupName);

                var vanillas = new[]{
                    GetVanillaFlagUIData(mergedFieldRequired, mergedTarget),
                    GetVanillaFlagUIData(mergedFieldForbidden, mergedTarget),
                };
                var arDatas = GetARFlagUIData(fieldInfo, metadata);
                if (arDatas.IsNullorEmpty()) {
                    for (int i = 0; i < 2; ++i) {
                        // hide optional flags.
                        BitMaskPanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: vanillas[i].Label,
                            hint: vanillas[i].Hint,
                            flagData: vanillas[i].FlagData);
                    }
                } else {
                    for (int i = 0; i < 2; ++i) {
                        Assertion.Equal(arDatas.Length, 2, "arDatas.Length");
                        MultiBitMaskPanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: vanillas[i].Label,
                            hint: arDatas[i].Hint,
                            flagDatas: new[] { vanillas[i].FlagData, arDatas[i].FlagData });
                    }
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }
        public static void CreateMergedComponent(
             RoadEditorPanel roadEditorPanel, string groupName,
             FieldInfo fieldInfo, FieldInfo mergedField, object metadata) {
            try {
                LogCalled(
                    $"instance={ roadEditorPanel?.name}",
                    $"groupName={ groupName}",
                    $"fieldInfo={fieldInfo}",
                    $"mergedField={mergedField}",
                    $"metadata={metadata}");
                AssertNotNull(roadEditorPanel, "RoadEditorPanel instance");
                var container = GetContainer(roadEditorPanel, groupName);

                var vanillas = GetARFlagUIData(mergedField, metadata);
                var arDatas = GetARFlagUIData(fieldInfo, metadata);
                if (vanillas.IsNullorEmpty() && arDatas.IsNullorEmpty()) {
                    return; //both optional and hidden;
                } else if (vanillas.IsNullorEmpty()) {
                    // only vanilla is optional and hidden
                    Assertion.Equal(arDatas.Length, 2, "arDatas.Length");
                    for (int i = 0; i < 2; ++i) {
                        BitMaskPanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: arDatas[i].Label,
                            hint: arDatas[i].Hint,
                            flagData: arDatas[i].FlagData);
                    }
                } else if (arDatas.IsNullorEmpty()) {
                    // only AN data is optional and hidden
                    Assertion.Equal(vanillas.Length, 2, "vanillas.Length");
                    for (int i = 0; i < 2; ++i) {
                        BitMaskPanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: vanillas[i].Label,
                            hint: vanillas[i].Hint,
                            flagData: vanillas[i].FlagData);
                    }
                } else {
                    for (int i = 0; i < 2; ++i) {
                        Assertion.Equal(arDatas.Length, 2, "arDatas.Length");
                        MultiBitMaskPanel.Add(
                            roadEditorPanel: roadEditorPanel,
                            container: container,
                            label: vanillas[i].Label,
                            hint: arDatas[i].Hint,
                            flagDatas: new[] { vanillas[i].FlagData, arDatas[i].FlagData });
                    }
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }

        static bool IsUIReplaced(FieldInfo vanillaField) {
            return
                InRoadEditor &&
                !RoadEditorPanelExtensions.RequiresUserFlag(vanillaField.FieldType) &&
                vanillaField.HasAttribute<BitMaskAttribute>() &&
                vanillaField.HasAttribute<CustomizablePropertyAttribute>();
        }

        static bool TryGetMerge(FieldInfo extensionField, object vanillaTarget,
            out FieldInfo vanillaRequiredField, out FieldInfo vanillaForbiddenField) {
            Assert(vanillaTarget is IInfoExtended, "vanillaTarget is IInfoExtended");
            vanillaRequiredField = vanillaForbiddenField = null;
            if (!Merge) return false;

            foreach (var vanillaField in vanillaTarget.GetFieldsWithAttribute<CustomizablePropertyAttribute>()) {
                if (CanMergeWith(extensionField, vanillaField)) {
                    if (vanillaField.Name.EndsWith("Required"))
                        vanillaRequiredField = vanillaField;
                    else
                        vanillaForbiddenField = vanillaField;
                    if (vanillaRequiredField != null && vanillaForbiddenField != null)
                        return true;
                }
            }
            return false;

            static bool CanMergeWith(FieldInfo extensionField, FieldInfo vanillaField) {
                var atts = extensionField.FieldType.GetAttributes<FlagPairAttribute>().EmptyIfNull();
                bool forward1 = extensionField.Name.Contains("Forward");
                bool forward2 = vanillaField.Name.Contains("forward");
                bool start1 = extensionField.Name.Contains("Start") || extensionField.Name.Contains("Tail");
                bool start2 = vanillaField.Name.Contains("start");
                return
                    Merge &&
                    IsUIReplaced(vanillaField) &&
                    atts.Any(att => att?.MergeWithEnum == vanillaField.FieldType) &&
                    forward1 == forward2 && // forward/backward segment flags
                    start1 == start2; // start/end node flags
            }
        }

        static bool TryGetMerge(FieldInfo extensionField, object metadata, out FieldInfo vanillaFlags) {
            vanillaFlags = null;
            if (!Merge) return false;

            foreach (var vanillaField in metadata.GetFieldsWithAttribute<CustomizablePropertyAttribute>()) {
                if (CanMergeWith2(extensionField, vanillaField)) {
                    vanillaFlags = vanillaField;
                    return true;
                }
            }
            return false;

            static bool CanMergeWith2(FieldInfo extensionField, FieldInfo vanillaField) {
                try {
                    if (!extensionField.FieldType.HasAttribute<FlagPairAttribute>() ||
                        !vanillaField.FieldType.HasAttribute<FlagPairAttribute>()) {
                        return false;
                    }
                    var atts = extensionField.FieldType.GetAttributes<FlagPairAttribute>();
                    var vanillaEnumType = vanillaField.FieldType.GetField("Required").FieldType;
                    bool start1 = extensionField.Name.Contains("Start") || extensionField.Name.Contains("Tail");
                    bool start2 = vanillaField.Name.Contains("Start") || vanillaField.Name.Contains("Tail");
                    return
                        Merge && InRoadEditor &&
                        atts.Any(att => att.MergeWithEnum == vanillaEnumType) &&
                        start1 == start2; //match start/end Node flag;
                } catch (Exception ex) {
                    Log.Exception(ex, $"extensionField={extensionField}, vanillaField={vanillaField}");
                    return false;
                }
            }
        }

        static bool ARCanMerge(FieldInfo field) {
            if (!Merge || !field.FieldType.HasAttribute<FlagPairAttribute>())
                return false;
            var enumType = field.FieldType.GetField("Required").FieldType;
            return
                enumType == typeof(NetNode.Flags) ||
                enumType == typeof(NetSegment.Flags) ||
                enumType == typeof(NetLane.Flags);
        }

        static bool VanillaCanMerge(FieldInfo field) {
            if (!Merge || !ModSettings.ARMode || field.DeclaringType == typeof(NetInfo.Lane))
                return false;
            return
                field.FieldType == typeof(NetNode.Flags) ||
                field.FieldType == typeof(NetSegment.Flags) ||
                field.FieldType == typeof(NetLane.Flags);
        }

        public static UIComponent GetContainer(RoadEditorPanel roadEditorPanel, string groupName) {
            UIComponent container = roadEditorPanel.m_Container;
            if (!string.IsNullOrEmpty(groupName)) {
                container = roadEditorPanel.GetGroupPanel(groupName).Container;
            }
            AssertNotNull(container, "container");
            return container;
        }

        struct FlagUIData {
            internal string Label;
            internal string Hint;
            internal FlagDataT FlagData;
            internal bool RequiredFlag; //true if required, otherwise forbidden
        }

        static FlagUIData GetVanillaFlagUIData(FieldInfo field, object target) {
            AssertNotNull(target, "target");
            AssertNotNull(field, "fieldInfo");
            Assert(field.HasAttribute<CustomizablePropertyAttribute>(), "field has CustomizablePropertyAttribute");
            Assert(field.HasAttribute<BitMaskAttribute>(), "field has BitMaskAttribute");
            Assert(field.FieldType.HasAttribute<FlagsAttribute>(), "field has FlagsAttribute");

            var enumType = field.FieldType;
            enumType = HintExtension.GetMappedEnumWithHints(enumType);

            var hints = field.GetHints();
            if (field.Name == "m_stopType")
                hints.Add("set this for the pedestrian lane that contains the bus/tram stop.");
            hints.AddRange(enumType.GetHints());
            string hint = hints.JoinLines();
            Log.Debug($"{field} -> hint is: " + hint);

            var flagData = new FlagDataT(
                    setValue: val => field.SetValue(target, val),
                    getValue: () => field.GetValue(target) as IConvertible,
                    enumType: enumType);

            return new FlagUIData {
                Label = field.GetAttribute<CustomizablePropertyAttribute>().name,
                Hint = hint,
                FlagData = flagData,
                RequiredFlag = field.Name.EndsWith("Required"),
            };
        }

        static FlagUIData[] GetARFlagUIData(FieldInfo field, object target) {
            AssertNotNull(target, "target");
            AssertNotNull(field, "fieldInfo");
            Assert(field.HasAttribute<CustomizablePropertyAttribute>(), "field has CustomizablePropertyAttribute");
            Assert(field.FieldType.HasAttribute<FlagPairAttribute>(), "field has FlagPairAttribute");

            var optionals = field.GetAttributes<OptionalAttribute>();
            var optionals2 = target.GetType().GetAttributes<OptionalAttribute>();
            foreach (var optional in optionals.Concat(optionals2)) {
                if (optional != null && !ModSettings.GetOption(optional.Option)) {
                    Log.Debug($"Hiding {target.GetType().Name}::`{field.Name}` because {optional.Option} is disabled");
                    return null;
                }
            }

            var att = field.GetAttribute<CustomizablePropertyAttribute>();
            var hints = field.GetHints();
            hints.AddRange(field.FieldType.GetHints());
            string hint = hints.JoinLines();
            Log.Debug("hint is " + hint);

            Type enumType = field.FieldType.GetField("Required").FieldType;
            enumType = HintExtension.GetMappedEnumWithHints(enumType);

            IConvertible GetRequired() {
                object subTarget = field.GetValue(target);
                var flag = GetFieldValue(subTarget, "Required");
                return flag as IConvertible;
            }
            void SetRequired(IConvertible flags) {
                var subTarget = field.GetValue(target);
                SetFieldValue(target: subTarget, fieldName: "Required", value: flags);
                field.SetValue(target, subTarget);
            }
            IConvertible GetForbidden() {
                object subTarget = field.GetValue(target);
                return GetFieldValue(subTarget, "Forbidden") as IConvertible;
            }
            void SetForbidden(IConvertible flags) {
                var subTarget = field.GetValue(target);
                SetFieldValue(target: subTarget, fieldName: "Forbidden", value: flags);
                field.SetValue(target, subTarget);
            }

            var flagDataRequired = new FlagDataT(
                setValue: SetRequired,
                getValue: GetRequired,
                enumType: enumType);
            var flagDataForbidden = new FlagDataT(
                setValue: SetForbidden,
                getValue: GetForbidden,
                enumType: enumType);

            var ret0 = new FlagUIData {
                Label = att.name + " Required",
                Hint = hint,
                FlagData = flagDataRequired,
                RequiredFlag = true,
            };

            var ret1 = new FlagUIData {
                Label = att.name + "  Forbidden",
                Hint = hint,
                FlagData = flagDataForbidden,
                RequiredFlag = false,
            };

            return new[] { ret0, ret1 };
        }
    }
}

