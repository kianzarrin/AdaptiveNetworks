namespace AdaptiveRoads.Util {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;
    using UnityEngine;

    internal static class REPropertySetExtensions {
        public static object GetTarget(this REPropertySet instance) =>
            ReflectionHelpers.GetFieldValue("m_Target", instance);

        public static FieldInfo GetTargetField(this REPropertySet instance) =>
            ReflectionHelpers.GetFieldValue("m_TargetField", instance) as FieldInfo;
    }

    internal static class REEnumBitmaskSetExtensions {
        public static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(typeof(REEnumBitmaskSet), methodName);

        public static int GetFlags(this REEnumBitmaskSet instance) =>
        (int)GetMethod("GetFlags").Invoke(instance, null);

        public static void Initialize(this REEnumBitmaskSet instance, object target, FieldInfo targetField, string labelText) =>
            GetMethod("Initialize")
            .Invoke(instance, new object[] { target, targetField, labelText });


        public static bool RequiresUserFlag(this REEnumBitmaskSet instance, Type type) =>
            (bool)GetMethod("RequiresUserFlag").Invoke(instance, new object[] { type });

        //private string REEnumBitmaskSet.GetUserFlagName(int flag)
        public static string GetUserFlagName(this REEnumBitmaskSet instance, int flag) =>
            (string)GetMethod("GetUserFlagName").Invoke(instance, new object[] { flag });
    }

    public static class RoadEditorPanelExtensions {
        public static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(typeof(RoadEditorPanel), methodName);

        public static void CreateGenericField(this RoadEditorPanel instance,
            string groupName, FieldInfo field, object target) {
            GetMethod("CreateGenericField").Invoke(instance, new object[] { groupName, field, target });
        }

        public static void AddLanePropFields(this RoadEditorPanel instance) =>
            GetMethod("AddLanePropFields").Invoke(instance, null);

        public static void AddLanePropSelectField(this RoadEditorPanel instance) =>
             GetMethod("AddLanePropSelectField").Invoke(instance, null);

        public static void AddCrossImportField(this RoadEditorPanel instance) =>
            GetMethod("AddCrossImportField").Invoke(instance, null);

        public static void AddModelImportField(this RoadEditorPanel instance, bool showColorField = true) =>
            GetMethod("AddModelImportField")
            .Invoke(instance, new object[] { showColorField });

        //private void RoadEditorPanel.FitToContainer(UIComponent comp) 
        public static void FitToContainer(this RoadEditorPanel instance, UIComponent comp) =>
            GetMethod("FitToContainer")
            .Invoke(instance, new object[] { comp });

        public static void OnObjectModified(this RoadEditorPanel instance) =>
            GetMethod("OnObjectModified").Invoke(instance, null);

        public static void AddToArrayField(this RoadEditorPanel instance,
            RoadEditorCollapsiblePanel panel, object element, FieldInfo field, object targetObject) {
            // private void AddToArrayField(RoadEditorCollapsiblePanel panel,
            //      object element, FieldInfo field, object targetObject)
            GetMethod("AddToArrayField")
                .Invoke(instance, new[] { panel, element, field, targetObject });
        }

        public static bool RequiresUserFlag(Type type) {
            return type == typeof(Building.Flags) || type == typeof(Vehicle.Flags);
        }

        static FastInvokeHandler mGetGroupPanel_ =
            MethodInvoker.GetHandler(GetMethod("GetGroupPanel"));
        public static RoadEditorCollapsiblePanel GetGroupPanel(this RoadEditorPanel instance, string name) {
            return (RoadEditorCollapsiblePanel)mGetGroupPanel_.Invoke(instance, new[] { name });
        }

        public static void DestroySidePanel(this RoadEditorPanel instance) =>
            GetMethod("DestroySidePanel").Invoke(instance, null);

        public static RoadEditorPanel GetSidePanel(this RoadEditorPanel instance) =>
            ReflectionHelpers.GetFieldValue("m_SidePanel", instance) as RoadEditorPanel;

        public static object GetTarget(this RoadEditorPanel instance) =>
            ReflectionHelpers.GetFieldValue("m_Target", instance);
        public static void Reset(this RoadEditorPanel instance) =>
            instance.Initialize(instance.GetTarget());

    }

    internal static class RoadEditorCollapsiblePanelExtensions {
        public static RoadEditorAddButton GetAddButton(
            this RoadEditorCollapsiblePanel instance) {
            return instance.component.GetComponentInChildren<RoadEditorAddButton>();
        }
        public static FieldInfo GetField(
                this RoadEditorCollapsiblePanel instance) =>
                instance.GetAddButton()?.field;

        public static object GetTarget(this RoadEditorCollapsiblePanel instance) =>
            instance.GetAddButton()?.target;

        public static Array GetArray(this RoadEditorCollapsiblePanel instance) =>
            instance.GetField()?.GetValue(instance.GetTarget()) as Array;

        public static void SetArray(this RoadEditorCollapsiblePanel instance, Array value) =>
            instance.GetField().SetValue(instance.GetTarget(), value);
    }

    internal static class DPTHelpers {
        internal static Type DPTType =
            Type.GetType("RoadEditorDynamicPropertyToggle, Assembly-CSharp", throwOnError: true);
        static T GetFieldValue<T>(string name, UICustomControl dpt) {
            Assertion.Assert(dpt.GetType() == DPTType);
            return (T)ReflectionHelpers.GetFieldValue(name, dpt);
        }
        static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(DPTType, methodName);

        internal static UICustomControl GetDPTInParent(Component c) =>
            c.GetComponentInParent(DPTType) as UICustomControl;

        internal static UICustomControl GetDPTInChildren(Component c) =>
            c.GetComponentInChildren(DPTType) as UICustomControl;


        internal static UIButton GetDPTSelectButton(UICustomControl dpt) =>
            GetFieldValue<UIButton>("m_SelectButton", dpt);

        // gets the element in array the DPT represents.
        internal static object GetDPTTargetElement(UICustomControl dpt) =>
            GetFieldValue<object>("m_TargetElement", dpt);

        /// <summary>
        /// gets the object that contains the array (m_field)
        /// </summary>
        internal static object GetDPTTargetObject(UICustomControl dpt) =>
            GetFieldValue<object>("m_TargetObject", dpt);

        internal static FieldInfo GetDPTField(UICustomControl dpt) =>
            GetFieldValue<object>("m_Field", dpt) as FieldInfo;

        internal static void ToggleDPTColor(UICustomControl dpt, bool selected) =>
            GetMethod("ToggleColor").Invoke(dpt, new object[] { selected });
    }

    internal static class RERefSetExtensions {
        static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(typeof(RERefSet), methodName);

        internal static void OnReferenceSelected(this RERefSet instance, PrefabInfo info) =>
            GetMethod("OnReferenceSelected")
            .Invoke(instance, new object[] { info });
    }
}
