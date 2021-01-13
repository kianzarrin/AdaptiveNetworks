namespace AdaptiveRoads.Patches.RoadEditor.AssetImporterAssetImportpatches {
    using ColossalFramework.UI;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;
    using System.Linq;
    using UnityEngine;
    using KianCommons.Math;
    using System.Text;

    // set default scale to 100
    [HarmonyPatch]
    public static class ScalePatch {
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

    // adds subfolders to node/segment models
    [HarmonyPatch(typeof(AssetImporterAssetImport), "RefreshAssetList")]
    [HarmonyPatch(new[] { typeof(string[]) })]
    public static class RefreshAssetListpatch {
        static void Postfix(string[] extensions,
            AssetImporterAssetImport __instance,
            UIListBox ___m_FileList,
            UITextureSprite ___m_SmallPreview

            ) {
            try {
                // panel width=1114
                // Preview pos.x=600
                // m_FileList : width=550
                __instance.component.width= 1115;

                ___m_FileList.width = 550;
                ___m_FileList.relativePosition =
                    ___m_FileList.relativePosition.SetI(0,0);

                ___m_SmallPreview.relativePosition =
                    ___m_SmallPreview.relativePosition.SetI(600, 0);

                AddSubfolders(___m_FileList, extensions);
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        private static void AddSubfolders(UIListBox m_FileList, string[] extensions) {
            DirectoryInfo dir = new DirectoryInfo(AssetImporterAssetImport.assetImportPath);
            if (!dir.Exists) return;

            var fileInfos = new List<FileInfo>();
            foreach (var subdir in dir.GetDirectories()) {
                if (!subdir.Name.StartsWith("_"))
                    fileInfos.AddRange(AddSubfoldersRecursive(subdir, extensions));
            }

            // append names
            var names = fileInfos.Select(_f => _f.MakeRelative(dir));
            names = m_FileList.items.Concat(names);
            m_FileList.items = names.ToArray();
        }

        internal static string MakeRelative(this FileInfo file, DirectoryInfo dir) {
            return RelativePath(dir.FullName, file.FullName);
        }

        public static string RelativePath(string absPath, string relTo) {
            string[] absDirs = absPath.Split('\\');
            string[] relDirs = relTo.Split('\\');

            // Get the shortest of the two paths
            int len = absDirs.Length < relDirs.Length ? absDirs.Length :
            relDirs.Length;

            // Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            // Find common root
            for (index = 0; index < len; index++) {
                if (absDirs[index] == relDirs[index]) lastCommonRoot = index;
                else break;
            }

            // If we didn't find a common prefix then throw
            if (lastCommonRoot == -1) {
                throw new ArgumentException("Paths do not have a common base");
            }

            // Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            // Add on the ..
            for (index = lastCommonRoot + 1; index < absDirs.Length; index++) {
                if (absDirs[index].Length > 0) relativePath.Append("..\\");
            }

            // Add on the folders
            for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++) {
                relativePath.Append(relDirs[index] + "\\");
            }
            relativePath.Append(relDirs[relDirs.Length - 1]);

            return relativePath.ToString();
        }

        private static List<FileInfo> AddSubfoldersRecursive(DirectoryInfo dir, string[] extensions) {
            var ret = new List<FileInfo>();
            FileInfo[] files = dir.GetFiles();
            if (files != null) {
                foreach (FileInfo fileInfo in files) {
                    if (!Path.GetFileNameWithoutExtension(fileInfo.Name).EndsWith(sLODModelSignature, StringComparison.OrdinalIgnoreCase)) {
                        for (int j = 0; j < extensions.Length; j++) {
                            if (string.Compare(Path.GetExtension(fileInfo.Name), extensions[j]) == 0) {
                                ret.Add(fileInfo);
                            }
                        }
                    }
                }
            }

            foreach (var subdir in dir.GetDirectories()) {
                if (!subdir.Name.StartsWith("_"))
                    ret.AddRange(AddSubfoldersRecursive(subdir, extensions));
            }

            return ret;
        }

        static string sLODModelSignature = "_lod";
    }
}