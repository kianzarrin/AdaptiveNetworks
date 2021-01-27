namespace AdaptiveRoads.Patches.RoadEditor.AssetImporterAssetImportPatches {
    using ColossalFramework.UI;
    using HarmonyLib;
    using JetBrains.Annotations;
    using System.Collections.Generic;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;

    // set default scale to 100
    [HarmonyPatch]
    public static class ImportScalePatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(AssetImporterAssetImport), "Awake");
            yield return GetMethod(typeof(AssetImporterAssetImport), "ResetTransformFields");
            yield return GetMethod(typeof(AssetImporterAssetImport), "SetDefaultScale");
        }
        [UsedImplicitly]
        static void Postfix(UITextField ___m_Scale) {
            if(AdaptiveRoads.UI.ModSettings.DefaultScale100) {
                if(___m_Scale.text == "1") // if CalculateDefaultScale() returned 1
                    ___m_Scale.text = "100";
            }
        }
    }
}