namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.UI;
    using HarmonyLib;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
    using System.Reflection;

    //  public static object AssetEditorRoadUtils.AddArrayElement(object target, FieldInfo field, object newObject = null)
    //	public static void AssetEditorRoadUtils.RemoveArrayElement(object element, object target, FieldInfo field)
    /// <summary>
    /// Extends the newly added element to hold metadata.
    /// this makes sure the UI component is linked to the extended element.
    /// </summary>
    [HarmonyPatch(typeof(AssetEditorRoadUtils))]
    public static class AddArrayElementPatch {
        [HarmonyPostfix]
        [HarmonyPatch("AddArrayElement")]
        static void AddArrayElement(object target, FieldInfo field, ref object __result) {
            try {
                if(!ModSettings.ARMode)
                    return;
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
            } catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
        }
    }
}
