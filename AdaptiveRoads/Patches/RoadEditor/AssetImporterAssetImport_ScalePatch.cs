namespace AdaptiveRoads.Patches.RoadEditor.x {
    using ColossalFramework.UI;
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;
    using JetBrains.Annotations;

    // set default scale to 100
    [HarmonyPatch]
    public static class AssetImporterAssetImport_ScalePatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(AssetImporterAssetImport), "Awake");
            yield return GetMethod(typeof(AssetImporterAssetImport), "ResetTransformFields");
            yield return GetMethod(typeof(AssetImporterAssetImport), "SetDefaultScale");
        }
        [UsedImplicitly]
        static void Postfix(UITextField ___m_Scale) {
            if (___m_Scale.text == "1") // if CalculateDefaultScale() returned 1
                ___m_Scale.text = "100";
        }
    }
}
