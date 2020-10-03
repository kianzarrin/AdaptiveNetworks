namespace AdvancedRoads.Patches.RoadEditor {
    using AdvancedRoads.Manager;
    using AdvancedRoads.UI.RoadEditor;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Linq;
    using System.Reflection;
    using static KianCommons.Assertion;

    /// <summary>
    /// extend CreateGenericField to support flag extension types.
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), "CreateGenericField")]
    public static class CreateGenericField {
        public static void Postfix(string groupName, FieldInfo field, object target, RoadEditorPanel __instance) {
            try {
                if (target is NetLaneProps.Prop target2) {
                    Log.Debug($"{__instance.name}.CreateGenericField.Prefix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
                    if (field.Name == nameof(NetLaneProps.Prop.m_flagsForbidden)) {
                        var fields = typeof(NetInfoExt.LaneProp).GetFields()
                            .Where(_field => _field.HasAttribute<CustomizablePropertyAttribute>());
                        var target3 = target2 as PrefabIndeces.NetInfoExtension.Lane.Prop;
                        AssertNotNull(target3, "[3]target:" + target);
                        NetInfoExt.LaneProp target4 = target3.GetExt();
                        AssertNotNull(target4, $"[4]target:{target} Buffer[{target3.PrefabIndex}]={NetInfoExt.Buffer[target3.PrefabIndex]}");
                        foreach (var field2 in fields) {
                            CreateExtendedComponent(groupName, field2, target4, __instance);
                        }
                    }
                }
            }
            catch (Exception e) {
                Log.Exception(e);
                throw e;
            }

        }
        public static void CreateExtendedComponent(string groupName, FieldInfo fieldInfo, object target, RoadEditorPanel instance) {
            if (!fieldInfo.FieldType.HasAttribute<FlagPairAttribute>())
                return;
            Assert(string.IsNullOrEmpty(groupName), "groupName is empty");
            var container = instance.component.GetComponentInChildren<UIScrollablePanel>();
            AssertNotNull(container, "container");
            Log.Debug("CreateExtendedComponent():container=" + container);

            Assert(fieldInfo.FieldType.HasAttribute<FlagPairAttribute>(), "HasAttribute:FlagsPair");
            Type enumType = fieldInfo.FieldType.GetField("Required").FieldType;

            Assert(fieldInfo.HasAttribute<CustomizablePropertyAttribute>(), "HasAttribute:CustomizablePropertyAttribute");
            AssertNotNull(target, "target");
            AssertNotNull(target, "fieldInfo");
            AssertNotNull(target, "RoadEditorPanel instance");
            Log.Debug(
                $"CreateExtendedComponent(groupName={groupName}, fieldInfo={fieldInfo}, target={target}, instance={instance.name}) called",
                false);

            var att = fieldInfo.GetCustomAttributes(typeof(CustomizablePropertyAttribute), false)[0] as CustomizablePropertyAttribute;

            if (fieldInfo.FieldType == typeof(NetInfoExt.LaneInfoFlags)) {

                int GetRequired() {
                    var value = (NetInfoExt.LaneInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Required;
                }
                void SetRequired(int flags) {
                    var value = (NetInfoExt.LaneInfoFlags)fieldInfo.GetValue(target);
                    value.Required = (NetLaneExt.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                int GetForbidden() {
                    var value = (NetInfoExt.LaneInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Forbidden;
                };
                void SetForbidden(int flags) {
                    var value = (NetInfoExt.LaneInfoFlags)fieldInfo.GetValue(target);
                    value.Forbidden = (NetLaneExt.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                var bitMaskPanel0 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Required",
                    enumType: typeof(NetLaneExt.Flags),
                    setHandler: SetRequired,
                    getHandler: GetRequired,
                    false);
                var bitMaskPanel1 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Forbidden",
                    enumType: typeof(NetLaneExt.Flags),
                    setHandler: SetForbidden,
                    getHandler: GetForbidden,
                    true);
            } else if (fieldInfo.FieldType == typeof(NetInfoExt.SegmentInfoFlags)) {
                int GetRequired() {
                    var value = (NetInfoExt.SegmentInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Required;
                }
                void SetRequired(int flags) {
                    var value = (NetInfoExt.SegmentInfoFlags)fieldInfo.GetValue(target);
                    value.Required = (NetSegmentExt.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                int GetForbidden() {
                    var value = (NetInfoExt.SegmentInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Forbidden;
                };
                void SetForbidden(int flags) {
                    var value = (NetInfoExt.SegmentInfoFlags)fieldInfo.GetValue(target);
                    value.Forbidden = (NetSegmentExt.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                var bitMaskPanel0 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Required",
                    enumType: typeof(NetSegmentExt.Flags),
                    setHandler: SetRequired,
                    getHandler: GetRequired,
                    false);
                var bitMaskPanel1 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Forbidden",
                    enumType: typeof(NetSegmentExt.Flags),
                    setHandler: SetForbidden,
                    getHandler: GetForbidden,
                    true);
            } else if (fieldInfo.FieldType == typeof(NetInfoExt.SegmentEndInfoFlags)) {
                int GetRequired() {
                    var value = (NetInfoExt.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Required;
                }
                void SetRequired(int flags) {
                    var value = (NetInfoExt.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                    value.Required = (NetSegmentEnd.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                int GetForbidden() {
                    var value = (NetInfoExt.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Forbidden;
                };
                void SetForbidden(int flags) {
                    var value = (NetInfoExt.SegmentEndInfoFlags)fieldInfo.GetValue(target);
                    value.Forbidden = (NetSegmentEnd.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                var bitMaskPanel0 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Required",
                    enumType: typeof(NetSegmentEnd.Flags),
                    setHandler: SetRequired,
                    getHandler: GetRequired,
                    false);
                var bitMaskPanel1 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Forbidden",
                    enumType: typeof(NetSegmentEnd.Flags),
                    setHandler: SetForbidden,
                    getHandler: GetForbidden,
                    true);
            } else if (fieldInfo.FieldType == typeof(NetInfoExt.NodeInfoFlags)) {
                int GetRequired() {
                    var value = (NetInfoExt.NodeInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Required;
                }
                void SetRequired(int flags) {
                    var value = (NetInfoExt.NodeInfoFlags)fieldInfo.GetValue(target);
                    value.Required = (NetNodeExt.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                int GetForbidden() {
                    var value = (NetInfoExt.NodeInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Forbidden;
                };
                void SetForbidden(int flags) {
                    var value = (NetInfoExt.NodeInfoFlags)fieldInfo.GetValue(target);
                    value.Forbidden = (NetNodeExt.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                var bitMaskPanel0 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Required",
                    enumType: typeof(NetNodeExt.Flags),
                    setHandler: SetRequired,
                    getHandler: GetRequired,
                    false);
                var bitMaskPanel1 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Forbidden",
                    enumType: typeof(NetNodeExt.Flags),
                    setHandler: SetForbidden,
                    getHandler: GetForbidden,
                    true);
            } else if (fieldInfo.FieldType == typeof(NetInfoExt.VanillaSegmentInfoFlags)) {
                int GetRequired() {
                    var value = (NetInfoExt.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Required;
                }
                void SetRequired(int flags) {
                    var value = (NetInfoExt.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                    value.Required = (NetSegment.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                int GetForbidden() {
                    var value = (NetInfoExt.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                    return (int)value.Forbidden;
                };
                void SetForbidden(int flags) {
                    var value = (NetInfoExt.VanillaSegmentInfoFlags)fieldInfo.GetValue(target);
                    value.Forbidden = (NetSegment.Flags)flags;
                    fieldInfo.SetValue(target, value);
                };
                var bitMaskPanel0 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Required",
                    enumType: typeof(NetSegment.Flags),
                    setHandler: SetRequired,
                    getHandler: GetRequired,
                    false);
                var bitMaskPanel1 = BitMaskPanel.Add(
                    roadEditorPanel: instance,
                    container: container,
                    label: att.name + " Flags Forbidden",
                    enumType: typeof(NetSegment.Flags),
                    setHandler: SetForbidden,
                    getHandler: GetForbidden,
                    true);
            }

            //CreateExtendedComponentHelper(field, target,  instance, "Required");
            //CreateExtendedComponentHelper(field, target, instance, "Forbidden");
        }


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

