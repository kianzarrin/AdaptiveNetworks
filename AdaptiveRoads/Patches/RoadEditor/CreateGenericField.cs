namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI.RoadEditor;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using System.Reflection;
    using static KianCommons.Assertion;
    using AdaptiveRoads.UI;
    using static KianCommons.ReflectionHelpers;

    /// <summary>
    /// extend CreateGenericField to support flag extension types.
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), "CreateGenericField")]
    public static class CreateGenericField {
        public static bool Prefix(string groupName, FieldInfo field, object target,
            RoadEditorPanel __instance) {
            if (NetInfoExtionsion.EditedNetInfo == null)
                return true; // ignore this outside of asset editor.
            if (Extensions.RequiresUserFlag(field.FieldType))
                return true;
            if (field.HasAttribute<BitMaskAttribute>() && field.HasAttribute<CustomizablePropertyAttribute>()) {
                UIComponent container = __instance.m_Container;
                if (!string.IsNullOrEmpty(groupName)) {
                    container = __instance.GetGroupPanel(groupName).Container;
                }
                var att = field.GetCustomAttributes(typeof(CustomizablePropertyAttribute), false)[0] as CustomizablePropertyAttribute;
                var enumType = field.FieldType;
                if (enumType == typeof(NetSegment.Flags))
                    enumType = typeof(NetSegmentFlags);
                var bitMaskPanel = BitMaskPanel.Add(
                    roadEditorPanel: __instance,
                    container: container,
                    label: att.name,
                    enumType: enumType,
                    setHandler: val => field.SetValue(target, val),
                    getHandler: () => (int)field.GetValue(target),
                    hint: null,
                    dark: false);
                return false;
            }
            return true;
        }

        public static void Postfix(string groupName, FieldInfo field, object target, RoadEditorPanel __instance) {
            try {
                if (target is NetLaneProps.Prop prop) {
                    Log.Debug($"{__instance.name}.CreateGenericField.Prefix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
                    if (field.Name == nameof(NetLaneProps.Prop.m_endFlagsForbidden)) {
                        var metadata = prop.GetOrCreateMetaData();
                        foreach (var field2 in metadata.GetFieldsWithAttribute<CustomizablePropertyAttribute>()) {
                            CreateExtendedComponent(groupName, field2, metadata, __instance);
                        }
                    }
                } else if (target is NetInfo.Node node) {
                    Log.Debug($"{__instance.name}.CreateGenericField.Prefix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
                    if (field.Name == nameof(NetInfo.Node.m_flagsForbidden)) {
                        var fields = typeof(NetInfoExtionsion.Node).GetFields()
                            .Where(_field => _field.HasAttribute<CustomizablePropertyAttribute>());
                        var node2 = node.GetOrCreateMetaData();
                        foreach (var field2 in fields) {
                            CreateExtendedComponent(groupName, field2, node2, __instance);
                        }
                    }
                } else if (target is NetInfo.Segment segment) {
                    Log.Debug($"{__instance.name}.CreateGenericField.Prefix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
                    var fields = typeof(NetInfoExtionsion.Segment.FlagsT).GetFields()
                        .Where(_field => _field.HasAttribute<CustomizablePropertyAttribute>());
                    var segment2 = segment.GetOrCreateMetaData();
                    AssertNotNull(segment2, $"{segment}");
                    if (field.Name == nameof(NetInfo.Segment.m_forwardForbidden)) {
                        foreach (var field2 in fields) {
                            CreateExtendedComponent(groupName, field2, segment2.ForwardFlags, __instance, "Forward ");
                        }
                    } else if (field.Name == nameof(NetInfo.Segment.m_backwardForbidden)) {
                        foreach (var field2 in fields) {
                            CreateExtendedComponent(groupName, field2, segment2.BackwardFlags, __instance, "Backward ");
                        }
                    }

                }
            } catch (Exception e) {
                Log.Exception(e);
            }

        }

        public static void CreateExtendedComponent(
            string groupName, FieldInfo fieldInfo, object target, RoadEditorPanel instance, string prefix = "") {
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
            var optional = fieldInfo.GetAttribute<OptionalAttribute>();
            if (optional != null && !ModSettings.GetOption(optional.Option)) {
                Log.Debug($"Hiding {target.GetType().Name}::`{att.name}` because {optional.Option} is disabled");
                return;
            }

            var hints = fieldInfo.GetHints();
            hints.AddRange(fieldInfo.FieldType.GetHints());
            string hint = hints.JoinLines();
            Log.Debug("hint is " + hint);

            // TODO : use these instead
            int GetRequired2() {
                object subTarget = fieldInfo.GetValue(target);
                return (int)GetFieldValue("Required", subTarget);
            }

            void SetRequired2(int flags) {
                var subTarget = fieldInfo.GetValue(target);
                SetFieldValue("Required", subTarget, flags);
                fieldInfo.SetValue(target, subTarget);
            }

            int GetForbidden2() {
                object subTarget = fieldInfo.GetValue(target);
                return (int)GetFieldValue("Forbidden", subTarget);
            }

            void SetForbidden2(int flags) {
                var subTarget = fieldInfo.GetValue(target);
                SetFieldValue("Forbidden", subTarget, flags);
                fieldInfo.SetValue(target, subTarget);
            }


            if (fieldInfo.FieldType.HasAttribute<FlagPairAttribute>()) {
                Type enumType = fieldInfo.FieldType.GetField("Required").FieldType;
                if (fieldInfo.FieldType == typeof(NetInfoExtionsion.LaneInfoFlags)) {

                    int GetRequired() {
                        var value = (NetInfoExtionsion.LaneInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Required;
                    }
                    void SetRequired(int flags) {
                        var value = (NetInfoExtionsion.LaneInfoFlags)fieldInfo.GetValue(target);
                        value.Required = (NetLaneExt.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    int GetForbidden() {
                        var value = (NetInfoExtionsion.LaneInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Forbidden;
                    };
                    void SetForbidden(int flags) {
                        var value = (NetInfoExtionsion.LaneInfoFlags)fieldInfo.GetValue(target);
                        value.Forbidden = (NetLaneExt.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    var bitMaskPanel0 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Required",
                        enumType: typeof(NetLaneExt.Flags),
                        setHandler: SetRequired,
                        getHandler: GetRequired,
                        hint: hint,
                        false);
                    var bitMaskPanel1 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Forbidden",
                        enumType: typeof(NetLaneExt.Flags),
                        setHandler: SetForbidden,
                        getHandler: GetForbidden,
                        hint: hint,
                        true);
                } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.SegmentInfoFlags)) {
                    int GetRequired() {
                        var value = (NetInfoExtionsion.SegmentInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Required;
                    }
                    void SetRequired(int flags) {
                        var value = (NetInfoExtionsion.SegmentInfoFlags)fieldInfo.GetValue(target);
                        value.Required = (NetSegmentExt.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    int GetForbidden() {
                        var value = (NetInfoExtionsion.SegmentInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Forbidden;
                    };
                    void SetForbidden(int flags) {
                        var value = (NetInfoExtionsion.SegmentInfoFlags)fieldInfo.GetValue(target);
                        value.Forbidden = (NetSegmentExt.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    var bitMaskPanel0 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Required",
                        enumType: typeof(NetSegmentExt.Flags),
                        setHandler: SetRequired,
                        getHandler: GetRequired,
                        hint: hint,
                        false);
                    var bitMaskPanel1 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Forbidden",
                        enumType: typeof(NetSegmentExt.Flags),
                        setHandler: SetForbidden,
                        getHandler: GetForbidden,
                        hint: hint,
                        true);
                } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.SegmentEndInfoFlags)) {
                    int GetRequired() {
                        var value = (NetInfoExtionsion.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Required;
                    }
                    void SetRequired(int flags) {
                        var value = (NetInfoExtionsion.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                        value.Required = (NetSegmentEnd.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    int GetForbidden() {
                        var value = (NetInfoExtionsion.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Forbidden;
                    };
                    void SetForbidden(int flags) {
                        var value = (NetInfoExtionsion.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                        value.Forbidden = (NetSegmentEnd.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    var bitMaskPanel0 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Required",
                        enumType: typeof(NetSegmentEnd.Flags),
                        setHandler: SetRequired,
                        getHandler: GetRequired,
                        hint: hint,
                        false);
                    var bitMaskPanel1 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Forbidden",
                        enumType: typeof(NetSegmentEnd.Flags),
                        setHandler: SetForbidden,
                        getHandler: GetForbidden,
                        hint: hint,
                        true);
                } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.NodeInfoFlags)) {
                    int GetRequired() {
                        var value = (NetInfoExtionsion.NodeInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Required;
                    }
                    void SetRequired(int flags) {
                        var value = (NetInfoExtionsion.NodeInfoFlags)fieldInfo.GetValue(target);
                        value.Required = (NetNodeExt.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    int GetForbidden() {
                        var value = (NetInfoExtionsion.NodeInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Forbidden;
                    };
                    void SetForbidden(int flags) {
                        var value = (NetInfoExtionsion.NodeInfoFlags)fieldInfo.GetValue(target);
                        value.Forbidden = (NetNodeExt.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    var bitMaskPanel0 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Required",
                        enumType: typeof(NetNodeExt.Flags),
                        setHandler: SetRequired,
                        getHandler: GetRequired,
                        hint: hint,
                        false);
                    var bitMaskPanel1 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Forbidden",
                        enumType: typeof(NetNodeExt.Flags),
                        setHandler: SetForbidden,
                        getHandler: GetForbidden,
                        hint: hint,
                        true);
                } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.VanillaSegmentInfoFlags)) {
                    int GetRequired() {
                        var value = (NetInfoExtionsion.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Required;
                    }
                    void SetRequired(int flags) {
                        var value = (NetInfoExtionsion.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                        value.Required = (NetSegment.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    int GetForbidden() {
                        var value = (NetInfoExtionsion.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Forbidden;
                    };
                    void SetForbidden(int flags) {
                        var value = (NetInfoExtionsion.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                        value.Forbidden = (NetSegment.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    var bitMaskPanel0 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Required",
                        enumType: typeof(NetSegmentFlags),
                        setHandler: SetRequired,
                        getHandler: GetRequired,
                        hint: hint,
                       false);
                    var bitMaskPanel1 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Forbidden",
                        enumType: typeof(NetSegmentFlags),
                        setHandler: SetForbidden,
                        getHandler: GetForbidden,
                        hint: hint,
                       true);
                } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.VanillaNodeInfoFlags)) {
                    int GetRequired() {
                        var value = (NetInfoExtionsion.VanillaNodeInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Required;
                    }
                    void SetRequired(int flags) {
                        var value = (NetInfoExtionsion.VanillaNodeInfoFlags)fieldInfo.GetValue(target);
                        value.Required = (NetNode.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    int GetForbidden() {
                        var value = (NetInfoExtionsion.VanillaNodeInfoFlags)fieldInfo.GetValue(target);
                        return (int)value.Forbidden;
                    };
                    void SetForbidden(int flags) {
                        var value = (NetInfoExtionsion.VanillaNodeInfoFlags)fieldInfo.GetValue(target);
                        value.Forbidden = (NetNode.Flags)flags;
                        fieldInfo.SetValue(target, value);
                    };
                    var bitMaskPanel0 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Required",
                        enumType: typeof(NetNodeFlags),
                        setHandler: SetRequired,
                        getHandler: GetRequired,
                        hint: hint,
                       false);
                    var bitMaskPanel1 = BitMaskPanel.Add(
                        roadEditorPanel: instance,
                        container: container,
                        label: prefix + att.name + " Flags Forbidden",
                        enumType: typeof(NetNodeFlags),
                        setHandler: SetForbidden,
                        getHandler: GetForbidden,
                        hint: hint,
                       true);
                } else {
                    Log.Error($"CreateExtendedComponent: Unhandled field: {fieldInfo} att:{att.name} ");
                }
            } else if (fieldInfo.FieldType == typeof(NetInfoExtionsion.Range) &&
                       fieldInfo.Name.ToLower().Contains("speed")) {
                SpeedRangePanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: prefix + att.name,
                    target: target,
                    fieldInfo: fieldInfo);
            } else {
                Log.Error($"CreateExtendedComponent: Unhandled field: {fieldInfo} att:{att.name} ");
            }
        }

        //static int GetRequired2(FieldInfo fieldInfo, object target) {
        //    object subTarget = fieldInfo.GetValue(target);
        //    return (int)GetFieldValue("Required", subTarget);
        //}

        //static void SetRequired2(int flags, FieldInfo fieldInfo, object target) {
        //    var subTarget = fieldInfo.GetValue(target);
        //    SetFieldValue("Required", subTarget, flags);
        //    fieldInfo.SetValue(target, subTarget);
        //}

        //public static void CreateExtendedComponentHelper(FieldInfo field, object target, RoadEditorPanel instance, string subFieldName) {
        //    Assertion.Assert(field.FieldType.HasAttribute<FlagPairAttribute>(), "HasAttribute:FlagsPair");
        //    Assertion.Assert(field.HasAttribute<CustomizablePropertyAttribute>(), "HasAttribute:CustomizablePropertyAttribute");
        //    CustomizablePropertyAttribute att =
        //        field.GetCustomAttributes(typeof(CustomizablePropertyAttribute), true)[0] as CustomizablePropertyAttribute;
        //    string label = att.name + "Flags" + subFieldName;
        //    UIComponent container = instance.m_Container;


        //    target = field.GetValue(target); // target should be the instance that contains subField. // BUG: DO struct does not work.
        //    var subField = field.FieldType.GetField(subFieldName);

        //    //from: CreateFieldComponent()
        //    var repropertySet = UITemplateManager.Get<REPropertySet>(RoadEditorPanel.kEnumBitmaskSet);

        //    //from CreateGenericField:
        //    container.AttachUIComponent(repropertySet.gameObject);
        //    instance.FitToContainer(repropertySet.component);
        //    repropertySet.EventPropertyChanged += instance.OnObjectModified; //() => instance.OnObjectModified();

        //    // from: repropertySet.SetTarget(target, field);
        //    repropertySet.set_Target(target);
        //    repropertySet.set_TargetField(subField);
        //    (repropertySet as REEnumBitmaskSet).Initialize(target, subField, label);

        //}
    }
}

