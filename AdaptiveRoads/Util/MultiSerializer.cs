using ColossalFramework.IO;
using KianCommons.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace AdaptiveRoads.Util {
    public class MultiSerializer<T> where T : DTO.ISerialziableDTO
        {
        public string SubDir;// = "ARTemplates";
        public  string FileExt = ".xml";

        public MultiSerializer(string subDir) => SubDir = subDir;
        public MultiSerializer(string subDir, string fileExt) {
            SubDir = subDir;
            FileExt = fileExt;
        }
    
        public  string Dir => Path.Combine(DataLocation.localApplicationData, "ARTemplates");
        public  string FilePath(string name) => Path.Combine(Dir, name + FileExt);

        public  void Save(string name, T value) {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new Exception($"Name:{name} contains invalid characters");
            EnsureDir();
            string path = FilePath(name);
            string data = XMLSerializerUtil.Serialize(value);
            XMLSerializerUtil.WriteToFileWrapper(path, data);
        }

        public  T Load(string name) {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException($"name:{name} contains invalid characters");
            return Load(FilePath(name));

        }
        public  T LoadFile(string path) {
            EnsureDir();
            string data = XMLSerializerUtil.ReadFromFileWrapper(path, out Version version);
            var ret = XMLSerializerUtil.Deserialize<T>(data);
            typeof(T).GetMethod("OnLoaded")?.Invoke(ret, null);
            return ret;
        }

        public IEnumerable<T> LoadAllFiles() {
            EnsureDir();
            var dir = new DirectoryInfo(Dir);
            var files = dir.GetFiles("*" + FileExt);
            foreach (var file in files) {
                var ret = LoadFile(file.FullName);
                if (ret != null) {
                    ret.OnLoaded();
                    yield return ret;
                }
            }
        }

        public  void EnsureDir() {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
        }
    }
}
