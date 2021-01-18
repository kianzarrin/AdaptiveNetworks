namespace FbxUtil {
    using System.IO;
    using UnityEngine;
    using Fbx;
    using KianCommons;
    using System.Text;

    public static class FbxConverter {
        #region stream IO
        public static void ExportAsciiFbx(this Mesh mesh, Stream stream) {
            using(var sw = new StreamWriter(stream) )
                sw.Write(mesh.ToAsciiFBX());
        }
        public static void ExportBinaryFbx(this Mesh mesh, Stream stream) {
            var writer = new FbxBinaryWriter(stream);
            writer.Write(mesh.ToFBXDocument());
        }
        #endregion

        #region File IO
        public static void ExportAsciiFbx(this Mesh mesh, string path) =>
            File.WriteAllText(path, mesh.ToAsciiFBX());

        public static void ExportBinaryFbx(this Mesh mesh, string path) =>
            FbxIO.WriteBinary(mesh.ToFBXDocument(), path);
        
        #endregion

        #region conversion
        public static FbxDocument AsciiToBinary(string data) {
            using(var s = data.ToStream()) { 
                var reader = new FbxAsciiReader(s);
                return reader.Read();
            }
        }

        public static string ToAsciiFBX(this Mesh mesh) {
            GameObject go = new GameObject();
            go.name = mesh.name;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            string data = UnityFBXExporter.FBXExporter.MeshToString(go, "");
            GameObject.Destroy(go);
            return data;
        }

        public static FbxDocument ToFBXDocument(this Mesh mesh) {
            string data = mesh.ToAsciiFBX();
            return AsciiToBinary(data);
        }
        #endregion

        #region Stream Util

        static string ReadAllText(this Stream stream) {
            stream.Flush();
            stream.Position = 0;
            using (var r = new StreamReader(stream))
                return r.ReadToEnd();
        }

        public static Stream ToStream(this string value) => ToStream(value, Encoding.UTF8);
        public static Stream ToStream(this string value, Encoding encoding) =>
            new MemoryStream(encoding.GetBytes(value ?? string.Empty));

        #endregion

        static void Test(Mesh mesh) {
            // Read a file
            var documentNode = FbxIO.ReadBinary("MyModel.fbx");

            // Update a property
            documentNode["Creator"].Value = "My Application";

            using (var s = new MemoryStream()) {
                var w = new FbxAsciiWriter(s);
                w.Write(documentNode);
                Log.Debug(s.ReadAllText());
            }

            // Write the updated binary
            FbxIO.WriteBinary(documentNode, "MyModel_patched.fbx");
        }
    }
}

