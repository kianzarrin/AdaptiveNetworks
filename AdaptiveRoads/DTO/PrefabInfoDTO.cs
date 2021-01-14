using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
namespace AdaptiveRoads.DTO {
    public class PrefabInfoDTO<T> : IXmlSerializable
        where T : PrefabInfo {
        internal string name_;

        public static explicit operator PrefabInfoDTO<T>(T prefab) =>
            new PrefabInfoDTO<T> { name_ = prefab.name };

        public static explicit operator T(PrefabInfoDTO<T> prefab) {
            if (string.IsNullOrEmpty(prefab.name_))
                return null;
            else
                return PrefabCollection<T>.FindLoaded(prefab.name_);
        }

        public XmlSchema GetSchema() => null;
        public void WriteXml(XmlWriter writer) => writer.WriteString(name_);
        public void ReadXml(XmlReader reader) => name_ = reader.ReadString();
    }
}
