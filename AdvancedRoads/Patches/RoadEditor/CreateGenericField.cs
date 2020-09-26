namespace AdvancedRoads.Patches.RoadEditor {
    using HarmonyLib;
    using System;
    using System.Reflection;
    using KianCommons;
    using AdvancedRoads.Manager;
    using ColossalFramework.UI;
    using System.Linq;

    /// <summary>
    /// extend CreateGenericField to support flag extension types.
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), "CreateGenericField")]
    public static class CreateGenericField {
        public static void Postfix(string groupName, FieldInfo field, object target, RoadEditorPanel __instance) {
            if(target is NetLaneProps.Prop) {
                Log.Debug($"{__instance.name}.CreateGenericField.Prefix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
                if (field.Name == nameof(NetLaneProps.Prop.m_flagsForbidden)) {
                    var fields = typeof(NetInfoExt.LaneProp).GetFields()
                        .Where(_field => _field.HasAttribute<CustomizablePropertyAttribute>());
                    foreach (var field2 in fields) {
                        CreateExtendedComponent(groupName, field2, target, __instance);
                    }
                }
            }
        }

        public static void CreateExtendedComponent(string groupName,FieldInfo field, object target, RoadEditorPanel instance) {
            if (!field.FieldType.HasAttribute<FlagPairAttribute>())
                return;
            Assertion.Assert(string.IsNullOrEmpty(groupName), "groupName is empty");
            CreateExtendedComponentHelper(field, target, instance, "Required");
            CreateExtendedComponentHelper(field, target, instance, "Forbidden");
        }

        public static void CreateExtendedComponentHelper(FieldInfo field, object target, RoadEditorPanel instance, string subFieldName) {
            Assertion.Assert(field.FieldType.HasAttribute<FlagPairAttribute>(), "HasAttribute:FlagsPair");
            Assertion.Assert(field.HasAttribute<CustomizablePropertyAttribute>(), "HasAttribute:CustomizablePropertyAttribute");
            CustomizablePropertyAttribute att =
                field.GetCustomAttributes(typeof(CustomizablePropertyAttribute), true)[0] as CustomizablePropertyAttribute;
            string label = att.name + "Flags" + subFieldName;
            UIComponent container = instance.m_Container;
            var subField = field.FieldType.GetField(subFieldName);

            //from: CreateFieldComponent()
            var repropertySet = UITemplateManager.Get<REPropertySet>(RoadEditorPanel.kEnumBitmaskSet);

            //from CreateGenericField:
            container.AttachUIComponent(repropertySet.gameObject);
            instance.FitToContainer(repropertySet.component);
            repropertySet.EventPropertyChanged += instance.OnObjectModified; //() => instance.OnObjectModified();

            // from: repropertySet.SetTarget(target, field);
            repropertySet.set_Target(target);
            repropertySet.set_TargetField(subField);
            (repropertySet as REEnumBitmaskSet).Initialize(target, subField, label);

        }
    }
}

