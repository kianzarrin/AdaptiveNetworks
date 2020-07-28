using System;
using UnityEngine.Assertions.Must;

namespace PrefabIndeces {
    public static class NetInfoExtension {
        public static Version Version => Util.VersionOf(typeof(NetInfoExtension));

        public static NetInfo GetInfo(ushort index) =>
            PrefabCollection<NetInfo>.GetPrefab(index);
        public static ushort GetIndex(this NetInfo info) =>
            Util.Clamp2UShort(info.m_prefabDataIndex);

        public class Segment : NetInfo.Segment {
            public ushort Index;
            public ushort PrefabIndex;
            public Segment(ushort index, ushort prefabIndex) {
                NetInfo.Segment tempalte = GetInfo(prefabIndex).m_segments[index];
                Util.CopyProperties<NetInfo.Segment>(this, tempalte);
                Index = index;
                PrefabIndex = prefabIndex;
            }

            public static NetInfo.Segment Extend(ushort index, ushort prefabIndex) {
                NetInfo.Segment tempalte = GetInfo(prefabIndex).m_segments[index];
                Type t = tempalte.GetType();
                if (t == typeof(NetInfo.Segment))
                    return new Segment(index, prefabIndex);
                else if (t.FullName == typeof(NetInfo.Segment).FullName) {
                    if (Util.VersionOf(tempalte) >= Version)
                        return tempalte;
                    else
                        return new Segment(index, prefabIndex);
                } else {
                    throw new Exception("unrecognised type:" + t);
                }
            }
        }

        public class Node : NetInfo.Node {
            public ushort Index;
            public ushort PrefabIndex;
            public Node(ushort index, ushort prefabIndex) {
                NetInfo.Node tempalte = GetInfo(prefabIndex).m_nodes[index];
                Util.CopyProperties<NetInfo.Node>(this, tempalte);
                Index = index;
                PrefabIndex = prefabIndex;
            }

            public static NetInfo.Node Extend(ushort index, ushort prefabIndex) {
                NetInfo.Node tempalte = GetInfo(prefabIndex).m_nodes[index];
                Type t = tempalte.GetType();
                if (t == typeof(NetInfo.Node))
                    return new Node(index, prefabIndex);
                else if (t.FullName == typeof(NetInfo.Node).FullName) {
                    if (Util.VersionOf(tempalte) >= Version)
                        return tempalte;
                    else
                        return new Node(index, prefabIndex);
                } else {
                    throw new Exception("unrecognised type:" + t);
                }
            }
        }

        public class Lane : NetInfo.Lane {
            public ushort Index;
            public ushort PrefabIndex;
            public Lane(ushort index, ushort prefabIndex) {
                NetInfo.Lane tempalte = GetInfo(prefabIndex).m_lanes[index];
                Util.CopyProperties<NetInfo.Lane>(this, tempalte);
                Index = index;
                PrefabIndex = prefabIndex;
            }
            public static NetInfo.Lane Extend(ushort index, ushort prefabIndex) {
                NetInfo.Lane tempalte = GetInfo(prefabIndex).m_lanes[index];
                Type t = tempalte.GetType();
                if (t == typeof(NetInfo.Lane))
                    return new Lane(index, prefabIndex);
                else if (t.FullName == typeof(NetInfo.Lane).FullName) {
                    if (Util.VersionOf(tempalte) >= Version)
                        return tempalte;
                    else
                        return new Lane(index, prefabIndex);
                } else {
                    throw new Exception("unrecognised type:" + t);
                }
            }


            public class Prop : NetLaneProps.Prop {
                public ushort Index;
                public ushort LaneIndex;
                public ushort PrefabIndex;
                public Prop(ushort index, ushort laneIndex, ushort prefabIndex) {
                    NetLaneProps.Prop tempalte = GetInfo(prefabIndex).m_lanes[laneIndex].m_laneProps.m_props[index];
                    Util.CopyProperties<NetLaneProps.Prop>(this, tempalte);
                    Index = index;
                    LaneIndex = laneIndex;
                    PrefabIndex = prefabIndex;
                }

                public static NetLaneProps.Prop Extend(ushort index, ushort laneIndex, ushort prefabIndex) {
                    NetLaneProps.Prop tempalte = GetInfo(prefabIndex).m_lanes[laneIndex].m_laneProps.m_props[index];
                    Type t = tempalte.GetType();
                    if (t == typeof(NetLaneProps.Prop))
                        return new Prop(index, laneIndex, prefabIndex);
                    else if (t.FullName == typeof(NetLaneProps.Prop).FullName) {
                        if (Util.VersionOf(tempalte) >= Version)
                            return tempalte;
                        else
                            return new Prop(index, laneIndex, prefabIndex);
                    } else {
                        throw new Exception("unrecognised type:" + t);
                    }
                }
            }
        }

        public static void ExtendLoadedPrefabs() {
            int n = PrefabCollection<NetInfo>.LoadedCount();
            for (uint i = 0; i < n; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                ExtendPrefab(info);
            }
        }

        public static void ExtendPrefab(NetInfo info) {
            ExtendPrefabNodes(info);
            ExtendPrefabSegments(info);
            ExtendPrefabLanes(info);
            ExtendPrefabProps(info);
        }

        public static void ExtendPrefabNodes(NetInfo info) {
            ushort infoIndex = info.GetIndex();
            var nodes = new NetInfo.Node[info.m_nodes.Length];
            for (ushort i = 0; i < nodes.Length; ++i) {
                nodes[i] = NetInfoExtension.Node.Extend(i, infoIndex);
            }
            info.m_nodes = nodes;
        }

        public static void ExtendPrefabSegments(NetInfo info) {
            ushort infoIndex = info.GetIndex();
            var segments = new NetInfo.Segment[info.m_segments.Length];
            for (ushort i = 0; i < segments.Length; ++i) {
                segments[i] = NetInfoExtension.Segment.Extend(i, infoIndex);
            }
            info.m_segments = segments;
        }

        public static void ExtendPrefabLanes(NetInfo info) {
            ushort infoIndex = info.GetIndex();
            var lanes = new NetInfo.Lane[info.m_lanes.Length];
            for (ushort i = 0; i < lanes.Length; ++i) {
                lanes[i] = NetInfoExtension.Lane.Extend(i, infoIndex);
            }
            info.m_lanes = lanes;
        }

        public static void ExtendPrefabProps(NetInfo info) {
            ushort infoIndex = info.GetIndex();
            for (ushort laneIndex = 0; laneIndex < info.m_lanes.Length; ++laneIndex) {
                var n = info.m_lanes[laneIndex].m_laneProps.m_props.Length;
                var new_props = new NetLaneProps.Prop[n];
                for (ushort i = 0; i < n; ++i) {
                    new_props[i] =  NetInfoExtension.Lane.Prop.Extend(i, laneIndex, infoIndex);
                }
                info.m_lanes[laneIndex].m_laneProps.m_props = new_props;
            }
        }

    }
}
