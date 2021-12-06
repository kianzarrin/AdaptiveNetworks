namespace AdaptiveRoads.CustomScript {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using KianCommons;
    using KianCommons.Plugins;
    using ColossalFramework.Plugins;
    using System.Collections;



    internal static class ScriptCompiler {
        private static readonly string WorkspacePath = Path.Combine(Application.temporaryCachePath, "AdaptiveRoads");
        private static readonly string SourcesPath = Path.Combine(WorkspacePath, "src");
        private static readonly string DllsPath = Path.Combine(WorkspacePath, "dll");

        private static readonly string[] GameAssemblies =
        {
            "Assembly-CSharp.dll",
            "ICities.dll",
            "ColossalManaged.dll",
            "UnityEngine.dll",
        };

        private static Hashtable LoadedAssemblies = new Hashtable();

        public static Assembly AddAssembly(byte[] data) {
            string checksum = ComputeHash(data);
            LoadedAssemblies[checksum] ??= Assembly.Load(data);
            return LoadedAssemblies[checksum] as Assembly;
        }

        static ScriptCompiler() {
            if (!Directory.Exists(WorkspacePath)) {
                Directory.CreateDirectory(WorkspacePath);
            }

            ClearFolder(WorkspacePath);
            Directory.CreateDirectory(SourcesPath);
            Directory.CreateDirectory(DllsPath);
        }

        public static PredicateBase GetPredicateInstance(Assembly assembly) {
            Assertion.NotNull(assembly, "assembly");
            var tPredicate =
                assembly.GetExportedTypes().FirstOrDefault(typeof(PredicateBase).IsAssignableFrom) ??
                throw new Exception("did not found PredicateBase");

            return
                Activator.CreateInstance(tPredicate) as PredicateBase ??
                throw new Exception("Failed to create an instance of the PredicateBase class!");
        }


        public static string ComputeHash(this FileInfo file) {
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                using (var fs = file.OpenRead()) {
                    var hash = md5.ComputeHash(file.OpenRead());
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        public static string ComputeHash(byte[] data) {
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }


        public static bool CompileSource(FileInfo file, out string dllPath) {
            try {
                var randomName = $"tmp_{file.ComputeHash()}";

                // write source files to SourcesPath\randomName\*.*
                var sourcePath = Path.Combine(SourcesPath, randomName);
                Directory.CreateDirectory(sourcePath);
                var sourceFilePath = Path.Combine(sourcePath, file.Name);
                file.CopyTo(sourceFilePath);

                Log.Debug("Source files copied to " + sourcePath);

                // compile sources to DllsPath\randomName\randomName.dll
                var outputPath = Path.Combine(DllsPath, randomName);
                Directory.CreateDirectory(outputPath);
                dllPath = Path.Combine(outputPath, randomName + ".dll");

                var AR = Path.Combine(PluginUtil.GetCurrentAssemblyPlugin().modPath, nameof(AdaptiveRoads));
                var additionalAssemblies = GameAssemblies.Concat(new[] { AR }).ToArray();

                PluginManager.CompileSourceInFolder(sourcePath, outputPath, additionalAssemblies);
                return File.Exists(dllPath);
            } catch (Exception ex) {
                Log.Exception(ex);
                dllPath = null;
                return false;
            }
        }

        private static void ClearFolder(string path) {
            var directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles()) {
                file.Delete();
            }

            foreach (var dir in directory.GetDirectories()) {
                ClearFolder(dir.FullName);
                dir.Delete();
            }
        }
    }
}
