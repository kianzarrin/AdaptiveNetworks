namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
    using System.Reflection;
    using static KianCommons.EnumerationExtensions;

    // AssetEditorRoadUtils
    //  public static object AddArrayElement(object target, FieldInfo field, object newObject = null)
    //	public static void RemoveArrayElement(object element, object target, FieldInfo field)
    [HarmonyPatch(typeof(AssetEditorRoadUtils))]
    public static class AssetEditorRoadUtilsPatch {
        [HarmonyPostfix]
        [HarmonyPatch("AddArrayElement")]
        static void AddArrayElement(object target, FieldInfo field, ref object __result) {
            try {
                if (!field.FieldType.IsArray)
                    return;
                var newVal = PrefabMetadataHelpers.Extend(__result);
                if (newVal == null)
                    return;

                Array array = (Array)field.GetValue(target);
                int index = array.Length - 1;
                array.SetValue(newVal, index);
                __result = newVal;

                Log.Debug($"AddArrayElement -> return={__result}");
            }
            catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
        }
    }
}
