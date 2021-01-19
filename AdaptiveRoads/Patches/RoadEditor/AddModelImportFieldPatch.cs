namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.UI.RoadEditor;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using System.IO;
    using UnityEngine;
    using ObjUnity3D;
    using HarmonyLib;
    using FbxUtil;

    /// <summary>
    /// add hints
    /// </summary>
    [HarmonyPatch(typeof(RoadEditorPanel), "AddModelImportField")]
    public static class AddModelImportFieldPatch {

        public static void Postfix(
            object ___m_Target,
            UIScrollablePanel ___m_Container) {
            var button = ___m_Container.AddUIComponent<EditorButon>();
            button.text = "Dump mesh as fbx";
            button.eventClick += (_, __) => Dump2(___m_Target);
        }

        static void Dump2(object target) {
            try {
                if (target is NetInfo.Node node) {
                    node.m_nodeMesh.DumpFpx("node");
                }
                if (target is NetInfo.Segment segment) {
                    segment.m_segmentMesh.DumpFpx("segment");
                }
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void DumpObj(this Mesh mesh, string name) {
            string path = GetFilePath(mesh.name, ".obj");
            using (var fs = new FileStream(path, FileMode.Create))
                OBJLoader.ExportOBJ(mesh.EncodeOBJ(), fs);
        }

        public static void DumpFpx(this Mesh mesh, string name) {
            string path = GetFilePath("", mesh.name, ".fbx");
            mesh.ExportBinaryFbx2(path);
        }

        //    static void Dump(object target) {
        //    try {
        //        Mesh mesh = null;
        //        Texture2D main = null;
        //        Texture2D apr = null;
        //        Texture2D xys = null;

        //        if (target is NetInfo.Node node) {
        //            Material material = node.m_nodeMaterial;
        //            main = material.GetTexture(ID_Defuse) as Texture2D;
        //            apr = material.GetTexture(ID_APRMap) as Texture2D;
        //            xys = material.GetTexture(ID_XYSMap) as Texture2D;
        //            mesh = node.m_mesh;
        //        }
        //        if (target is NetInfo.Segment segment) {
        //            Material material = segment.m_segmentMaterial;
        //            main = material.GetTexture(ID_Defuse) as Texture2D;
        //            apr = material.GetTexture(ID_APRMap) as Texture2D;
        //            xys = material.GetTexture(ID_XYSMap) as Texture2D;
        //            mesh = segment.m_mesh;
        //            segment.ExportToFBX(GetFilePath(mesh.name, ".fbx"));
        //        }
        //        string name = mesh.name;
        //        main?.Dump(GetFilePath(name, "_D.png"));
        //        apr?.Dump(GetFilePath(name, "_APR.png"));
        //        xys?.Dump(GetFilePath(name, "_XYS.png"));
        //        File.WriteAllBytes(GetFilePath(name, ".obj"), mesh.EncodeBinary());
        //    } catch (Exception ex) {
        //        Log.Exception(ex);
        //    }
        //}

        internal static int ID_Defuse => NetManager.instance.ID_MainTex;
        internal static int ID_APRMap => NetManager.instance.ID_APRMap;
        internal static int ID_XYSMap => NetManager.instance.ID_XYSMap;
        internal static string getTexName(int id) {
            if (id == ID_Defuse) return "_MainTex";
            if (id == ID_APRMap) return "_APRMap";
            if (id == ID_XYSMap) return "_XYSMap";
            throw new Exception("Bad Texture ID");
        }
        internal static int[] texIDs => new int[] { ID_Defuse, ID_APRMap, ID_XYSMap };
        public static Texture2D TryMakeReadable(this Texture2D texture) {
            if (texture.IsReadable())
                return texture;
            else
                return texture.MakeReadable();
        }
        public static bool IsReadable(this Texture2D texture) {
            try {
                texture.GetPixel(0, 0);
                return true;
            } catch {
                return false;
            }
        }

        public static void Dump(this Texture tex, string path) {
            if (tex == null) throw new ArgumentNullException("tex");
            Texture2D texture = tex is Texture2D ? tex as Texture2D : throw new Exception($"texture:{tex} is not texture2D");
            Log.Info($"Dumping texture:<{tex.name}> size:<{tex.width}x{tex.height}>");
            texture = texture.TryMakeReadable();

            byte[] bytes = texture.EncodeToPNG();
            if (bytes == null) {
                Log.Info($"Warning! bytes == null. Failed to dump {tex?.name} with format  {(tex as Texture2D).format} to {path}.");
                return;
            }
            Log.Info("Dumping to " + path);
            File.WriteAllBytes(path, bytes);
        }

        static string GetFilePath(string subdir, string name, string ext) {
            if (ext[0] == '.') ext = ext.Substring(1);
            string filename = name + "." + ext;
            filename = filename.RemoveChars(Path.GetInvalidFileNameChars());
            string dir = subdir.RemoveChars(Path.GetInvalidPathChars());

            string path = Path.Combine(AssetImporterAssetImport.assetImportPath, "ARDumps");
            if(dir != "")
                path = Path.Combine(path, dir);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, filename);
            return path;

        }

        static string GetFilePath(string name, string ext) {
            if (ext[0] == '.') ext = ext.Substring(1);
            string filename = name + "." + ext;
            filename = filename.RemoveChars(Path.GetInvalidFileNameChars());
            string dir = name.RemoveChars(Path.GetInvalidPathChars());

            string path = Path.Combine(AssetImporterAssetImport.assetImportPath, "ARDumps");
            path = Path.Combine(path, dir);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, filename);
            return path;
        }
    }
}

