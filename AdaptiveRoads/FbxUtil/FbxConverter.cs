namespace FbxUtil {
    using System.IO;
    using UnityEngine;
    public static class FbxConverter {
        public static void ExportFbx(this Mesh mesh, StreamWriter s) =>
            s.Write(mesh.ToAsciiFBX());
        
        public static string ToAsciiFBX(this Mesh mesh) {
            GameObject go = new GameObject();
            go.name = mesh.name;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            string data = UnityFBXExporter.FBXExporter.MeshToString(go, "");
            GameObject.Destroy(go);
            return data;
        }
    }
}

