namespace AdvancedRoads.Patches.RoadEditor {
    using HarmonyLib;
    using System;
    using System.Reflection;
    using KianCommons;

    /// <summary>
    /// changeing types confuses AddCustomFields.
    /// this patch resolves that confusion by using the replaced types.
    /// TODO move this pacth to prefab indeces mod.
    /// </summary>
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

        static MethodInfo mAddLanePropFields_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddLanePropFields")
            ?? throw new Exception("mAddLanePropFields_ is null");

        static MethodInfo mAddLanePropSelectField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddLanePropSelectField")
            ?? throw new Exception("mAddLanePropSelectField_ is null");

        static MethodInfo mAddCrossImportField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddCrossImportField")
            ?? throw new Exception("mAddCrossImportField_ is null");

        static MethodInfo mAddModelImportField_ =
            AccessTools.DeclaredMethod(typeof(RoadEditorPanel), "AddModelImportField")
            ?? throw new Exception("mAddModelImportField_ is null");

        static FieldInfo fTarget_ =
            AccessTools.Field(typeof(RoadEditorPanel), "m_Target")
            ?? throw new Exception("fTarget_ is null");

        public static void Postfix(RoadEditorPanel __instance) {
            object target = fTarget_.GetValue(__instance);
            Log.Debug($"AddCustomFields.PostFix() target={target}\n" + Environment.StackTrace);
            if (target is NetInfo.Segment) {
                mAddCrossImportField_.Invoke(__instance, null);
                mAddModelImportField_.Invoke(__instance, new object[] { true });
            } else if (target is NetInfo.Node) {
                mAddCrossImportField_.Invoke(__instance, null);
                mAddModelImportField_.Invoke(__instance, new object[]{false});
            } else if (target is NetInfo.Lane) {
                mAddLanePropFields_.Invoke(__instance, null);
            } else if (target is NetLaneProps.Prop) {
                mAddLanePropSelectField_.Invoke(__instance, null);
            }
        }
    }
}

