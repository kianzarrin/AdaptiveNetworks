using ColossalFramework.IO;
using KianCommons.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using KianCommons;

namespace AdaptiveRoads.DTO {
    public class MultiSerializer<T> where T : ISerialziableDTO
        {
        public  const string FileExt = ".xml";

    
        public  string Dir => Path.Combine(DataLocation.localApplicationData, "ARTemplates");
        public  string FilePath(string name) => Path.Combine(Dir, name + FileExt);

        public  void Save(string name, T value) {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new Exception($"Name:{name} contains invalid characters");
            EnsureDir();
            string path = FilePath(name);
            string data = XMLSerializerUtil.Serialize(value);
            XMLSerializerUtil.WriteToFile(path, data);
        }

        public  T Load(string name) {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException($"name:{name} contains invalid characters");
            return Load(FilePath(name));

        }
        public T LoadFile(string path) {
            EnsureDir();
            string data = XMLSerializerUtil.ReadFromFile(path);
            //Version version = XMLSerializerUtil.ExtractVersion(data);
            var ret = XMLSerializerUtil.Deserialize<T>(data);
            return ret;
        }

        public IEnumerable<T> LoadAllFiles() {
            EnsureDir();
            var dir = new DirectoryInfo(Dir);
            var files = dir.GetFiles("*" + FileExt);
            foreach (var file in files) {
                var ret = LoadFile(file.FullName);
                if (ret != null) {
                    ret.OnLoaded(file);
                    yield return ret;
                }
            }
        }

        public void EnsureDir() {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
        }
    }
}
