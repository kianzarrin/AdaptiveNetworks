namespace AdaptiveRoads.Patches.metadata {
    using AdaptiveRoads.Patches.RoadEditor;
    using HarmonyLib;
    using PrefabMetadata.API;
    using System;
    using AdaptiveRoads.Util;

    /// <summary>
    /// changeing types confuses AddCustomFields.
    /// this patch resolves that confusion by using the replaced types.
    /// TODO move this pacth to prefab indeces mod.
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel))]
    public static class AddCustomFields {
        [HarmonyPatch("AddCustomFields")]
        public static void Postfix(RoadEditorPanel __instance, object ___m_Target) {
            if (___m_Target == null) throw new ArgumentNullException("___m_Target");
            object target = ___m_Target;
            //Log.Debug($"AddCustomFields.PostFix() target={target}\n" + Environment.StackTrace);
            if (!(target is IInfoExtended))
                return;// handle extended target.
            if (target is NetInfo.Segment) {
                __instance.AddCrossImportField();
                __instance.AddModelImportField(true);
            } else if (target is NetInfo.Node) {
                __instance.AddCrossImportField();
                __instance.AddModelImportField(false);
            } else if (target is NetInfo.Lane) {
                __instance.AddLanePropFields();
            } else if (target is NetLaneProps.Prop) {
                __instance.AddLanePropSelectField();
            }
        }
    }
}

