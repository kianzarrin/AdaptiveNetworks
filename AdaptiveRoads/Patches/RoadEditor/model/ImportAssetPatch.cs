namespace AdaptiveRoads.Patches.RoadEditor.model {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Math;
    using KianCommons.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// RefreshAssetListpatch has files with relative paths.
    /// this patch fixes filename and path.
    /// </summary>
    [HarmonyPatch(typeof(ImportAsset), "Import")]
    public static class ImportAssetPatch {
        static void Prefix(ref string path, ref string filename) {
            try {
                var totalPath = Path.Combine(path, filename);
                path = Path.GetDirectoryName(totalPath);
                filename = Path.GetFileName(totalPath);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}
