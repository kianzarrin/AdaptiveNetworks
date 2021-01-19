namespace FbxUtil {
    using System.IO;
    using UnityEngine;
    using Fbx;
    using KianCommons;
    using System.Text;
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public static class FbxConverter {
        #region stream IO
        public static void ExportAsciiFbx(this Mesh mesh, Stream stream) {
            using(var sw = new StreamWriter(stream) )
                sw.Write(mesh.ToAsciiFBX());
        }

        [Obsolete("does not read properly", error: true)]
        public static void ExportBinaryFbx(this Mesh mesh, Stream stream) {
            var writer = new FbxBinaryWriter(stream);
            writer.Write(mesh.ToFBXDocument());
        }
        #endregion

        #region File IO
        public static void ExportAsciiFbx(this Mesh mesh, string path) =>
            File.WriteAllText(path, mesh.ToAsciiFBX());

        public static void ExportBinaryFbx2(this Mesh mesh, string path) {
            string tempPath = path.Replace(".fbx", ".ascii.fbx");
            mesh.ExportAsciiFbx(tempPath);
            ConvertBinary(tempPath, path);
        }

        [Obsolete("does not read properly", error: true)]
        public static void ExportBinaryFbx(this Mesh mesh, string path) =>
            FbxIO.WriteBinary(mesh.ToFBXDocument(), path);

        #endregion

        #region conversion
        [Obsolete("does not read properly", error: true)]
        public static FbxDocument AsciiToDoc(string data) {
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

        [Obsolete("does not read properly", error: true)]
        public static FbxDocument ToFBXDocument(this Mesh mesh) {
            string data = mesh.ToAsciiFBX();
            return AsciiToDoc(data);
        }

        public static void ConvertBinary(string source, string target) {
            string modPath = PluginUtil.GetPlugin(Assembly.GetExecutingAssembly()).modPath;
            string converter = "FbxFormatConverter.exe";
            source = '"' + source + '"';
            target = '"' + target + '"';
            Execute(modPath, converter, $"-c {source} -o {target} -binary");//.WaitForExit();
        }

        #endregion

        static Process Execute(string dir, string exeFile, string args) {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                WorkingDirectory = dir,
                FileName = exeFile,
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process process = new Process { StartInfo = startInfo };
            process.Start();
            return process;
        }
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
    }
}

