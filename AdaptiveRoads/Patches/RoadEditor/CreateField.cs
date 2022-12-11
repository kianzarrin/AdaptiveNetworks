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
    using AdaptiveRoads.Data;
    using TagsInfo = AdaptiveRoads.Manager.NetInfoExtionsion.TagsInfo;
    using static AdaptiveRoads.Manager.NetInfoExtionsion;
    using System.Globalization;

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
                        field)) {
                    return false;
                }

                if (ModSettings.ARMode &&
                    field.FieldType == typeof(NetInfo.ConnectGroup)) {
                    CreateConnectGroupComponent(__instance, groupName, target, field);
                    return false;
                }

                if (IsUIReplaced(field)) {
                    if (VanillaCanMerge(field))
                        return false; // will be merged with AN drop-down later or has been merge with Node.Flags earlier
                    var container = GetContainer(__instance, groupName);
                    var uidata = GetVanillaFlagUIData(field, target);
                    var bitMaskPanel = BitMaskPanel.Add(
                        roadEditorPanel: __instance,
                        container: container,
                        label: uidata.Label,
                        hint: uidata.Hint,
                        flagData: uidata.FlagData);
                    return false;
                }
            } catch (Exception ex) { ex.Log(); }
            return true;
        }

        [HarmonyPatch(typeof(RoadEditorPanel), "CreateField")]
        public static bool Prefix(RoadEditorPanel __instance, FieldInfo field, object target) {
            try {
                if (field.Name.ToLower().Contains("tags")) {
                    // special tag section.
                    if (target is NetInfo netInfo) {
                        string groupName = field.GetAttribute<CustomizablePropertyAttribute>()?.group;
                        if (field.Name == nameof(NetInfo.m_tags)) {
                            CustomTagsDataT data = new(netInfo, nameof(netInfo.m_tags));
                            StringListMSDD.Add(
                                roadEditorPanel: __instance,
                                container: GetContainer(__instance, "Properties"),
                                label: "Tags",
                                hint: null,
                                customStringData: data);
                            return false;
                        }
                    } else if (target is NetInfo.Node nodeInfo) {
                        if (ReplaceTagsSection(__instance, nodeInfo, field)) {
                            return false;
                        }
                    }
                }
            } catch (Exception ex) { ex.Log(); }
            return true;
        }


        /// <returns>true if replaced, false to use original</returns>
        public static bool ReplaceTagsSection(RoadEditorPanel roadEditorPanel, NetInfo.Node nodeInfo, FieldInfo field) {
            var container = GetContainer(roadEditorPanel, NetInfoExtionsion.Node.TAG_GROUP_NAME);
            const string hint =
                "tags can apply for nodes with or without Direct connect.\n" +
                "For Direct connect nodes the required/forbidden criteria is  additionally checked on the segment.";
            switch (field.Name) {
                case nameof(nodeInfo.m_tagsRequired):
                    StringListMSDD.Add(
                        roadEditorPanel: roadEditorPanel,
                        container: container,
                        label: "Required",
                        hint: hint,
                        customStringData: new CustomTagsDataT(nodeInfo, nameof(NetInfo.Node.m_tagsRequired)));
                    StringListMSDD.Add(
                        roadEditorPanel: roadEditorPanel,
                        container: container,
                        label: "Forbidden",
                        hint: hint,
                        customStringData: new CustomTagsDataT(nodeInfo, nameof(NetInfo.Node.m_tagsForbidden)));
                    FieldInfo f_forbidAnyTags = typeof(NetInfo.Node).GetField(nameof(NetInfo.Node.m_forbidAnyTags));
                    roadEditorPanel.CreateGenericField(groupName: NetInfoExtionsion.Node.TAG_GROUP_NAME, field: f_forbidAnyTags, target: nodeInfo);
                    RangePanel8.Add(
                    roadEditorPanel: roadEditorPanel,
                    container: container,
                    label: "match         count",
                    hint: "number of segments that match required/forbidden criteria",
                    from: RefChain.Create(nodeInfo).Field<byte>(nameof(NetInfo.Node.m_minSameTags)),
                    to: RefChain.Create(nodeInfo).Field<byte>(nameof(NetInfo.Node.m_maxSameTags)));
                    RangePanel8.Add(
                    roadEditorPanel: roadEditorPanel,
                    container: container,
                    label: "mismatch count",
                    hint: "number of segments that do not match required/forbidden criteria",
                    from: RefChain.Create(nodeInfo).Field<byte>(nameof(NetInfo.Node.m_minOtherTags)),
                    to: RefChain.Create(nodeInfo).Field<byte>(nameof(NetInfo.Node.m_maxOtherTags)));
                    return true;
                case nameof(nodeInfo.m_tagsForbidden):
                case nameof(nodeInfo.m_forbidAnyTags):
                case nameof(nodeInfo.m_minSameTags):
                case nameof(nodeInfo.m_maxSameTags):
                case nameof(nodeInfo.m_minOtherTags):
                case nameof(nodeInfo.m_maxOtherTags):
                    return true; // already covered above
                default:
                    // unknown field.
                    return false;
            }
        }

        public static void CreateTagsSection(RoadEditorPanel roadEditorPanel, object metadata, FieldInfo fieldInfo) {
            string group = fieldInfo.GetAttribute<CustomizablePropertyAttribute>().name;
            var container = GetContainer(roadEditorPanel, group);
            const string hint = null;
            RefChain refChainRoot = RefChain.Create(metadata).Field(fieldInfo);

            var traverseRequired = refChainRoot.Field<string[]>(nameof(TagsInfo.Required));
            StringListMSDD.Add(
                roadEditorPanel: roadEditorPanel,
                container: container,
                label: "Required",
                hint: hint,
                customStringData: new CustomTagsDataT(traverseRequired));

            var traverseForbidden = refChainRoot.Field<string[]>(nameof(TagsInfo.Forbidden));
            StringListMSDD.Add(
                roadEditorPanel: roadEditorPanel,
                container: container,
                label: "Forbidden",
                hint: hint,
                customStringData: new CustomTagsDataT(traverseForbidden));

            EditorCheckboxPanel.Add(
                roadEditorPanel,
                container,
                label: "Forbid Any Tags",
                hint: "Forbid all other tags",
                refChain: refChainRoot.Field<bool>(nameof(TagsInfo.ForbidAll)));

            RangePanel8.Add(
                roadEditorPanel: roadEditorPanel,
                container: container,
                label: "match         count",
                hint: "number of segments that match required/forbidden criteria",
                from: refChainRoot.Field<byte>(nameof(TagsInfo.MinMatch)),
                to: refChainRoot.Field<byte>(nameof(TagsInfo.MaxMatch)));
            RangePanel8.Add(
                roadEditorPanel: roadEditorPanel,
                container: container,
                label: "mismatch count",
                hint: "number of segments that do not match required/forbidden criteria",
                from: refChainRoot.Field<byte>(nameof(TagsInfo.MinMismatch)),
                to: refChainRoot.Field<byte>(nameof(TagsInfo.MaxMismatch)));
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

                if (ModSettings.ARMode) {
                    var metaData = GenericUtil.GetMetaData(target);

                    if (metaData != null) {
#if DEBUG
                        Log.Called(field, target, groupName);
#endif
                        foreach (var field2 in field.GetAfterFields(metaData)) {
                            CreateGenericComponentExt(
                                roadEditorPanel: __instance, groupName: groupName,
                                target: target, metadata: metaData, extensionField: field2);
                        }
                    }
                }

                //////////////////////////////////
                // special cases: replace Labels, expose fields, buttons

                if (target is NetLaneProps.Prop prop) {
                    ReplaceLabel(__instance, "Start Flags Required:", "Tail Node Flags Required:");
                    ReplaceLabel(__instance, "Start Flags Forbidden:", "Tail  Node Flags Forbidden:");
                    ReplaceLabel(__instance, "End Flags Required:", "Head  Node Flags Required:");
                    ReplaceLabel(__instance, "End Flags Forbidden:", "Head  Node Flags Forbidden:");

                    if (typeof(NetInfoExtionsion.LaneProp).ComesAfter(field)) {
                        Assert(prop.LocateEditProp(out _, out var lane), "could not locate prop");
                        bool forward = lane.IsGoingForward();
                        bool backward = lane.IsGoingBackward();
                        bool unidirectional = forward || backward;

                        if (!unidirectional) {
                            EditorButtonPanel.Add(
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


                        EditorButtonPanel.Add(
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
                } else if (target is NetInfo netInfo) {
                    if (field.Name == nameof(NetInfo.m_surfaceLevel)) {
                        ExposeField(
                            roadEditorPanel: __instance,
                            groupName: groupName,
                            target: netInfo,
                            fieldName: nameof(NetInfo.m_terrainStartOffset),
                            label: "Terrain Start Offset");
                        ExposeField(
                            roadEditorPanel: __instance,
                            groupName: groupName,
                            target: netInfo,
                            fieldName: nameof(NetInfo.m_terrainEndOffset),
                            label: "Terrain End Offset");
                    }
                    if (ModSettings.ARMode) {
                        ReplaceLabel(__instance, "Pavement Width", "Pavement Width Left");
                        if (field.Name == nameof(NetInfo.m_surfaceLevel)) {
                            Log.Debug("adding QuayRoads button");
                            var qrButtonPanel = EditorButtonPanel.Add(
                            roadEditorPanel: __instance,
                            container: __instance.GetGroupPanel("Properties").m_Panel,
                            label: "Edit QuayRoads profile",
                            hint: "",
                            action: () => QuayRoadsPanel.GetOrOpen(netInfo, __instance));
                            qrButtonPanel.EventDestroy += (_, _) => { QuayRoadsPanel.CloseIfOpen(netInfo); };
                        }
                    }
                } else if (target is NetInfo.Lane laneInfo) {
                    if (ModSettings.ARMode) {
                        if (field.Name == nameof(NetInfo.Lane.m_stopType)) {
                            NetInfo netInfo2 = laneInfo.GetParent(out _);
                            var laneTags = netInfo2.GetMetaData().Lanes.GetOrCreate(laneInfo).LaneTags;
                            StringListMSDD.Add(
                                roadEditorPanel: __instance,
                                container: GetContainer(__instance, groupName),
                                label: "Lane Tags",
                                hint: null,
                                customStringData: new CustomTagBaseDataT(laneTags));
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

        static IEnumerable<MemberInfo> GetAfterMemebers(this FieldInfo before, object target) {
            return target.GetMembersWithAttribute<CustomizablePropertyAttribute>()
                .Where(member => member.ComesAfter(before));
        }


        static bool ComesAfter(this MemberInfo after, FieldInfo before) {
            Assert(after != before, $"{after} != {before}");
            return after.AfterMember() == before.Name;
        }

        static string AfterMember(this MemberInfo member) {
            return
                member.GetAttribute<AfterFieldAttribute>()?.FieldName ??
                member.DeclaringType.GetAttribute<AfterFieldAttribute>()?.FieldName ??
                throw new Exception("could not find after field for " + member);
        }

        /// <returns>false if this field is not special case and needs to be handled normally.</returns>
        public static bool CreateGenericComponentExt0(
            RoadEditorPanel roadEditorPanel, string groupName,
            object target, object metadata, FieldInfo extensionField) {
            if (target != metadata && TryGetMerge(extensionField, target, out var vanillaRequired, out var vanillaForbidden)) {
                CreateMergedComponent(
                    roadEditorPanel: roadEditorPanel, groupName: groupName,
                    fieldInfo: extensionField, metadata: metadata,
                    mergedFieldRequired: vanillaRequired,
                    mergedFieldForbidden: vanillaForbidden,
                    mergedTarget: target);
            } else if (TryGetMergeExt(extensionField, metadata, out var vanilla)) {
                CreateMergedComponent(
                    roadEditorPanel: roadEditorPanel, groupName: groupName,
                    fieldInfo: extensionField, mergedField: vanilla, metadata: metadata);
            } else if (ARCanMerge(extensionField)) {
                // do not create because it will merge with something in future.
            } else {
                return CreateExtendedComponent(roadEditorPanel, groupName, extensionField, metadata);
            }
            return true; // handled
        }

        public static void CreateGenericComponentExt(
            RoadEditorPanel roadEditorPanel, string groupName,
            object target, object metadata, FieldInfo extensionField) {
            if (!CreateGenericComponentExt0(roadEditorPanel, groupName, target, metadata, extensionField)) {
                roadEditorPanel.CreateField(extensionField, metadata);
            }
        }

        public static void CreateButton(
            RoadEditorPanel roadEditorPanel, string groupName,
            object target, object metadata, MethodInfo extensionMethod) {
            Log.Called();
            var att = extensionMethod.GetAttribute<CustomizableActionAttribute>();
            Assertion.NotNull(att, "att");
            if (!string.IsNullOrEmpty(att.group)) groupName = att.group; 
            var container = GetContainer(roadEditorPanel, groupName);

            var button = EditorButtonPanel.Add(
                roadEditorPanel: roadEditorPanel,
                container: container,
                label: att.name,
                hint: extensionMethod.GetHints()?.JoinLines(),
                action: () => extensionMethod.Invoke(metadata, new object[] { target }));
            button.eventClick += (_, __) => {
                roadEditorPanel.OnObjectModified();
            };
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

                if (fieldInfo.FieldType == typeof(TagsInfo)) {
                    CreateTagsSection(roadEditorPanel, metadata, fieldInfo);
                } else if (typeof(TagBase).IsAssignableFrom(fieldInfo.FieldType)) {
                    TagBase tagBase = (TagBase)fieldInfo.GetValue(metadata);
                    StringListMSDD.Add(
                        roadEditorPanel: roadEditorPanel,
                        container: container,
                        label: att.name,
                        hint: fieldInfo.GetHints().JoinLines(),
                        customStringData: new CustomTagBaseDataT(tagBase));
                } else if (fieldInfo.HasAttribute<BitMaskLanesAttribute>()) {
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
                } else if (fieldInfo.FieldType.HasAttribute<FlagPairAttribute>()) {
                    var uidatas = GetARFlagUIData(fieldInfo, metadata);
                    if (uidatas != null) {
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
                } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.Range)) {
                    if (fieldInfo.Name.ToLower().Contains("speed")) {
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
                } else if (fieldInfo.FieldType == typeof(UserDataInfo)) {
                    CreateUserDataInfoSection(
                        roadEditorPanel: roadEditorPanel,
                        target: metadata,
                        fieldInfo: fieldInfo,
                        groupName: groupName);
                } else if (fieldInfo.FieldType == typeof(string)) {
                    var panel = EditorStringPanel.Add(
                        roadEditorPanel: roadEditorPanel,
                        container: container,
                        label: att.name,
                        hint: fieldInfo.GetHints().JoinLines(),
                        target: metadata,
                        fieldInfo: fieldInfo);
                } else if (fieldInfo.FieldType == typeof(NetLaneProps)) {
                    FieldInfo field2 = typeof(NetLaneProps).GetField("m_props");
                    NetLaneProps laneprops = fieldInfo.GetValue(metadata) as NetLaneProps;
                    Assertion.NotNull(laneprops);
                    roadEditorPanel.CreateField(field2, laneprops);
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
            //Assert(fieldInfo.Name == nameof(NetInfo.m_connectGroup));
            Log.Called(roadEditorPanel, groupName, target, fieldInfo);

            CustomConnectGroupT tagSource = null;
            if (target is NetInfo.Node nodeInfo)
                tagSource = nodeInfo.GetOrCreateMetaData().CustomConnectGroups;
            else if (target is NetInfo netInfo)
                tagSource = netInfo.GetOrCreateMetaData().CustomConnectGroups;
            else if (target is NetInfoExtionsion.Track track)
                tagSource = track.CustomConnectGroups;
            Assertion.NotNull(tagSource, "tagSource");

            var container = GetContainer(roadEditorPanel, groupName);
            var uidata = GetVanillaFlagUIData(fieldInfo, target);

            var bitMaskPanel = BitMaskPanelCustomisable.Add(
                roadEditorPanel: roadEditorPanel,
                container: container,
                label: uidata.Label,
                hint: uidata.Hint,
                flagData: uidata.FlagData,
                Tags: tagSource);
        }

        public static void CreateUserDataInfoSection(RoadEditorPanel roadEditorPanel, object target, FieldInfo fieldInfo, string groupName) {
            Assert(fieldInfo.FieldType == typeof(UserDataInfo), "field type is UserDataInfo");
            Log.Called(roadEditorPanel, target, fieldInfo);
            groupName = fieldInfo.GetAttribute<CustomizablePropertyAttribute>()?.group ?? groupName;
            NetInfo netInfo = RoadEditorUtils.GetSelectedNetInfo(out _);
            var net = netInfo.GetMetaData();
            UserDataInfo userDataInfo = fieldInfo.GetValue(target) as UserDataInfo;
            var userValues = userDataInfo?.UserValues;
            if (!userValues.IsNullorEmpty()) {
                var userDatanames = net.UserDataNamesSet ??= new();
                for (int i = 0; i < userValues.Length; ++i) {
                    CreateUserDataValueEntry(
                        roadEditorPanel: roadEditorPanel,
                        groupName,
                        target:target,
                        fieldInfo:fieldInfo,
                        index: i);
                }
            }

            var groupPanel = roadEditorPanel.GetGroupPanel(groupName);
            var addButton = groupPanel.Container.AddUIComponent<EditorButon>();
            addButton.text = "Add New Selector";
            addButton.width = 200;
            addButton.eventClick += (c, __) => {
                UserValueDropDownPanel.AddNewEntryMiniPanel(entry => {
                    net.UserDataNamesSet ??= new();
                    net.UserDataNamesSet.Segment.Add(entry);
                    net.AllocateUserData();
                    CreateUserDataValueEntry(
                        roadEditorPanel: roadEditorPanel,
                        groupName,
                        target: target,
                        fieldInfo: fieldInfo,
                        index: null);
                });
            };
        }

        /// <param name="index">null => last</param>
        public static void CreateUserDataValueEntry(
            RoadEditorPanel roadEditorPanel, string groupName, object target, FieldInfo fieldInfo, int ?index) {
            Assert(fieldInfo.FieldType == typeof(UserDataInfo), "field type is UserDataInfo");
            Log.Called(roadEditorPanel, target, fieldInfo);
            var groupPanel = roadEditorPanel.GetGroupPanel(groupName);
            Assertion.NotNull(groupName, "groupName");
            NetInfo netInfo = RoadEditorUtils.GetSelectedNetInfo(out _);
            Assertion.NotNull(netInfo, "netInfo");
            var net = netInfo.GetMetaData();
            Assertion.NotNull(net,"net");
            UserDataInfo userDataInfo = fieldInfo.GetValue(target) as UserDataInfo;
            Assertion.NotNull(userDataInfo,"userDataInfo");

            var userDatanames = net.UserDataNamesSet ??= new();
            var userValues = userDataInfo?.UserValues;
            Assertion.NotNull(userValues,"userValues");
            int i = index ?? userValues.Length - 1;

            Assertion.NotNull(userDatanames?.Segment?.ValueNames,
                $"userDatanames?.Segment?.ValueNames={userDatanames}?.{userDatanames?.Segment}?.{userDatanames?.Segment?.ValueNames}");
            var data = new UserValueDropDownPanel.DataT {
                Index = i,
                Names = userDatanames.Segment.ValueNames[i],
                Target = userDataInfo,
            };
            data.eventNamesUpdated += (names) => userDatanames.Segment.ValueNames[i] = names;
            data.eventEntryRemoved += (index) => {
                try {
                    Log.Called(index);
                    net.RemoveSegmentUserValue(index);

                    // fix indeces:
                    var groupPanel = roadEditorPanel.GetGroupPanel(groupName);
                    foreach(var panel in groupPanel.GetComponentsInChildren<UserValueDropDownPanel>()) {
                        if (panel.Data.Index > index)
                            panel.Data.Index--;
                    }

                } catch (Exception ex) {
                    ex.Log();
                }
            };

            var panel = UserValueDropDownPanel.Add(
                roadEditorPanel: roadEditorPanel,
                groupPanel: groupPanel,
                hint: "user can select one value from the drop down. None => not used in this model",
                data: data);
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

                var vanillaFields = new[] { mergedFieldRequired, mergedFieldForbidden };
                FlagUIData[] arDatas = GetARFlagUIData(fieldInfo, metadata);
                Assertion.Equal(arDatas.Length, 2, "arDatas.Length");
                for (int i = 0; i < 2; ++i) {
                    FieldInfo vanillaField = vanillaFields[i];
                    FlagUIData vanillaData = GetVanillaFlagUIData(vanillaField, mergedTarget);
                    string hint = arDatas.IsNullorEmpty() ? vanillaData.Hint : arDatas[i].Hint;

                    List<FlagDataT> flagDatas = new();
                    flagDatas.Add(vanillaData.FlagData);
                    if(!arDatas.IsNullorEmpty())
                        flagDatas.Add(arDatas[i].FlagData);

                    MultiBitMaskPanel.Add(
                        roadEditorPanel: roadEditorPanel,
                        container: container,
                        label: vanillaData.Label,
                        hint: hint,
                        flagDatas: flagDatas.ToArray());
                    
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
            vanillaRequiredField = vanillaForbiddenField = null;
            if (vanillaTarget is NetInfo or NetAI) {
                return false;
            }
            Assert(vanillaTarget is IInfoExtended, "vanillaTarget is IInfoExtended : " + vanillaTarget.ToSTR());
            if (!Merge) return false;

            foreach (var vanillaField in vanillaTarget.GetFieldsWithAttribute<CustomizablePropertyAttribute>()) {
                if (CanMergeWith(extensionField, vanillaField) && !vanillaField.Name.EndsWith("2")) {
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

        static bool TryGetReplacementProperty(FieldInfo field, object target, out PropertyInfo property) {
            property = null;
            if (target is NetInfo or NetAI) {
                return false;
            }

            foreach (var otherProperty in target.GetType().GetProperties()) {
                if(field.Name == "m_" + otherProperty.Name) {
                    property = otherProperty;
                    return true;
                }
            }

            return false;
        }

        ///<summary>
        /// get extensions fields that can be merged together.
        /// </summary>
        static bool TryGetMergeExt(FieldInfo extensionField, object metadata, out FieldInfo vanillaFlags) {
            vanillaFlags = null;
            if (!Merge) return false;

            foreach (var vanillaField in metadata.GetFieldsWithAttribute<CustomizablePropertyAttribute>()) {
                if (CanMergeWithExt(extensionField, vanillaField)) {
                    vanillaFlags = vanillaField;
                    return true;
                }
            }
            return false;

            static bool CanMergeWithExt(FieldInfo extensionField, FieldInfo vanillaField) {
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
                enumType == typeof(NetNode.FlagsLong) ||
                enumType == typeof(NetSegment.Flags) ||
                enumType == typeof(NetLane.Flags);
        }

        static bool VanillaCanMerge(FieldInfo field) {
            if (!Merge) return false;

            Type fieldType = field.FieldType;

            if (fieldType == typeof(NetNode.Flags2) ||
                fieldType == typeof(VehicleInfo.VehicleCategoryPart2)) {
                // always merge
                return true; 
            }

            if (field.DeclaringType == typeof(NetInfo.Lane) && fieldType == typeof(NetLane.Flags)) {
                // exception
                return false; 
            }

            return // merged only in ARMode:
                fieldType == typeof(NetNode.Flags) ||
                fieldType == typeof(NetSegment.Flags) ||
                fieldType == typeof(NetLane.Flags);
        }

        public static UIComponent GetContainer(RoadEditorPanel roadEditorPanel, string groupName) {
            UIComponent container = roadEditorPanel.m_Container;
            if (!string.IsNullOrEmpty(groupName)) {
                container = roadEditorPanel.GetGroupPanel(groupName).Container;
            }
            AssertNotNull(container, "container");
            return container;
        }

        public static Type GetMemberType(this MemberInfo memberInfo) {
            return memberInfo switch {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => throw new NotImplementedException(),
            };
        }
        public static object GetValue(this MemberInfo memberInfo, object obj) {
            return memberInfo switch {
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj, null),
                _ => throw new NotImplementedException(),
            };
        }
        public static void SetValue(this MemberInfo memberInfo, object obj, object value) {
            switch(memberInfo) {
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue(obj, value);
                    break;
                case PropertyInfo propertyInfo:
                    propertyInfo.SetValue(obj, value, null);
                    break;
                default:
                    throw new NotImplementedException();
            };
        }

        struct FlagUIData {
            internal string Label;
            internal string Hint;
            internal FlagDataT FlagData;
        }

        static VehicleInfo.VehicleCategory VehicleCategoryALL;
        static CreateField() {
            foreach(var item in EnumBitMaskExtensions.GetPow2Values<VehicleInfo.VehicleCategory>()) {
                VehicleCategoryALL |= item;
            }
        }

        /// <summary>
        /// fills in FlagUIData for a vanilla field. uses replacement property or merges field together where appropriate.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        static FlagUIData GetVanillaFlagUIData(FieldInfo field, object target) {
            AssertNotNull(target, "target");
            AssertNotNull(field, "member");
            Assert(field.HasAttribute<CustomizablePropertyAttribute>(), "field has CustomizablePropertyAttribute");
            Assert(field.HasAttribute<BitMaskAttribute>(), "field has BitMaskAttribute");
            Assert(field.GetMemberType().HasAttribute<FlagsAttribute>(), "field has FlagsAttribute");

            FlagDataT flagData;
            Type enumType;
            if(field.FieldType == typeof(VehicleInfo.VehicleCategoryPart1)) {
                // handle vehicle category
                var field2 = target.GetFieldsWithAttribute<CustomizablePropertyAttribute>()
                    .Single(item => item.FieldType == typeof(VehicleInfo.VehicleCategoryPart2));
                enumType = typeof(VehicleInfo.VehicleCategory);

                flagData = new FlagDataT(
                    setValue: val => {
                        var cat = (VehicleInfo.VehicleCategory)val;
                        if(cat == VehicleCategoryALL) cat = (VehicleInfo.VehicleCategory)(-1L);
                        VehicleInfo.GetCategoryParts(cat, out var val1, out var val2);
                        field.SetValue(target, val1);
                        field2.SetValue(target, val2);
                    },
                    getValue: () => {
                        var val1 = (VehicleInfo.VehicleCategoryPart1)field.GetValue(target);
                        var val2 = (VehicleInfo.VehicleCategoryPart2)field2.GetValue(target);
                        var cat = VehicleInfo.GetCategory(val1, val2);
                        if (cat == VehicleCategoryALL) cat = (VehicleInfo.VehicleCategory)(-1L);
                        return cat;
                    },
                    enumType: enumType);
            } else if (TryGetReplacementProperty(field, target, out PropertyInfo property)) {
                enumType = property.PropertyType;
                enumType = HintExtension.GetMappedEnumWithHints(enumType);

                // handle NetNode.FlagsLong
                flagData = new FlagDataT(
                    setValue: val => property.SetValue(target, val),
                    getValue: () => property.GetValue(target) as IConvertible,
                    enumType: enumType);
            } else {
                enumType = field.GetMemberType();
                enumType = HintExtension.GetMappedEnumWithHints(enumType);
                flagData = new FlagDataT(
                    setValue: val => field.SetValue(target, val),
                    getValue: () => field.GetValue(target) as IConvertible,
                    enumType: enumType);
            }


            var hints = field.GetHints();
            if (field.Name == "m_stopType")
                hints.Add("set this for the pedestrian lane that contains the bus/tram stop.");
            hints.AddRange(enumType.GetHints());
            string hint = hints.JoinLines();
            Log.Debug($"{field} -> hint is: " + hint);
            return new FlagUIData {
                Label = field.GetAttribute<CustomizablePropertyAttribute>().name.Remove("Part 1"),
                Hint = hint,
                FlagData = flagData,
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
            };

            var ret1 = new FlagUIData {
                Label = att.name + "  Forbidden",
                Hint = hint,
                FlagData = flagDataForbidden,
            };

            return new[] { ret0, ret1 };
        }
    }
}

