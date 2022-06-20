namespace AdaptiveRoads.DTO {
    using AdaptiveRoads.Util;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;


    public abstract class TemplateBase<T> : ISerialziableDTO
        where T: TemplateBase<T> {
        public TemplateBase() {
            Name = Name?.RemoveChars(Path.GetInvalidFileNameChars());
            Date = DateTime.UtcNow;
            version_ = this.VersionOf();
        }

        [XmlIgnore] protected Version version_;

        [XmlAttribute] public string Version {
            get => version_.ToString();
            set => version_ = new Version(value);
        }
        [XmlIgnore] public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date;

        public abstract string Summary { get; }

        private static MultiSerializer<T> Serializer = new MultiSerializer<T>();
        public void Save() => Serializer.Save(Name, this as T);

        public void OnLoaded(FileInfo file) => Name = file.Name.RemoveExtension();
        public static IEnumerable<T> LoadAllFiles() => Serializer.LoadAllFiles();
    }
}
