namespace AdvancedRoads.Patches.RoadEditor {
    using HarmonyLib;
    using System;
    using System.Reflection;
    using KianCommons;

    [HarmonyPatch]
    public static class AddCustomFields {
        static MethodInfo mAddCustomFields_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddCustomFields")
            ?? throw new Exception("mAddCustomFields_ is null");

        public static MethodBase TargetMethod() {
            var ret = mAddCustomFields_;
            Log.Info("aquired method: " + ret);
            return ret;
        }

        static MethodInfo mAddLanePropSelectField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddLanePropSelectField")
            ?? throw new Exception("mAddLanePropSelectField_ is null");

        static FieldInfo fTarget_ =
            AccessTools.Field(typeof(RoadEditorPanel), "m_Target")
            ?? throw new Exception("fTarget_ is null");



        public static void Postfix(RoadEditorPanel __instance) {
            object target = fTarget_.GetValue(__instance);
            Type type = target.GetType();
            if (type.FullName == typeof(PrefabIndeces.NetInfoExtension.Lane.Prop).FullName) {
                mAddLanePropSelectField_.Invoke(__instance, null);
            }
        }
    }
}

