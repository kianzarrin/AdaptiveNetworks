using PrefabMetadata.API;
using PrefabMetadata.Utils;
using System;
using System.Collections.Generic;

namespace PrefabMetadata {
    public static class NetInfoMetaDataExtension {
        public static Version Version => typeof(NetInfoMetaDataExtension).VersionOf();

        public static NetInfo GetInfo(ushort index) =>
            PrefabCollection<NetInfo>.GetPrefab(index);

        public static ushort GetIndex(this NetInfo info) =>
            Utils.Util.Clamp2U16(info.m_prefabDataIndex);

        [Serializable]
        public class Segment : NetInfo.Segment, IInfoExtended<NetInfo.Segment> {
            public List<ICloneable> m_metaData;
            public List<ICloneable> MetaData {
                get => m_metaData;
                set => m_metaData = value;
            }

            public IInfoExtended<NetInfo.Segment> Clone() {
                var ret = new Segment();
                Utils.Util.CopyProperties<NetInfo.Segment>(ret, this);
                ret.m_metaData = m_metaData.Clone();
                return ret;
            }

            public NetInfo.Segment RolledBackClone() {
                NetInfo.Segment ret = new NetInfo.Segment();
                Utils.Util.CopyProperties<NetInfo.Segment>(ret, this);
                return ret;
            }

            public static Segment Extend(NetInfo.Segment template) {
                if (template.GetType() == typeof(NetInfo.Segment)) {
                    var ret = new Segment();
                    Utils.Util.CopyProperties<NetInfo.Segment>(ret, template);
                    ret.m_metaData = new List<ICloneable>();
                    return ret;
                } else if (template is Segment template2) {
                    return template2;
                } else {
                    throw new Exception("unrecognised type:" + template.GetType());
                }
            }
        }

        [Serializable]
        public class Node : NetInfo.Node, IInfoExtended<NetInfo.Node> {
            public List<ICloneable> m_metaData;
            public List<ICloneable> MetaData {
                get => m_metaData;
                set => m_metaData = value;
            }

            public IInfoExtended<NetInfo.Node> Clone() {
                var ret = new Node();
                Utils.Util.CopyProperties<NetInfo.Node>(ret, this);
                ret.m_metaData = m_metaData.Clone();
                return ret;
            }

            public NetInfo.Node RolledBackClone() {
                NetInfo.Node ret = new NetInfo.Node();
                Utils.Util.CopyProperties<NetInfo.Node>(ret, this);
                return ret;
            }

            public static Node Extend(NetInfo.Node template) {
                if (template.GetType() == typeof(NetInfo.Node)) {
                    var ret = new Node();
                    Utils.Util.CopyProperties<NetInfo.Node>(ret, template);
                    ret.m_metaData = new List<ICloneable>();
                    return ret;
                } else if (template is Node template2) {
                    return template2;
                } else {
                    throw new Exception("unrecognised type:" + template.GetType());
                }
            }
        }

        [Serializable]
        public class Prop : NetLaneProps.Prop, IInfoExtended<NetLaneProps.Prop> {
            public List<ICloneable> m_metaData;
            public List<ICloneable> MetaData {
                get => m_metaData;
                set => m_metaData = value;
            }

            public IInfoExtended<NetLaneProps.Prop> Clone() {
                var ret = new Prop();
                Utils.Util.CopyProperties<NetLaneProps.Prop>(ret, this);
                ret.m_metaData = m_metaData.Clone();
                return ret;
            }

            public NetLaneProps.Prop RolledBackClone() {
                NetLaneProps.Prop ret = new NetLaneProps.Prop();
                Utils.Util.CopyProperties<NetLaneProps.Prop>(ret, this);
                return ret;
            }

            public static Prop Extend(NetLaneProps.Prop template) {
                if (template.GetType() == typeof(NetLaneProps.Prop)) {
                    var ret = new Prop();
                    Utils.Util.CopyProperties<NetLaneProps.Prop>(ret, template);
                    ret.m_metaData = new List<ICloneable>();
                    return ret;
                } else if (template is Prop template2) {
                    return template2;
                } else {
                    throw new Exception("unrecognised type:" + template.GetType());
                }
            }
        }
    }
}
