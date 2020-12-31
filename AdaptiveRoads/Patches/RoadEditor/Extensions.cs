namespace AdaptiveRoads.Patches.RoadEditor {
    using System;
    using System.Reflection;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;

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
    }
}
