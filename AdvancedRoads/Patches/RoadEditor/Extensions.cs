using System;
using ColossalFramework.UI;
using HarmonyLib;
using KianCommons;
using System.Reflection;

namespace AdvancedRoads.Patches.RoadEditor {
    public static class Extensions {
        #region REPropertySet
        //private int REEnumBitmaskSet.GetFlags()
        static MethodInfo mGetFlags_ =
            AccessTools.DeclaredMethod(typeof(REEnumBitmaskSet), "GetFlags")
            ?? throw new Exception("mGetFlags_ is null");
        public static int GetFlags(this REEnumBitmaskSet instance)
            => (int)mGetFlags_.Invoke(instance, null);

        //protected override void REEnumBitmaskSet.Initialize(object target, FieldInfo targetField, string labelText)
        static MethodInfo mREEnumBitmaskSet_Initialize_ =
            AccessTools.DeclaredMethod(typeof(REEnumBitmaskSet), "Initialize")
            ?? throw new Exception("mREEnumBitmaskSet_Initialize_ is null");
        public static void Initialize(this REEnumBitmaskSet instance, object target, FieldInfo targetField, string labelText)
         => mREEnumBitmaskSet_Initialize_.Invoke(instance, new object[] { target, targetField, labelText});

        //private bool REEnumBitmaskSet.RequiresUserFlag(Type type)
        static MethodInfo mREEnumBitmaskSet_RequiresUserFlag_ =
            AccessTools.DeclaredMethod(typeof(REEnumBitmaskSet), "RequiresUserFlag")
            ?? throw new Exception("mREEnumBitmaskSet_RequiresUserFlag_ is null");
        public static bool RequiresUserFlag(this REEnumBitmaskSet instance, Type type)
         => (bool)mREEnumBitmaskSet_RequiresUserFlag_.Invoke(instance, new object[] { type});

        //private string REEnumBitmaskSet.GetUserFlagName(int flag)
        static MethodInfo mREEnumBitmaskSet_GetUserFlagName_ =
            AccessTools.DeclaredMethod(typeof(REEnumBitmaskSet), "GetUserFlagName")
            ?? throw new Exception("mREEnumBitmaskSet_GetUserFlagName_ is null");
        public static string GetUserFlagName(this REEnumBitmaskSet instance, int flag)
         => (string)mREEnumBitmaskSet_GetUserFlagName_.Invoke(instance, new object[] { flag });


        // protected FieldInfo REPropertySet.m_TargetField;
        public static FieldInfo fREPropertySet_TargetField =
            AccessTools.Field(typeof(REPropertySet), "m_TargetField")
            ?? throw new Exception("fREPropertySet_TargetField is null");
        public static FieldInfo m_TargetField(this REPropertySet instance)
            => (FieldInfo)fREPropertySet_TargetField.GetValue(instance);
        public static void set_TargetField(this REPropertySet instance, FieldInfo value) =>
            fREPropertySet_TargetField.SetValue(instance, value);

        // protected object REPropertySet.m_Target;
        public static FieldInfo fREPropertySet_Target =
            AccessTools.Field(typeof(REPropertySet), "m_Target")
            ?? throw new Exception("fREPropertySet_Target is null");
        public static object m_Target(this REPropertySet instance) => fREPropertySet_Target.GetValue(instance);
        public static void set_Target(this REPropertySet instance, object value)
            => fREPropertySet_Target.SetValue(instance, value);

        #endregion

        static FieldInfo fRoadEditorPanel_Target_ =
            AccessTools.Field(typeof(RoadEditorPanel), "m_Target")
            ?? throw new Exception("fRoadEditorPanel_Target_ is null");
        public static object m_Target(this RoadEditorPanel instance)
            => fRoadEditorPanel_Target_.GetValue(instance);

        static MethodInfo mCreateFieldComponent_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "CreateFieldComponent")
            ?? throw new Exception("mCreateFieldComponent_ is null");

        static MethodInfo mAddLanePropFields_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddLanePropFields")
            ?? throw new Exception("mAddLanePropFields_ is null");
        public static void AddLanePropFields(this RoadEditorPanel instance)
            => mAddLanePropFields_.Invoke(instance, null);

        static MethodInfo mAddLanePropSelectField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddLanePropSelectField")
            ?? throw new Exception("mAddLanePropSelectField_ is null");
        public static void AddLanePropSelectField(this RoadEditorPanel instance)
            => mAddLanePropSelectField_.Invoke(instance, null);

        static MethodInfo mAddCrossImportField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddCrossImportField")
            ?? throw new Exception("mAddCrossImportField_ is null");
        public static void AddCrossImportField(this RoadEditorPanel instance)
            => mAddCrossImportField_.Invoke(instance, null);

        static MethodInfo mAddModelImportField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddModelImportField")
            ?? throw new Exception("mAddModelImportField_ is null");
        public static void AddModelImportField(this RoadEditorPanel instance, bool showColorField = true)
            => mAddModelImportField_.Invoke(instance, new object[] { showColorField });

        //private void RoadEditorPanel.FitToContainer(UIComponent comp) 
        static MethodInfo mFitToContainer_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "FitToContainer")
            ?? throw new Exception("mFitToContainer_ is null");
        public static void FitToContainer(this RoadEditorPanel instance, UIComponent comp)
            => mFitToContainer_.Invoke(instance, new object[] { comp });

        // private void RoadEditorPanel.OnObjectModified()
        static MethodInfo mOnObjectModified_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "OnObjectModified")
            ?? throw new Exception("mOnObjectModified_ is null");
        public static void OnObjectModified(this RoadEditorPanel instance)
            => mOnObjectModified_.Invoke(instance, null);




    }
}
