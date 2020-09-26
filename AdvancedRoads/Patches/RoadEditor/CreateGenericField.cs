namespace AdvancedRoads.Patches.RoadEditor {
    using HarmonyLib;
    using System;
    using System.Reflection;
    using KianCommons;
    using AdvancedRoads.Manager;
    using ColossalFramework.UI;

    /// <summary>
    /// extend CreateGenericField to support flag extension types.
    /// </summary>
    [HarmonyPatch]
    public static class CreateGenericField {

        // private void RoadEditorPanel.CreateGenericField(string groupName, FieldInfo field, object target)
        static MethodInfo mCreateGenericField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "CreateGenericField")
            ?? throw new Exception("mCreateGenericField_ is null");

        public static MethodBase TargetMethod() {
            var ret = mCreateGenericField_;
            Log.Info("aquired method: " + ret);
            return ret;
        }

        // REPropertySet:
        // protected object m_Target;
        // protected FieldInfo m_TargetField;
        static FieldInfo fREPropertySet_Target_ =
            AccessTools.Field(typeof(REPropertySet), "m_Target")
            ?? throw new Exception("fREPropertySet_Target_ is null");
        static FieldInfo fREPropertySet_TargetField_ =
            AccessTools.Field(typeof(REPropertySet), "m_TargetField")
            ?? throw new Exception("fREPropertySet_TargetField_ is null");


        static MethodInfo mCreateFieldComponent_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "CreateFieldComponent")
            ?? throw new Exception("mCreateFieldComponent_ is null");

        //protected override void REEnumBitmaskSet.Initialize(object target, FieldInfo targetField, string labelText)
        static MethodInfo mREEnumBitmaskSet_Initialize_ =
            AccessTools.DeclaredMethod(typeof(REEnumBitmaskSet), "Initialize")
            ?? throw new Exception("mREEnumBitmaskSet_Initialize_ is null");

        //private void RoadEditorPanel.FitToContainer(UIComponent comp) 
        static MethodInfo mFitToContainer_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "FitToContainer")
            ?? throw new Exception("mFitToContainer_ is null");

        // private void RoadEditorPanel.OnObjectModified()
        static MethodInfo mOnObjectModified_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "OnObjectModified")
            ?? throw new Exception("mOnObjectModified_ is null");


        public static bool Prefix(string groupName, FieldInfo field, object target, RoadEditorPanel __instance) {
            Log.Debug($"{__instance.name}.CreateGenericField.Prefix({groupName},{field},{target})\n"/* + Environment.StackTrace*/);
            if (!field.FieldType.HasAttribute<FlagPairAttribute>())
                return true;
            Assertion.Assert(string.IsNullOrEmpty(groupName), "groupName is empty");
            CreateComponentHelper(field, target, __instance, "Required");
            CreateComponentHelper(field, target, __instance, "Forbidden");

            return false;
        }

        public static void CreateComponentHelper(FieldInfo field, object target, RoadEditorPanel __instance, string subFieldName) {
            if (!field.HasAttribute<CustomizablePropertyAttribute>())
                return;
            CustomizablePropertyAttribute att =
                field.GetCustomAttributes(typeof(CustomizablePropertyAttribute), true)[0] as CustomizablePropertyAttribute;
            string label = att.name + "Flags" + subFieldName;
            UIComponent container = __instance.m_Container;
            var subField = field.FieldType.GetField(subFieldName);

            //from: CreateFieldComponent()
            var repropertySet = UITemplateManager.Get<REPropertySet>(RoadEditorPanel.kEnumBitmaskSet);

            //from CreateGenericField:
            container.AttachUIComponent(repropertySet.gameObject);
            mFitToContainer_.Invoke(__instance, new[] { repropertySet.component });
            repropertySet.EventPropertyChanged += () => mOnObjectModified_.Invoke(__instance, null);

            // from: repropertySet.SetTarget(target, field);
            fREPropertySet_Target_.SetValue(repropertySet, target);
            fREPropertySet_TargetField_.SetValue(repropertySet, subField);
            mREEnumBitmaskSet_Initialize_.Invoke(repropertySet, new[] { target, subField, label });

        }

        //public static void GetRequiredForbidden(FieldInfo field, REPropertySet required, REPropertySet forbidden) {
        //    var fRequired = field.FieldType.GetField("Required");
        //    var fForbidden = field.FieldType.GetField("Forbidden");
        //}

        //public static REPropertySet CreateAGenericField(FieldInfo field) {

        //}


    }
}

