namespace AdaptiveRoads.Patches.RoadEditor.model {
    using ColossalFramework;
    using ColossalFramework.UI;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;

    // remember import scale
    [HarmonyPatch]
    static class ImportScalePatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(AssetImporterAssetImport), "Awake");
            yield return GetMethod(typeof(AssetImporterAssetImport), "ResetTransformFields");
            yield return GetMethod(typeof(AssetImporterAssetImport), "SetDefaultScale");
        }

        [UsedImplicitly]
        static void Prefix(UITextField ___m_Scale) {
            if (___m_Scale != null) {
                ___m_Scale.eventTextChanged -= ScaleTextChanged;
            }
        }

        [UsedImplicitly]
        static void Postfix(UITextField ___m_Scale) {
            try {
                Log.Called();
                Log.Debug(Environment.StackTrace);
                ___m_Scale.eventTextChanged -= ScaleTextChanged;
                if (ImportScale.value != 0f) {
                    Log.Debug("saved import scale is " + ImportScale.value);
                    ___m_Scale.text = ImportScale.value.ToString();
                }

                ___m_Scale.eventTextChanged += ScaleTextChanged;
            } catch (Exception ex) {
                ex.Log();
            }
        }

        private static SavedFloat ImportScale = new SavedFloat("Import Scale", AdaptiveRoads.UI.ModSettings.FILE_NAME, def: 0, autoUpdate:true);
        private static void ScaleTextChanged(UIComponent component, string value) {
            try {
                if (float.TryParse(value, out float scale)) {
                    ImportScale.value = scale;
                    Log.Debug("ImportScale set to " + ImportScale.value);
                }
            }
            catch(Exception ex) { ex.Log(); }
        }
    }
}