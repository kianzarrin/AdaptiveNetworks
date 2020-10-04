using System;
using System.Diagnostics;

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

            public void SetIndex(ushort index, ushort prefabIndex) {
                Index = index;
                PrefabIndex = prefabIndex;
            }

            public Segment(ushort index, ushort prefabIndex) {
                NetInfo.Segment tempalte = GetInfo(prefabIndex).m_segments[index];
                Util.CopyProperties<NetInfo.Segment>(this, tempalte);
                SetIndex(index, prefabIndex);
            }

            public static NetInfo.Segment Extend(ushort index, ushort prefabIndex) {
                NetInfo.Segment tempalte = GetInfo(prefabIndex).m_segments[index];
                Type t = tempalte.GetType();
                if (t == typeof(NetInfo.Segment)) {
                    return new Segment(index, prefabIndex);
                } else if (t.FullName == typeof(NetInfoExtension.Segment).FullName) {
                    Version v = tempalte.VersionOf(3);
                    Version v2 = Version.Take(3);
                    if (v > v2)
                        return tempalte;
                    else if (v == v2) {
                        Util.Assert(tempalte is NetInfoExtension.Segment, "tempalte is NetInfoExtension.Segment");
                        (tempalte as NetInfoExtension.Segment).SetIndex(index, prefabIndex);
                        return tempalte;
                    } else
                        return new Segment(index, prefabIndex);
                } else {
                    throw new Exception("unrecognised type:" + t);
                }
            }

            public  NetInfo.Segment RolledBackClone() {
                NetInfo.Segment ret = new NetInfo.Segment();
                Util.CopyProperties<NetInfo.Segment>(ret, this);
                return ret;
            }
        }

        public class Node : NetInfo.Node {
            public ushort Index;
            public ushort PrefabIndex;

            public void SetIndex(ushort index, ushort prefabIndex) {
                Index = index;
                PrefabIndex = prefabIndex;
            }

            public Node(ushort index, ushort prefabIndex) {
                NetInfo.Node tempalte = GetInfo(prefabIndex).m_nodes[index];
                Util.CopyProperties<NetInfo.Node>(this, tempalte);
                SetIndex(index, prefabIndex);
            }

            public static NetInfo.Node Extend(ushort index, ushort prefabIndex) {
                NetInfo.Node tempalte = GetInfo(prefabIndex).m_nodes[index];
                Type t = tempalte.GetType();
                if (t == typeof(NetInfo.Node))
                    return new Node(index, prefabIndex);
                else if (t.FullName == typeof(NetInfoExtension.Node).FullName) {
                    Version v = tempalte.VersionOf(3);
                    Version v2 = Version.Take(3);
                    if (v > v2)
                        return tempalte;
                    else if(v == v2) {
                        Util.Assert(tempalte is NetInfoExtension.Node, "tempalte is NetInfoExtension.Node");
                        (tempalte as NetInfoExtension.Node).SetIndex(index, prefabIndex);
                        return tempalte;
                    }
                    else
                        return new Node(index, prefabIndex);
                } else {
                    throw new Exception("unrecognised type:" + t);
                }
            }

            public NetInfo.Node RolledBackClone() {
                NetInfo.Node ret = new NetInfo.Node();
                Util.CopyProperties<NetInfo.Node>(ret, this);
                return ret;
            }
        }

        public class Lane : NetInfo.Lane {
            public ushort Index;
            public ushort PrefabIndex;

            public void SetIndex(ushort index, ushort prefabIndex) {
                Index = index;
                PrefabIndex = prefabIndex;
            }

            public Lane(ushort index, ushort prefabIndex) {
                NetInfo.Lane tempalte = GetInfo(prefabIndex).m_lanes[index];
                Util.CopyProperties<NetInfo.Lane>(this, tempalte);
                SetIndex(index, prefabIndex);
            }

            public static NetInfo.Lane Extend(ushort index, ushort prefabIndex) {
                NetInfo.Lane tempalte = GetInfo(prefabIndex).m_lanes[index];
                Type t = tempalte.GetType();
                if (t == typeof(NetInfo.Lane))
                    return new Lane(index, prefabIndex);
                else if (t.FullName == typeof(NetInfoExtension.Lane).FullName) {
                    Version v = tempalte.VersionOf(3);
                    Version v2 = Version.Take(3);
                    if (v > v2)
                        return tempalte;
                    else if (v == v2) {
                        Util.Assert(tempalte is NetInfoExtension.Lane, "tempalte is NetInfoExtension.Lane");
                        (tempalte as NetInfoExtension.Lane).SetIndex(index, prefabIndex);
                        return tempalte;
                    } else
                        return new Lane(index, prefabIndex);
                } else {
                    throw new Exception("unrecognised type:" + t);
                }
            }

            public NetInfo.Lane RolledBackClone() {
                NetInfo.Lane ret = new NetInfo.Lane();
                Util.CopyProperties<NetInfo.Lane>(ret, this);
                return ret;
            }

            public class Prop : NetLaneProps.Prop {
                public ushort Index;
                public ushort LaneIndex;
                public ushort PrefabIndex;

                public void SetIndex(ushort index, ushort laneIndex, ushort prefabIndex) {
                    Index = index;
                    LaneIndex = laneIndex;
                    PrefabIndex = prefabIndex;
                }

                public Prop(ushort index, ushort laneIndex, ushort prefabIndex) {
                    NetLaneProps.Prop tempalte = GetInfo(prefabIndex).m_lanes[laneIndex].m_laneProps.m_props[index];
                    Util.CopyProperties<NetLaneProps.Prop>(this, tempalte);
                    SetIndex(index, laneIndex, prefabIndex);
                }

                public static NetLaneProps.Prop Extend(ushort index, ushort laneIndex, ushort prefabIndex) {
                    NetLaneProps.Prop tempalte = GetInfo(prefabIndex).m_lanes[laneIndex].m_laneProps.m_props[index];
                    Type t = tempalte.GetType();
                    if (t == typeof(NetLaneProps.Prop))
                        return new Prop(index, laneIndex, prefabIndex);
                    else if (t.FullName == typeof(NetInfoExtension.Lane.Prop).FullName) {
                        Version v = tempalte.VersionOf(3);
                        Version v2 = Version.Take(3);
                        if (v > v2)
                            return tempalte;
                        else if (v == v2) {
                            Util.Assert(tempalte is Prop, "tempalte is Prop");
                            (tempalte as Prop).SetIndex(index, laneIndex, prefabIndex);
                            return tempalte;
                        } else
                            return new Prop(index, laneIndex, prefabIndex);
                    } else {
                        throw new Exception("unrecognised type:" + t);
                    }
                }
                public NetLaneProps.Prop RolledBackClone() {
                    NetLaneProps.Prop ret = new NetLaneProps.Prop();
                    Util.CopyProperties<NetLaneProps.Prop>(ret, this);
                    return ret;
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

        public static void ExtendPrefab(this NetInfo info) {
            UnityEngine.Debug.Log($"ExtendPrefab({info})");
            if (info == null) return;
            ExtendPrefabNodes(info);
            ExtendPrefabSegments(info);
            ExtendPrefabLanes(info);
            ExtendPrefabProps(info);
        }

        public static void ReversePrefab(this NetInfo info) {
            if (info.m_nodes != null) {
                var nodes = new NetInfo.Node[info.m_nodes.Length];
                for (ushort i = 0; i < nodes.Length; ++i) {
                    if (info.m_nodes[i] is NetInfoExtension.Node nodeExt) {
                        nodes[i] = nodeExt.RolledBackClone();
                    }
                }
                info.m_nodes = nodes;
            }

            if (info.m_segments != null) {
                var segments = new NetInfo.Segment[info.m_segments.Length];
                for (ushort i = 0; i < segments.Length; ++i) {
                    if (info.m_segments[i] is NetInfoExtension.Segment segmentExt) {
                        segments[i] = segmentExt.RolledBackClone();
                    }
                }
                info.m_segments = segments;
            }

            if (info.m_lanes != null) {
                var lanes = new NetInfo.Lane[info.m_lanes.Length];
                for (ushort i = 0; i < lanes.Length; ++i) {
                    if (info.m_lanes[i] is NetInfoExtension.Lane laneExt) {
                        lanes[i] = laneExt.RolledBackClone();
                    }
                }
                info.m_lanes = lanes;

                for (ushort laneIndex = 0; laneIndex < info.m_lanes.Length; ++laneIndex) {
                    var lane = info.m_lanes[laneIndex];
                    var props = lane?.m_laneProps?.m_props;
                    if (props == null)
                        continue;
                    var n = props.Length;
                    var new_props = new NetLaneProps.Prop[n];
                    for (ushort i = 0; i < n; ++i) {
                        if (props[i] is NetInfoExtension.Lane.Prop propExt) {
                            new_props[i] = propExt.RolledBackClone();
                        }
                    }
                    lane.m_laneProps.m_props = new_props;
                }
            }
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
            if (!info || info == null) throw new Exception("info is null");
            if (info.m_lanes == null) throw new Exception("m_lanes is null");
            for (ushort laneIndex = 0; laneIndex < info.m_lanes.Length; ++laneIndex) {
                var lane = info.m_lanes[laneIndex];
                var props = lane?.m_laneProps?.m_props;
                if (props == null)
                    continue;
                var n = props.Length;
                var new_props = new NetLaneProps.Prop[n];
                for (ushort i = 0; i < n; ++i) {
                    new_props[i] = NetInfoExtension.Lane.Prop.Extend(i, laneIndex, infoIndex);
                }
                lane.m_laneProps.m_props = new_props;
            }
        }


    }
}
