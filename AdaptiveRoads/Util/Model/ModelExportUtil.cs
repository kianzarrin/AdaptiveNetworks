namespace AdaptiveRoads.Util.Model {
    using FbxUtil;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class ModelExportUtil {
        public static string [] ModelTextures = new string[] { "_MainTex", "_APRMap", "_XYSMap" };
        public static string LegalizeFileName(string illegal) {
            if (string.IsNullOrEmpty(illegal)) {
                return DateTime.Now.ToString("yyyyMMddhhmmss");
            }

            var regexSearch = new string(Path.GetInvalidFileNameChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(illegal, "_");
        }

        #region mesh
        public static Mesh GetReadable(this Mesh mesh) {
            if (!mesh.isReadable) {
                Log.Warning($"Mesh \"{mesh.name}\" is marked as non-readable, running workaround..");

                try {
                    // copy the relevant data to the temporary mesh
                    var mesh2 = new Mesh {
                        vertices = mesh.vertices,
                        colors = mesh.colors,
                        triangles = mesh.triangles,
                        normals = mesh.normals,
                        tangents = mesh.tangents,
                    };
                    mesh2.RecalculateBounds();
                    mesh2.name = mesh.name;
                    return mesh2;
                } catch (Exception ex) {
                    Log.Error($"Workaround failed with error - {ex.Message}");
                    return null;
                }
            }
            return mesh;
        }

        public static void DumpToFBX(this Mesh mesh, string path) {
            try {
                mesh = mesh.GetReadable();
                using var stream = new FileStream(path, FileMode.Create);
                mesh.ExportAsciiFbx(stream);
                Log.Info($"Dumped mesh \"{mesh.name}\" to \"{path}\"");
            } catch (Exception ex) {
                Log.Error($"There was an error while trying to dump mesh \"{mesh.name}\" - {ex.Message}");
            }
        }
        #endregion

        #region Texture
        public static void DumpTexture2D(this Texture2D texture, string path) {
            byte[] bytes;
            try {
                bytes = texture.EncodeToPNG();
            } catch {
                try {
                    bytes = texture.MakeReadable().EncodeToPNG();
                } catch (Exception ex) {
                    ex.Log(false);
                    return;
                }
            }

            File.WriteAllBytes(path, bytes);
           Log.Debug($"Texture dumped to \"{path}\"");
        }
        public static void DumpToPNG(Texture previewTexture, string path) {
            previewTexture?.ToTexture2D()?.DumpTexture2D(path);
        }

        public static Color32[] BuildBlankTextureColors(int length) {
            var result = new Color32[length];
            for (var i = 0; i < length; i++) {
                result[i].r = byte.MaxValue;
                result[i].g = byte.MaxValue;
                result[i].b = byte.MaxValue;
                result[i].a = byte.MaxValue;
            }

            return result;
        }

        public static void Dump(this Material material, string dir, string modelName, bool lod, bool extract = true) {
            string assetName = Path.Combine(dir, modelName);
            if (lod) assetName += "_lod";
            DumpMainTex(assetName, material.GetTexture("_MainTex") as Texture2D, extract);
            DumpACI(assetName, material.GetTexture("_ACIMap") as Texture2D, extract);
            DumpXYS(assetName, material.GetTexture("_XYSMap") as Texture2D, extract);
            DumpXYCA(assetName, material.GetTexture("_XYCAMap") as Texture2D, extract);
            DumpAPR(assetName, material.GetTexture("_APRMap") as Texture2D, extract);
        }

        private static void DumpMainTex(string assetName, Texture2D mainTex, bool extract = true) {
            if (mainTex == null) {
                return;
            }

            if (extract) {
                var length = mainTex.width * mainTex.height;
                var r = BuildBlankTextureColors(length);
                mainTex.ExtractChannels(r, r, r, null, false, false, false, false, false, false, false);
                DumpTexture2D(r.ColorsToTexture2D(mainTex.width, mainTex.height), $"{assetName}_d.png");
            } else {
                DumpTexture2D(mainTex, $"{assetName}_MainTex.png");
            }
        }

        private static void DumpACI(string assetName, Texture2D aciMap, bool extract = true) {
            if (aciMap == null) {
                return;
            }

            if (extract) {
                var length = aciMap.width * aciMap.height;
                var r = BuildBlankTextureColors(length);
                var g = BuildBlankTextureColors(length);
                var b = BuildBlankTextureColors(length);
                aciMap.ExtractChannels(r, g, b, null, true, true, true, true, true, false, false);
                DumpTexture2D(r.ColorsToTexture2D(aciMap.width, aciMap.height), $"{assetName}_a.png");
                DumpTexture2D(g.ColorsToTexture2D(aciMap.width, aciMap.height), $"{assetName}_c.png");
                DumpTexture2D(b.ColorsToTexture2D(aciMap.width, aciMap.height), $"{assetName}_i.png");
            } else {
                DumpTexture2D(aciMap, $"{assetName}_ACI.png");
            }
        }

        private static void DumpXYS(string assetName, Texture2D xysMap, bool extract = true) {
            if (xysMap == null) {
                return;
            }

            if (extract) {
                var length = xysMap.width * xysMap.height;
                var r1 = BuildBlankTextureColors(length);
                var b1 = BuildBlankTextureColors(length);
                xysMap.ExtractChannels(r1, r1, b1, null, false, false, true, false, false, true, false);
                DumpTexture2D(r1.ColorsToTexture2D(xysMap.width, xysMap.height), $"{assetName}_n.png");
                DumpTexture2D(b1.ColorsToTexture2D(xysMap.width, xysMap.height), $"{assetName}_s.png");
            } else {
                DumpTexture2D(xysMap, $"{assetName}_XYS.png");
            }
        }

        private static void DumpXYCA(string assetName, Texture2D xycaMap, bool extract = true) {
            if (xycaMap == null) {
                return;
            }

            if (extract) {
                var length = xycaMap.width * xycaMap.height;
                var r1 = BuildBlankTextureColors(length);
                var b1 = BuildBlankTextureColors(length);
                var a1 = BuildBlankTextureColors(length);
                xycaMap.ExtractChannels(r1, r1, b1, a1, false, false, true, false, true, true, true);
                DumpTexture2D(r1.ColorsToTexture2D(xycaMap.width, xycaMap.height), $"{assetName}_n.png");
                DumpTexture2D(b1.ColorsToTexture2D(xycaMap.width, xycaMap.height), $"{assetName}_c.png");
                DumpTexture2D(a1.ColorsToTexture2D(xycaMap.width, xycaMap.height), $"{assetName}_a.png");
            } else {
                DumpTexture2D(xycaMap, $"{assetName}_XYCA.png");
            }
        }

        private static void DumpAPR(string assetName, Texture2D aprMap, bool extract = true) {
            if (aprMap == null) {
                return;
            }

            if (extract) {
                var length = aprMap.width * aprMap.height;
                var a1 = BuildBlankTextureColors(length);
                var p1 = BuildBlankTextureColors(length);
                var r1 = BuildBlankTextureColors(length);
                aprMap.ExtractChannels(a1, p1, r1, null, true, true, true, true, true, false, false);
                DumpTexture2D(a1.ColorsToTexture2D(aprMap.width, aprMap.height), $"{assetName}_a.png");
                DumpTexture2D(p1.ColorsToTexture2D(aprMap.width, aprMap.height), $"{assetName}_p.png");
                DumpTexture2D(r1.ColorsToTexture2D(aprMap.width, aprMap.height), $"{assetName}_r.png");
            } else {
                DumpTexture2D(aprMap, $"{assetName}_APR.png");
            }
        }


        #endregion
    }
}
