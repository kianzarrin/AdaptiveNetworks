using System.IO;
using UnityEngine;
using FE = UnityFBXExporter.FBXExporter;

namespace UnityFBXExporter {
    public static class FbxConverter {
        public static void ExportFbx(this Mesh mesh, StreamWriter s) {
            GameObject go = new GameObject();
            go.name = mesh.name;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            string data = FE.MeshToString(go, "dummypath");
            s.Write(data);
            GameObject.Destroy(go);
        }
    }
}

