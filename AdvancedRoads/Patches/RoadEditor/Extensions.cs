namespace AdvancedRoads.Patches.RoadEditor {
    using System;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System.Reflection;

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

        #endregion

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
