namespace AdaptiveRoads.Patches.RoadEditor {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Reflection;

    public static class REPropertySetExtensions {
        public static MethodInfo GetMethod(string methodName) =>
            ReflectionHelpers.GetMethod(typeof(REEnumBitmaskSet), methodName);

        //private int REEnumBitmaskSet.GetFlags()
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

    internal static class RoadEditorDynamicPropertyToggleHelpers {
        internal static Type ToggleType =
            Type.GetType("RoadEditorDynamicPropertyToggle, Assembly-CSharp", throwOnError: true);
        static T GetFieldValue<T>(string name, UICustomControl toggle) {
            Assertion.Assert(toggle.GetType() == ToggleType);
            return (T)ReflectionHelpers.GetFieldValue(name, toggle);
        }

        internal static UIButton GetToggleSelectButton(UICustomControl toggle) =>
            GetFieldValue<UIButton>("m_SelectButton", toggle);

        // gets the element in array the toggle represents.
        internal static object GetToggleTargetElement(UICustomControl toggle) =>
            GetFieldValue<object>("m_TargetElement", toggle);

        /// <summary>
        /// gets the object that contains the array (m_field)
        /// </summary>
        internal static object GetToggleTargetObject(UICustomControl toggle) =>
            GetFieldValue<object>("m_TargetObject", toggle);


    }
}
