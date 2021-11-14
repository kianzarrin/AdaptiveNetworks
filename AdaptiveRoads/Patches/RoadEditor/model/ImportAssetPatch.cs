namespace AdaptiveRoads.Patches.RoadEditor.model {
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.IO;

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
