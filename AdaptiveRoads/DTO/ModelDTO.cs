namespace AdaptiveRoads.DTO {
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using UnityEngine;
    using FbxUtil;
    using ObjUnity3D;
    using CSUtil.Commons;
    using AdaptiveRoads.Util.Model;
    using System.Xml.Schema;
    using System.Xml;
    using KianCommons;

    public class ModelDTO : IXmlSerializable {
        public Mesh mesh, lodMesh;
        public Material material, lodMaterial;
        public string ShaderName;
        public Color32 Color = UnityEngine.Color.grey;
        public string Model; // subfolder/prefix
        string RelativePath => Path.GetDirectoryName(Model); // subfolder
        string ModelName => Path.GetFileName(Model); // prefix


        public void Export(string dir) {
            if (Model.IsNullOrEmpty()) return;
            dir = Path.Combine(dir, RelativePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            mesh.DumpToFBX(Path.Combine(dir, ModelName + ".fbx"));
            lodMesh.DumpToFBX(Path.Combine(dir, ModelName + "_lod.fbx"));
            material.Dump(dir, ModelName, lod: false);
            lodMaterial.Dump(dir, ModelName, lod: true);
        }

        public void Import(string dir) {
            if (Model.IsNullOrEmpty()) return;
            dir = Path.Combine(dir, RelativePath);

            var importer = NoPaddingImportAssetModel.ImportModel(dir, ModelName, ShaderName);
            mesh = importer.mesh;
            lodMesh = importer.lodMesh;
            material = importer.material;
            lodMaterial = importer.lodMaterial;
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader) {
            reader.MoveToContent();
            Model = reader.GetAttribute("name")?.Trim('/', '\\');

            if (!Model.IsNullOrEmpty()) {
                bool isEmptyElement = reader.IsEmptyElement; // (1)
                reader.ReadStartElement();
                if (!isEmptyElement) 
                {
                    ShaderName = reader.ReadElementString("Shader");
                    reader.ReadEndElement();
                }

                isEmptyElement = reader.IsEmptyElement; // (2)
                reader.ReadStartElement();
                if (!isEmptyElement) {
                    string color = reader.ReadElementString("Color");
                    var rgba = color?.Split(',');
                    Color = UnityEngine.Color.grey;
                    try {
                        if (!rgba.IsNullorEmpty()) {
                            Color = new Color32 {
                                r = byte.Parse(rgba[0], null),
                                g = byte.Parse(rgba[1], null),
                                b = byte.Parse(rgba[2], null),
                                a = byte.Parse(rgba[3], null),
                            };
                        }
                    } catch (Exception ex) {
                        ex.Log("bad color :" + color, false);
                    }
                    reader.ReadEndElement();
                }
            }
        }

        public void WriteXml(XmlWriter writer) {
            Model = Model?.Trim('/', '\\');
            writer.WriteAttributeString("name", Model);
            if (!Model.IsNullOrEmpty()) {
                writer.WriteElementString("Shader", ShaderName);
                writer.WriteElementString("Color", $"{Color.r}, {Color.g}, {Color.b}, {Color.a}");
            }
        }
    }
}
