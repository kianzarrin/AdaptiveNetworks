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
            Log.Info($"Adding Assembly with checksum:{checksum} reuse={LoadedAssemblies[checksum]}", true);
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


        public static bool CompileSource(FileInfo file, out FileInfo dllFile) {
            try {
                var randomName = $"AR_ScriptedFlag_{file.ComputeHash()}";

                // write source files to SourcesPath\randomName\*.*
                var sourcePath = Path.Combine(SourcesPath, randomName);

                if(Directory.Exists(sourcePath))
                    Directory.Delete(sourcePath, recursive: true);
                Directory.CreateDirectory(sourcePath); // clean slate

                var sourceFilePath = Path.Combine(sourcePath, file.Name);
                file.CopyTo(sourceFilePath, overwrite:true);

                Log.Debug("Source files copied to " + sourcePath);

                // compile sources to DllsPath\randomName\randomName.dll
                var outputPath = Path.Combine(DllsPath, randomName);
                Directory.CreateDirectory(outputPath);
                string dllPath = Path.Combine(outputPath, randomName + ".dll");

                // TODO: compile with reference assemblies.
                var ARFiles =  Directory.GetFiles(PluginUtil.GetCurrentAssemblyPlugin().modPath, "*.dll");
                var TMPEFiles = Directory.GetFiles(PluginUtil.GetTrafficManager().modPath, "*.dll");
                var HarmonyFiles = Directory.GetFiles(PluginUtil.GetPlugin(typeof(HarmonyLib.Harmony).Assembly).modPath, "*.dll");
                var additionalAssemblies = GameAssemblies.Concat(ARFiles).Concat(TMPEFiles).Concat(HarmonyFiles).ToArray();

                Log.Info($"Calling PluginManager.CompileSourceInFolder({sourcePath}, {outputPath}, {additionalAssemblies.ToSTR()})");
                PluginManager.CompileSourceInFolder(sourcePath, outputPath, additionalAssemblies);
                dllFile = new FileInfo(dllPath);
                return dllFile.Exists;
            } catch (Exception ex) {
                Log.Exception(ex);
                dllFile = null;
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
