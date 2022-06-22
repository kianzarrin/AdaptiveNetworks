namespace AdaptiveRoads.Patches.RoadEditor.AssetImporterAssetImportPatches {
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Math;
    using KianCommons.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    // adds subfolders to node/segment models
    [HarmonyPatch(typeof(AssetImporterAssetImport), "RefreshAssetList")]
    [HarmonyPatch(new[] { typeof(string[]) })]
    public static class RefreshAssetListpatch {
        static void Postfix(string[] extensions,
            AssetImporterAssetImport __instance,
            UIListBox ___m_FileList,
            UITextureSprite ___m_SmallPreview) {
            try {
                if (ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo) {
                    // increase panel size:
                    __instance.component.width = 1115;
                    ___m_FileList.width = 550;
                    ___m_FileList.relativePosition =
                        ___m_FileList.relativePosition.SetI(0, 0);
                    ___m_SmallPreview.relativePosition =
                        ___m_SmallPreview.relativePosition.SetI(600, 0);
                }
                AddSubfolders(___m_FileList, extensions);
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        private static void AddSubfolders(UIListBox m_FileList, string[] extensions) {
            DirectoryInfo dir = new DirectoryInfo(AssetImporterAssetImport.assetImportPath);
            if(!dir.Exists) return;

            var fileInfos = new List<FileInfo>();
            foreach(var subdir in dir.GetDirectories()) {
                if(!subdir.Name.StartsWith("_"))
                    fileInfos.AddRange(AddSubfoldersRecursive(subdir, extensions));
            }

            // append names
            var names = fileInfos.Select(_f => _f.MakeRelative(dir));
            names = m_FileList.items.Concat(names);
            m_FileList.items = names.ToArray();
        }

        internal static string MakeRelative(this FileInfo file, DirectoryInfo dir) {
            return IOHelpers.RelativePath(dir.FullName, file.FullName);
        }



        private static List<FileInfo> AddSubfoldersRecursive(DirectoryInfo dir, string[] extensions) {
            var ret = new List<FileInfo>();
            FileInfo[] files = dir.GetFiles();
            if(files != null) {
                foreach(FileInfo fileInfo in files) {
                    string baseName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    string ext = Path.GetExtension(fileInfo.Name);
                    bool isLod = baseName.EndsWith(sLODModelSignature, StringComparison.OrdinalIgnoreCase);
                    if(!isLod) {
                        for(int j = 0; j < extensions.Length; j++) {
                            if(string.Compare(ext, extensions[j], ignoreCase: false) == 0) {
                                ret.Add(fileInfo);
                            }
                        }
                    }
                }
            }

            foreach(var subdir in dir.GetDirectories()) {
                if(!subdir.Name.StartsWith("_"))
                    ret.AddRange(AddSubfoldersRecursive(subdir, extensions));
            }

            return ret;
        }

        static string sLODModelSignature = "_lod";
    }
}