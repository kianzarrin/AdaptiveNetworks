using PrefabMetadata.API;
using PrefabMetadata.Utils;
using System;
using System.Collections.Generic;
using PrefabMetadata.Helpers;

namespace PrefabMetadata {
    internal static class NetInfoMetaDataExtension {
        internal static Version Version => typeof(NetInfoMetaDataExtension).VersionOf();

        internal static NetInfo GetInfo(ushort index) =>
            PrefabCollection<NetInfo>.GetPrefab(index);

        internal static ushort GetIndex(this NetInfo info) =>
            Utils.Util.Clamp2U16(info.m_prefabDataIndex);

        [Serializable]
        internal class Segment : NetInfo.Segment, IInfoExtended<NetInfo.Segment> {
            internal List<ICloneable> m_metaData;
            public List<ICloneable> MetaData {
                get => m_metaData;
                set => m_metaData = value;
            }

            public NetInfo.Segment Base => this;

            public IInfoExtended<NetInfo.Segment> Clone() {
                var ret = new Segment();
                Utils.Util.CopyProperties<NetInfo.Segment>(ret, this);
                ret.m_metaData = m_metaData.Clone();
                return ret;
            }

            object ICloneable.Clone() => Clone();

            public NetInfo.Segment UndoExtend() {
                NetInfo.Segment ret = new NetInfo.Segment();
                Utils.Util.CopyProperties<NetInfo.Segment>(ret, this);
                return ret;
            }

            internal static Segment Extend(NetInfo.Segment template) {
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
        internal class Node : NetInfo.Node, IInfoExtended<NetInfo.Node> {
            internal List<ICloneable> m_metaData;
            public List<ICloneable> MetaData {
                get => m_metaData;
                set => m_metaData = value;
            }
            public NetInfo.Node Base => this;

            public IInfoExtended<NetInfo.Node> Clone() {
                var ret = new Node();
                Utils.Util.CopyProperties<NetInfo.Node>(ret, this);
                ret.m_metaData = m_metaData.Clone();
                ret.m_tagsForbidden = m_tagsForbidden.Clone() as string[];
                ret.m_tagsRequired = m_tagsRequired.Clone() as string[];
                Util.CopyProperties(ret.m_nodeTagsRequired, m_nodeTagsRequired);
                Util.CopyProperties(ret.m_nodeTagsForbidden, m_nodeTagsForbidden);

                return ret;
            }

            object ICloneable.Clone() => Clone();

            public NetInfo.Node UndoExtend() {
                NetInfo.Node ret = new NetInfo.Node();
                Utils.Util.CopyProperties<NetInfo.Node>(ret, this);
                return ret;
            }

            internal static Node Extend(NetInfo.Node template) {
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
        internal class LaneProp : NetLaneProps.Prop, IInfoExtended<NetLaneProps.Prop> {
            internal List<ICloneable> m_metaData;
            public List<ICloneable> MetaData {
                get => m_metaData;
                set => m_metaData = value;
            }

            public NetLaneProps.Prop Base => this;

            public IInfoExtended<NetLaneProps.Prop> Clone() {
                var ret = new LaneProp();
                Utils.Util.CopyProperties<NetLaneProps.Prop>(ret, this);
                ret.m_metaData = m_metaData.Clone();
                return ret;
            }

            object ICloneable.Clone() => Clone();

            public NetLaneProps.Prop UndoExtend() {
                NetLaneProps.Prop ret = new NetLaneProps.Prop();
                Utils.Util.CopyProperties<NetLaneProps.Prop>(ret, this);
                return ret;
            }

            internal static LaneProp Extend(NetLaneProps.Prop template) {
                if (template.GetType() == typeof(NetLaneProps.Prop)) {
                    var ret = new LaneProp();
                    Utils.Util.CopyProperties<NetLaneProps.Prop>(ret, template);
                    ret.m_metaData = new List<ICloneable>();
                    return ret;
                } else if (template is LaneProp template2) {
                    return template2;
                } else {
                    throw new Exception("unrecognised type:" + template.GetType());
                }
            }
        }
    }
}
