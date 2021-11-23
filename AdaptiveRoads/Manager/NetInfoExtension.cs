using AdaptiveRoads.UI.RoadEditor.Bitmask;
using AdaptiveRoads.Util;
using ColossalFramework;
using KianCommons;
using KianCommons.Math;
using KianCommons.Serialization;
using PrefabMetadata.API;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using static AdaptiveRoads.Manager.NetInfoExtionsion;
using static AdaptiveRoads.UI.ModSettings;
using static KianCommons.ReflectionHelpers;
using AdaptiveRoads.Data.NetworkExtensions;

namespace AdaptiveRoads.Manager {
    using static HintExtension;
    using Vector3Serializable = KianCommons.Serialization.Vector3Serializable;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class FlagPairAttribute : Attribute {
        public Type MergeWithEnum;
    }

    public class AfterFieldAttribute : Attribute {
        public string FieldName;
        public AfterFieldAttribute(string fieldName) => FieldName = fieldName;
    }

    /// <summary>
    /// Field visibility in asset is controlled by settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class OptionalAttribute : Attribute {
        public string Option;
        public OptionalAttribute(string option) => Option = option;
    }

    public static class Extensions {
        internal static NetInfo GetInfo(ushort index) =>
            PrefabCollection<NetInfo>.GetPrefab(index);

        internal static ushort GetIndex(this NetInfo info) =>
            MathUtil.Clamp2U16(info.m_prefabDataIndex);

        public static Segment GetMetaData(this NetInfo.Segment segment) =>
            (segment as IInfoExtended)?.GetMetaData<Segment>();

        public static Node GetMetaData(this NetInfo.Node node) =>
            (node as IInfoExtended)?.GetMetaData<Node>();

        public static LaneProp GetMetaData(this NetLaneProps.Prop prop) =>
            (prop as IInfoExtended)?.GetMetaData<LaneProp>();

        public static Net GetMetaData(this NetInfo netInfo) =>
            NetMetadataContainer.GetMetadata(netInfo);

        public static Net GetOrCreateMetaData(this NetInfo netInfo) =>
            NetMetadataContainer.GetOrCreateMetadata(netInfo);

        public static void SetMetedata(this NetInfo netInfo, Net value) =>
            NetMetadataContainer.SetMetadata(netInfo, value);

        public static void RecalculateMetaData(this NetInfo netInfo) {
            netInfo.GetMetaData()?.Recalculate(netInfo);
        }

        public static void RemoveMetadataContainer(this NetInfo netInfo) =>
            NetMetadataContainer.RemoveContainer(netInfo);

        public static Segment GetOrCreateMetaData(this NetInfo.Segment segment) {
            Assertion.Assert(segment is IInfoExtended);
            var segment2 = segment as IInfoExtended;
            var ret = segment2.GetMetaData<Segment>();
            if(ret == null) {
                ret = new Segment(segment);
                segment2.SetMetaData(ret);
            }
            return ret;
        }

        public static Node GetOrCreateMetaData(this NetInfo.Node node) {
            Assertion.Assert(node is IInfoExtended);
            var node2 = node as IInfoExtended;
            var ret = node2.GetMetaData<Node>();
            if(ret == null) {
                ret = new Node(node);
                node2.SetMetaData(ret);
            }
            return ret;
        }

        public static LaneProp GetOrCreateMetaData(this NetLaneProps.Prop prop) {
            Assertion.Assert(prop is IInfoExtended);
            var prop2 = prop as IInfoExtended;
            var ret = prop2.GetMetaData<LaneProp>();
            if(ret == null) {
                ret = new LaneProp(prop);
                prop2.SetMetaData(ret);
            }
            return ret;
        }

        public static bool CheckRange(this Range range, float value) => range?.InRange(value) ?? true;

        public static bool CheckFlags(this NetInfo.Segment segmentInfo, NetSegment.Flags flags, bool turnAround) {
            if(!turnAround)
                return flags.CheckFlags(segmentInfo.m_forwardRequired, segmentInfo.m_forwardForbidden);
            else
                return flags.CheckFlags(segmentInfo.m_backwardRequired, segmentInfo.m_backwardForbidden);
        }

        /// <summary>
        /// Gets the used lane flags for the input lane (not all the lanes of the NetInfo)
        /// </summary>
        public static NetLaneExt.Flags GetUsedCustomFlagsLane(this NetInfo.Lane laneInfo) =>
            Net.GetUsedCustomFlagsLane(laneInfo);

        /// <param name="layerMask">for calculate/populate group data <c>layerMask=1<<layer</c></param>
        /// <returns>if there is any matching layer</returns>
        public static bool CheckNetLayers(this NetInfo info, int layerMask) => info && (layerMask & info.m_netLayers) != 0;
        public static int TrackLaneCount(this NetInfo info) => info?.GetMetaData()?.TrackLaneCount ?? 0;
    }

    public class NetMetadataContainer : MonoBehaviour {
        public Net Metadata;
        void OnDestroy() => Metadata = null;

        public static NetMetadataContainer GetContainer(NetInfo info) =>
            info?.gameObject.GetComponent<NetMetadataContainer>();
        public static Net GetMetadata(NetInfo info) =>
            GetContainer(info)?.Metadata;

        public static NetMetadataContainer GetOrCreateContainer(NetInfo info) {
            Assertion.Assert(info, "info");
            return GetContainer(info) ??
                info.gameObject.AddComponent<NetMetadataContainer>();
        }
        public static Net GetOrCreateMetadata(NetInfo info) {
            var container = GetOrCreateContainer(info);
            return container.Metadata ??= new Net(info);
        }

        public static void SetMetadata(NetInfo info, Net value) {
            Assertion.Assert(info, "info");
            GetOrCreateContainer(info).Metadata = value;
        }

        public static void RemoveContainer(NetInfo info) {
            Assertion.Assert(info, "info");
            DestroyImmediate(GetContainer(info));
        }
    }

    [Serializable]
    public static class NetInfoExtionsion {
        #region value types
        [Serializable]
        public class Range {
            public float Lower, Upper;
            public bool InRange(float value) => Lower <= value && value < Upper;
            public override string ToString() => $"[{Lower}:{Upper})";
        }

        [Serializable]
        [FlagPair]
        public struct VanillaSegmentInfoFlags {
            [BitMask]
            public NetSegment.Flags Required, Forbidden;
            public bool CheckFlags(NetSegment.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        public struct VanillaNodeInfoFlags {
            [BitMask]
            public NetNode.Flags Required, Forbidden;
            public bool CheckFlags(NetNode.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetSegment.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetSegmentFlags))]
        [Serializable]
        public struct SegmentInfoFlags {
            [BitMask]
            public NetSegmentExt.Flags Required, Forbidden;
            public NetSegmentExt.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentExt.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        [Hint("segment specific node flags")]
        public struct SegmentEndInfoFlags {
            [BitMask]
            public NetSegmentEnd.Flags Required, Forbidden;
            public NetSegmentEnd.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentEnd.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetNode.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlags))]
        [Serializable]
        public struct NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            public NetNodeExt.Flags UsedCustomFlags => (Required | Forbidden) & NetNodeExt.Flags.CustomsMask;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [FlagPair(MergeWithEnum = typeof(NetLane.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetLaneFlags))]
        [Serializable]
        public struct LaneInfoFlags {
            [BitMask]
            public NetLaneExt.Flags Required, Forbidden;
            public NetLaneExt.Flags UsedCustomFlags => (Required | Forbidden) & NetLaneExt.Flags.CustomsMask;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        #endregion

        #region sub prefab extensions


        [Serializable]
        [Optional(AR_MODE)]
        public class Net : ICloneable, ISerializable {
            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Net() { }
            public Net Clone() {
                var ret = this.ShalowClone();
                ret.ConnectGroups = ret.ConnectGroups?.ToArray();
                ret.ConnectGroupsHash = ret.ConnectGroupsHash?.ToArray();
                ret.NodeConnectGroupsHash = ret.NodeConnectGroupsHash?.ToArray();
                ret.QuayRoadsProfile = QuayRoadsProfile?.ToArray();
                ret.CustomFlagNames = ret.CustomFlagNames?.ToDictionary(entry => entry.Key, entry => entry.Value);
                ret.CustomLaneFlagNames0 = ret.CustomLaneFlagNames0?.ToDictionary(entry => entry.Key, entry => entry.Value);
                ret.CustomLaneFlagNames = ret.CustomLaneFlagNames
                    ?.Select(item => item?.ToDictionary(entry => entry.Key, entry => entry.Value))
                    ?.ToArray();
                for(int i = 0; i < ret.Tracks.Length; ++i) {
                    ret.Tracks[i] = ret.Tracks[i].Clone();
                }

                return ret;
            }

            object ICloneable.Clone() => Clone();
            public Net(NetInfo template) {
                PavementWidthRight = template.m_pavementWidth;
                UsedCustomFlags = GatherUsedCustomFlags(template);
                Template = template;
            }

            public void ReleaseModels() {
                foreach(var track in Tracks) {
                    track.ReleaseModel();
                }
            }

            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) {
                FillCustomLaneFlagNames();
                SerializationUtil.GetObjectFields(info, this);
            }

            // deserialization
            public Net(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.SetObjectFields(info, this);
            #endregion

            [NonSerialized]
            public NetInfo Template;

            public string[] ConnectGroups;

            [NonSerialized]
            public int[] NodeConnectGroupsHash;


            [NonSerialized]
            public int[] ConnectGroupsHash;

            [AfterField(nameof(NetInfo.m_pavementWidth))]
            [CustomizableProperty("Pavement Width Right", "Properties")]
            public float PavementWidthRight;

            [AfterField(nameof(NetInfo.m_pavementWidth))]
            [CustomizableProperty("Shift", "Properties")]
            [Hint("shifts road right-wards (when going from tail to head)")]
            public float Shift = 0;

            [AfterField(nameof(NetInfo.m_minCornerOffset))]
            [CustomizableProperty("Sharp Corners", "Properties")]
            [Hint("only works when corner is 90deg")]
            public bool SharpCorners;

            [AfterField(nameof(NetInfo.m_minCornerOffset))]
            [CustomizableProperty("Parking Angle °", "Properties")]
            public float ParkingAngleDegrees = 0;

            /// <summary>
            /// 1/sin(ParkingAngleDegrees)
            /// </summary>
            [NonSerialized]
            public float OneOverSinOfParkingAngle = 1;

            public Data.QuayRoads.ProfileSection[] QuayRoadsProfile = null;

            #region Custom Flags
            [NonSerialized]
            public CustomFlags UsedCustomFlags;

            public Dictionary<Enum, string> CustomFlagNames = new Dictionary<Enum, string>();

            [NonSerialized]
            public Dictionary<NetInfo.Lane, Dictionary<NetLaneExt.Flags, string>> CustomLaneFlagNames0;

            public Dictionary<NetLaneExt.Flags, string>[] CustomLaneFlagNames;

            [CustomizableProperty("Tracks")]
            [AfterField("m_nodes")]
            public Track[] Tracks = new Track[0];

            [NonSerialized]
            public ulong TrackLanes;

            [NonSerialized]
            public int TrackLaneCount;

            public bool HasTrackLane(int laneIndex) => ((1ul << laneIndex) & TrackLanes) != 0;

            static CustomFlags GatherUsedCustomFlags(NetInfo info) {
                var ret = CustomFlags.None;
                foreach(var item in info.m_nodes) {
                    if(item.GetMetaData() is Node metaData)
                        ret |= metaData.UsedCustomFlags;
                }

                foreach(var item in info.m_segments) {
                    if(item.GetMetaData() is Segment metaData)
                        ret |= metaData.UsedCustomFlags;
                }

                foreach(var lane in info.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    if(props.IsNullorEmpty()) continue;
                    foreach(var item in props) {
                        if(item.GetMetaData() is LaneProp metaData)
                            ret |= metaData.UsedCustomFlags;
                    }
                }

                foreach(var track in info.GetMetaData()?.Tracks ?? Enumerable.Empty<Track>()) {
                    ret |= track.UsedCustomFlags;
                }

                return ret;
            }

            private void FillCustomLaneFlagNames() {
                try {
                    CustomLaneFlagNames = null;
                    if(CustomLaneFlagNames.IsNullorEmpty()) return;
                    CustomLaneFlagNames = new Dictionary<NetLaneExt.Flags, string>[Template.m_lanes.Length];
                    for(int laneIndex = 0; laneIndex < CustomLaneFlagNames.Length; ++laneIndex) {
                        var lane = Template.m_lanes[laneIndex];
                        if(CustomLaneFlagNames0.TryGetValue(lane, out var dict)) {
                            CustomLaneFlagNames[laneIndex] = dict;
                        }
                    }
                } catch(Exception ex) { ex.Log(); }
            }

            public string GetCustomLaneFlagName(NetLaneExt.Flags flag, int laneIndex) {
                try {
                    if(CustomLaneFlagNames0 is not null) {
                        // edit prefab
                        var lane = Template.m_lanes[laneIndex];
                        if(CustomLaneFlagNames0.TryGetValue(lane, out var dict) &&
                            dict.TryGetValue(flag, out string name)) {
                            return name;
                        }

                    } else if(CustomLaneFlagNames is not null) {
                        // normal
                        Assertion.InRange(CustomLaneFlagNames, laneIndex);
                        var dict = CustomLaneFlagNames[laneIndex];
                        if(dict != null && dict.TryGetValue(flag, out string name)) {
                            return name;
                        }
                    }
                } catch(Exception ex) { ex.Log(); }

                return null;
            }

            public static string GetCustomFlagName(Enum flag, object target) {
                try {
                    if(target is NetLaneProps.Prop prop && flag is NetLaneExt.Flags laneFlag) {
                        var netInfo = prop.GetParent(laneIndex: out int laneIndex, out _);
                        return netInfo?.GetMetaData()?.GetCustomLaneFlagName(laneFlag, laneIndex);
                    } else {
                        var netInfo =
                            (target as NetInfo.Node)?.GetParent(out _) ??
                            (target as NetInfo.Segment)?.GetParent(out _) ??
                            (target as NetLaneProps.Prop)?.GetParent(out _, out _);
                        var dict = netInfo?.GetMetaData()?.CustomFlagNames;
                        if(dict != null && dict.TryGetValue(flag, out string name)) {
                            return name;
                        }
                    }
                } catch(Exception ex) { ex.Log(); }
                return null;
            }

            public static event Action OnCustomFlagRenamed;
            public void RenameCustomFlag(Enum flag, string name) {
                try {
                    CustomFlagNames ??= new Dictionary<Enum, string>();
                    if(name.IsNullOrWhiteSpace() || name == flag.ToString())
                        CustomFlagNames.Remove(flag);
                    else
                        CustomFlagNames[flag] = name;
                    OnCustomFlagRenamed?.Invoke();
                } catch(Exception ex) { ex.Log(); }
            }

            public void RenameCustomFlag(int laneIndex, NetLaneExt.Flags flag, string name) {
                try {
                    Assertion.NotNull(Template, "Template");
                    var lane = Template.m_lanes[laneIndex];
                    Dictionary<NetLaneExt.Flags, string> dict = null;

                    CustomLaneFlagNames0 ??= new Dictionary<NetInfo.Lane, Dictionary<NetLaneExt.Flags, string>>();
                    if(!CustomLaneFlagNames0.TryGetValue(lane, out dict)) {
                        dict = CustomLaneFlagNames0[lane] = new Dictionary<NetLaneExt.Flags, string>();
                    }

                    if(name.IsNullOrWhiteSpace() || name == flag.ToString())
                        dict.Remove(flag);
                    else
                        dict[flag] = name;

                    OnCustomFlagRenamed?.Invoke();
                } catch(Exception ex) { ex.Log(); }
            }

            public static void RenameCustomFlag(Enum flag, object target, string name) {
                try {
                    if(target is NetLaneProps.Prop prop && flag is NetLaneExt.Flags laneFlag) {
                        var netInfo = prop.GetParent(laneIndex: out int laneIndex, out _);
                        netInfo.GetMetaData().RenameCustomFlag(laneIndex: laneIndex, flag: (NetLaneExt.Flags)flag, name: name);
                    } else {
                        var netInfo =
                            (target as NetInfo.Node)?.GetParent(out _) ??
                            (target as NetInfo.Segment)?.GetParent(out _) ??
                            (target as NetLaneProps.Prop)?.GetParent(out _, out _);
                        netInfo.GetMetaData().RenameCustomFlag(flag: flag, name: name);
                    }
                } catch(Exception ex) { ex.Log(); }
            }

            /// <summary>
            /// Gets the used lane flags for the input lane (not all the lanes of the NetInfo)
            /// </summary>
            public static NetLaneExt.Flags GetUsedCustomFlagsLane(NetInfo.Lane laneInfo) {
                NetLaneExt.Flags mask = 0;
                var props = (laneInfo.m_laneProps?.m_props).EmptyIfNull();
                foreach(var prop in props) {
                    var metadata = prop.GetMetaData();
                    if(metadata != null)
                        mask |= (metadata.LaneFlags.Required | metadata.LaneFlags.Forbidden);
                }
                return mask & NetLaneExt.Flags.CustomsMask;
            }
            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="netInfo"></param>
            public void Recalculate(NetInfo netInfo) {
                try {
                    Assertion.NotNull(netInfo, "netInfo");
                    Template = netInfo;
                    RecalculateTracks(netInfo);
                    RefreshLevelOfDetail(netInfo);
                    UsedCustomFlags = GatherUsedCustomFlags(netInfo);
                    RecalculateParkingAngle();
                    RecalculateConnectGroups(netInfo);
                } catch(Exception ex) { ex.Log(); }
            }

            public void RefreshLevelOfDetail(NetInfo netInfo) {
                if(Tracks != null) {
                    Log.Called();
                    var max = Mathf.Max(netInfo.m_halfWidth * 50f, (netInfo.m_maxHeight - netInfo.m_minHeight) * 80f);
                    // get lod render instance from already existing nodes/segments. if not found then calculate.
                    float lodRenderDistance =
                        netInfo.m_segments?.FirstOrDefault()?.m_lodRenderDistance ??
                        netInfo.m_nodes?.FirstOrDefault()?.m_lodRenderDistance ??
                        Mathf.Clamp(100f + RenderManager.LevelOfDetailFactor * max, 100f, 1000f);

                    bool lodMissing = false;
                    TrackLanes = 0;
                    for(int i = 0; i < Tracks.Length; i++) {
                        var track = Tracks[i];
                        bool hasLod = track.m_mesh;
                        if(hasLod) {
                            track.m_lodRenderDistance = lodRenderDistance;
                        } else {
                            track.m_lodRenderDistance = 100000f;
                            lodMissing = true;
                        }
                        netInfo.m_netLayers |= 1 << track.m_layer;
                        this.TrackLanes |= track.LaneIndeces;
                    }
                    if(lodMissing) {
                        CODebugBase<LogChannel>.Warn(LogChannel.Core, "LOD missing: " + netInfo.gameObject.name, netInfo.gameObject);
                    }
                }
            }
            void RecalculateTracks(NetInfo netInfo) {
                if(Tracks != null) {
                    // has color been already assigned in NetInfo.InitializePrefab() ?
                    bool hasColor = netInfo.m_segments?.Any(item => item.m_material) ?? false;
                    hasColor = hasColor || (netInfo.m_nodes?.Any(item => item.m_material) ?? false);
                    TrackLanes = 0;
                    for(int i = 0; i < Tracks.Length; i++) {
                        var track = Tracks[i];
                        track.Recalculate(netInfo);
                        track.CachedArrayIndex = i;
                        bool hasLod = track.m_mesh;
                        if(!hasColor && track.m_material != null) {
                            netInfo.m_color = track.m_material.color;
                            hasColor = true;
                        }
                        netInfo.m_netLayers |= 1 << track.m_layer;
                        this.TrackLanes |= track.LaneIndeces;
                    }

                    TrackLaneCount = EnumBitMaskExtensions.CountOnes(TrackLanes);
                }
            }

            // NetInfo
            // Token: 0x06004ED3 RID: 20179 RVA: 0x00246F24 File Offset: 0x00245324
            [NonSerialized]
            public static NetInfo.Segment tempSegmentInfo_ = new NetInfo.Segment();
            public static NetInfo.Segment TempSegmentInfo(Track trackInfo) {
                tempSegmentInfo_.m_lodMesh = trackInfo.m_lodMesh;
                tempSegmentInfo_.m_lodMaterial = trackInfo.m_lodMaterial;
                tempSegmentInfo_.m_combinedLod = trackInfo.m_combinedLod;
                tempSegmentInfo_.m_preserveUVs = trackInfo.m_preserveUVs;
                tempSegmentInfo_.m_generateTangents = trackInfo.m_generateTangents;
                tempSegmentInfo_.m_layer = trackInfo.m_layer;
                return tempSegmentInfo_;
            }

            public void InitMeshData(Track trackInfo, Rect atlasRect, Texture2D rgbAtlas, Texture2D xysAtlas, Texture2D aprAtlas) {
                if(Log.VERBOSE) Log.Debug("InitMeshData() for trackInfo called");
                // work around private fields/methods.
                var tempSegment = TempSegmentInfo(trackInfo);
                Template.InitMeshData(TempSegmentInfo(trackInfo), atlasRect, rgbAtlas, xysAtlas, aprAtlas);
                trackInfo.m_combinedLod = tempSegment.m_combinedLod;
            }

            void RecalculateParkingAngle() {
                float sin = Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * ParkingAngleDegrees));
                if(sin >= Mathf.Sin(30))
                    OneOverSinOfParkingAngle = 1 / sin;
                else
                    OneOverSinOfParkingAngle = 1;
            }

            void RecalculateConnectGroups(NetInfo netInfo) {
                LogCalled();
                ConnectGroupsHash = ConnectGroups?.Select(item => item.GetHashCode()).ToArray();
                if(ConnectGroupsHash.IsNullorEmpty()) ConnectGroupsHash = null;

                foreach(var node in netInfo.m_nodes)
                    node.GetMetaData()?.Update();

                NodeConnectGroupsHash = GetNodeConnectGroupsHash(netInfo).ToArray();
                if(NodeConnectGroupsHash.IsNullorEmpty()) NodeConnectGroupsHash = null;

                var itemSource = ItemSource.GetOrCreate(typeof(NetInfo.ConnectGroup));
                foreach(var connectGroup in GetAllConnectGroups(netInfo))
                    itemSource.Add(connectGroup);
            }

            IEnumerable<int> GetNodeConnectGroupsHash(NetInfo netInfo) {
                foreach(var node in netInfo.m_nodes) {
                    var hashes = node.GetMetaData()?.ConnectGroupsHash;
                    if(hashes == null) continue;
                    foreach(int hash in hashes)
                        yield return hash;
                }
            }

            IEnumerable<string> GetAllConnectGroups(NetInfo netInfo) {
                if(ConnectGroups != null) {
                    foreach(var cg in ConnectGroups)
                        yield return cg;
                }

                foreach(var node in netInfo.m_nodes) {
                    var connectGroups = node.GetMetaData()?.ConnectGroups;
                    if(connectGroups == null) continue;
                    foreach(var cg in connectGroups)
                        yield return cg;
                }
            }
        }

        [AfterField(nameof(NetInfo.Segment.m_backwardForbidden))]
        [Serializable]
        [Optional(AR_MODE)]
        public class Segment : ICloneable, ISerializable {
            object ICloneable.Clone() => Clone();

            [AfterField(nameof(NetInfo.Segment.m_forwardForbidden))]
            [CustomizableProperty("Forward Extension")]
            public SegmentInfoFlags Forward;

            [CustomizableProperty("Backward Extension")]
            public SegmentInfoFlags Backward;

            [CustomizableProperty("Tail Node")]
            [Optional(SEGMENT_NODE)]
            public VanillaNodeInfoFlags VanillaTailtNode;

            [CustomizableProperty("Tail Node Extension")]
            [Optional(SEGMENT_NODE)]
            public NodeInfoFlags TailtNode;

            [CustomizableProperty("Head Node")]
            [Optional(SEGMENT_NODE)]
            public VanillaNodeInfoFlags VanillaHeadNode;

            [CustomizableProperty("Head Node Extension")]
            [Optional(SEGMENT_NODE)]
            public NodeInfoFlags HeadNode;

            [CustomizableProperty("Segment Tail")]
            [Optional(SEGMENT_SEGMENT_END)]
            public SegmentEndInfoFlags Tail;

            [CustomizableProperty("Segment Head")]
            [Optional(SEGMENT_SEGMENT_END)]
            public SegmentEndInfoFlags Head;

            public bool CheckEndFlags(
                    NetSegmentEnd.Flags tailFlags,
                    NetSegmentEnd.Flags headFlags,
                    NetNode.Flags tailNodeFlags,
                    NetNode.Flags headNodeFlags,
                    NetNodeExt.Flags tailNodeExtFlags,
                    NetNodeExt.Flags headNodeExtFlags) {
                return
                    Tail.CheckFlags(tailFlags) &
                    Head.CheckFlags(headFlags) &
                    VanillaTailtNode.CheckFlags(tailNodeFlags) &
                    VanillaHeadNode.CheckFlags(headNodeFlags) &
                    TailtNode.CheckFlags(tailNodeExtFlags) &
                    HeadNode.CheckFlags(headNodeExtFlags);
                ;
            }

            public bool CheckFlags(NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags tailFlags,
                    NetSegmentEnd.Flags headFlags,
                    NetNode.Flags tailNodeFlags,
                    NetNode.Flags headNodeFlags,
                    NetNodeExt.Flags tailNodeExtFlags,
                    NetNodeExt.Flags headNodeExtFlags,
                    bool turnAround) {
                if(!turnAround) {
                    return Forward.CheckFlags(flags) && CheckEndFlags(
                        tailFlags: tailFlags,
                        headFlags: headFlags,
                        tailNodeFlags: tailNodeFlags,
                        headNodeFlags: headNodeFlags,
                        tailNodeExtFlags: tailNodeExtFlags,
                        headNodeExtFlags: headNodeExtFlags);
                } else {
                    Helpers.Swap(ref tailFlags, ref headFlags);
                    Helpers.Swap(ref tailNodeFlags, ref headNodeFlags);
                    var ret = Backward.CheckFlags(flags) && CheckEndFlags(
                        tailFlags: tailFlags,
                        headFlags: headFlags,
                        tailNodeFlags: tailNodeFlags,
                        headNodeFlags: headNodeFlags,
                        tailNodeExtFlags: tailNodeExtFlags,
                        headNodeExtFlags: headNodeExtFlags);
                    return ret;
                }
            }

            public CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = Forward.UsedCustomFlags | Backward.UsedCustomFlags,
                SegmentEnd = Head.UsedCustomFlags | Tail.UsedCustomFlags,
            };

            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Segment() { }
            public Segment Clone() => this.ShalowClone();
            public Segment(NetInfo.Segment template) { }

            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.GetObjectFields(info, this);

            // deserialization
            public Segment(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.SetObjectFields(info, this);
            #endregion
        }

        [AfterField(nameof(NetInfo.Node.m_flagsForbidden))]
        [Serializable]
        [Optional(AR_MODE)]
        public class Node : ICloneable, ISerializable {
            public const string DC_GROUP_NAME = "Direct Connect";

            [CustomizableProperty("Node Extension")]
            public NodeInfoFlags NodeFlags;

            [CustomizableProperty("Segment End")]
            public SegmentEndInfoFlags SegmentEndFlags;

            [CustomizableProperty("Segment")]
            [Optional(NODE_SEGMENT)]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Extension")]
            [Optional(NODE_SEGMENT)]
            public SegmentInfoFlags SegmentFlags;

            [Hint("Apply the same flag requirements to target segment end")]
            [CustomizableProperty("Check target flags", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public bool CheckTargetFlags;

            public string[] ConnectGroups;

            [NonSerialized]
            public int[] ConnectGroupsHash;

            [Hint("used by other mods to decide how hide tracks/medians")]
            [CustomizableProperty("Lane Type", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public NetInfo.LaneType LaneType;

            [Hint("used by other mods to decide how hide tracks/medians")]
            [CustomizableProperty("Vehicle Type", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public VehicleInfo.VehicleType VehicleType;

            [Hint("tell DCR mod to manage this node")]
            [CustomizableProperty("Hide Broken Medians", DC_GROUP_NAME)]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public bool HideBrokenMedians = true;

            public bool CheckFlags(
                NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags,
                NetSegmentExt.Flags segmentFlags, NetSegment.Flags vanillaSegmentFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags);

            public CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                SegmentEnd = SegmentEndFlags.UsedCustomFlags,
                Node = NodeFlags.UsedCustomFlags,
            };

            public void Update() {
                ConnectGroupsHash = ConnectGroups?.Select(item => item.GetHashCode()).ToArray();
            }

            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Node() { }
            public Node(NetInfo.Node template) { }
            public Node Clone() => this.ShalowClone();
            object ICloneable.Clone() => Clone();
            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.GetObjectFields(info, this);

            // deserialization
            public Node(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.SetObjectFields(info, this);
            #endregion
        }

        [AfterField(nameof(NetLaneProps.Prop.m_endFlagsForbidden))]
        [Serializable]
        [Optional(AR_MODE)]
        public class LaneProp : ICloneable, ISerializable {
            #region serialization
            [Obsolete("only useful for the purpose of shallow clone and serialization", error: true)]
            public LaneProp() { }
            public LaneProp Clone() {
                var ret = this.ShalowClone();
                ret.ForwardSpeedLimit = ret.ForwardSpeedLimit?.ShalowClone();
                ret.BackwardSpeedLimit = ret.BackwardSpeedLimit?.ShalowClone();
                ret.LaneSpeedLimit = ret.LaneSpeedLimit?.ShalowClone();
                ret.SegmentCurve = ret.SegmentCurve?.ShalowClone();
                ret.LaneCurve = ret.LaneCurve?.ShalowClone();
                return ret;
            }

            object ICloneable.Clone() => Clone();
            public LaneProp(NetLaneProps.Prop template) { }

            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.GetObjectFields(info, this);

            // deserialization
            public LaneProp(SerializationInfo info, StreamingContext context) {
                SerializationUtil.SetObjectFields(info, this);

                // backward compatibility: SpeedLimit, AverageSpeedLimit
                SerializationUtil.SetObjectProperties(info, this);
            }

            [Obsolete("for backward compatibility only", error: true)]
            private Range SpeedLimit {
                set => LaneSpeedLimit = value;
            }

            [Obsolete("for backward compatibility only", error: true)]
            private Range AverageSpeedLimit {
                set => ForwardSpeedLimit = BackwardSpeedLimit = value;
            }

            #endregion

            [CustomizableProperty("Lane")]
            public LaneInfoFlags LaneFlags = new LaneInfoFlags();

            [CustomizableProperty("Tail Node Extension")]
            [Hint(LANE_HEAD_TAIL)]
            public NodeInfoFlags StartNodeFlags = new NodeInfoFlags();

            [CustomizableProperty("Head Node Extension")]
            [Hint(LANE_HEAD_TAIL)]
            public NodeInfoFlags EndNodeFlags = new NodeInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("Segment Tail")]
            [Optional(LANE_SEGMENT_END)]
            public SegmentEndInfoFlags SegmentStartFlags = new SegmentEndInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("Segment Head")]
            [Optional(LANE_SEGMENT_END)]
            public SegmentEndInfoFlags SegmentEndFlags = new SegmentEndInfoFlags();

            [CustomizableProperty("Segment")]
            [Optional(LANE_SEGMENT)]
            public VanillaSegmentInfoFlags VanillaSegmentFlags = new VanillaSegmentInfoFlags();

            [CustomizableProperty("Segment Extension")]
            [Optional(LANE_SEGMENT)]
            public SegmentInfoFlags SegmentFlags = new SegmentInfoFlags();

            [CustomizableProperty("Lane Speed Limit Range")]
            public Range LaneSpeedLimit; // null => N/A

            [Hint("Max speed limit of all forward lanes(considering LHT)")]
            [CustomizableProperty("Forward Lanes")]
            public Range ForwardSpeedLimit; // null => N/A

            [Hint("Max speed limit of all backward lanes(considering LHT)")]
            [CustomizableProperty("Backward Lanes")]
            public Range BackwardSpeedLimit; // null => N/A

            [CustomizableProperty("Lane Curve")]
            public Range LaneCurve;

            [CustomizableProperty("Segment Curve")]
            public Range SegmentCurve;

            /// <param name="laneSpeed">game speed</param>
            /// <param name="forwardSpeedLimit">game speed</param>
            /// <param name="backwardSpeedLimit">game speed</param>
            public bool Check(
                NetLaneExt.Flags laneFlags,
                NetSegmentExt.Flags segmentFlags,
                NetSegment.Flags vanillaSegmentFlags,
                NetNodeExt.Flags startNodeFlags, NetNodeExt.Flags endNodeFlags,
                NetSegmentEnd.Flags segmentStartFlags, NetSegmentEnd.Flags segmentEndFlags,
                float laneSpeed, float forwardSpeedLimit, float backwardSpeedLimit,
                float segmentCurve, float laneCurve) =>
                LaneFlags.CheckFlags(laneFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) &&
                VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags) &&
                SegmentStartFlags.CheckFlags(segmentStartFlags) &&
                SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                StartNodeFlags.CheckFlags(startNodeFlags) &&
                EndNodeFlags.CheckFlags(endNodeFlags) &&
                LaneSpeedLimit.CheckRange(laneSpeed) &&
                ForwardSpeedLimit.CheckRange(forwardSpeedLimit) &&
                BackwardSpeedLimit.CheckRange(backwardSpeedLimit) &&
                SegmentCurve.CheckRange(segmentCurve) &&
                LaneCurve.CheckRange(laneCurve);
            public CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                SegmentEnd = SegmentStartFlags.UsedCustomFlags | SegmentStartFlags.UsedCustomFlags,
                Lane = LaneFlags.UsedCustomFlags,
                Node = StartNodeFlags.UsedCustomFlags | EndNodeFlags.UsedCustomFlags
            };
        }

        [Serializable]
        public class Track : ICloneable, ISerializable {
            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Track() { }
            public Track Clone() => this.ShalowClone();
            object ICloneable.Clone() => this.Clone();
            public Track(NetInfo template) {
                Assertion.Assert(template, "template");
                var lanes = template.m_lanes;
                for(int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                    if(lanes[laneIndex].m_vehicleType.IsFlagSet(TRACK_VEHICLE_TYPES))
                        LaneIndeces |= 1ul << laneIndex;
                }
            }

            public void ReleaseModel() {
                UnityEngine.Object.Destroy(m_mesh);
                UnityEngine.Object.Destroy(m_lodMesh);
                AssetEditorRoadUtils.ReleaseMaterial(m_material);
                AssetEditorRoadUtils.ReleaseMaterial(m_lodMaterial);
            }

            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) {
                try {
                    var package = PackageManagerUtil.SavingPackage;
                    Assertion.NotNull(package, "package");
                    var fields = this.GetType().GetFields(ReflectionHelpers.COPYABLE).Where(field => !field.HasAttribute<NonSerializedAttribute>());
                    foreach(FieldInfo field in fields) {
                        var type = field.GetType();
                        object value = field.GetValue(this);
                        if(type == typeof(Vector3)) {
                            //Vector3Serializable v = (Vector3Serializable)field.GetValue(instance);
                            info.AddValue(field.Name, value, typeof(Vector3Serializable));
                        } else if(value is Mesh mesh) {
                            Log.Debug($"package.AddAsset mesh : {mesh} InMainThread={Helpers.InMainThread()}");
                            var asset = package.AddAsset(mesh.name, mesh, true);
                            info.AddValue(field.Name, asset.checksum);
                        } else if(value is Material material) {
                            Log.Debug($"package.AddAsset material : {material} InMainThread={Helpers.InMainThread()}");
                            var asset = package.AddAsset(material.name, material, true);
                            info.AddValue(field.Name, asset.checksum);
                        } else {
                            info.AddValue(field.Name, value, field.FieldType);
                        }
                    }
                } catch(Exception ex) {
                    ex.Log();
                }
            }

            // deserialization
            public Track(SerializationInfo info, StreamingContext context) {
                try {
                    var package = PackageManagerUtil.PersistencyPackage;
                    var sharing = LSMUtil.GetSharing();
                    Assertion.NotNull(package, "package");
                    foreach(SerializationEntry item in info) {
                        FieldInfo field = this.GetType().GetField(item.Name, ReflectionHelpers.COPYABLE);
                        if(field != null) {
                            object val;
                            if(field.FieldType == typeof(Mesh)) {
                                bool lod = field.Name.Contains("lod");
                                string checksum = item.Value as string;
                                val = LSMUtil.GetMesh(sharing, checksum, package, lod);
                            } else if(field.FieldType == typeof(Material)) {
                                bool lod = field.Name.Contains("lod");
                                string checksum = item.Value as string;
                                val = LSMUtil.GetMaterial(sharing, checksum, package, lod);
                            } else {
                                val = Convert.ChangeType(item.Value, field.FieldType);
                            }
                            field.SetValue(this, val);
                        }
                    }
                } catch(Exception ex) {
                    ex.Log();
                }
            }
            #endregion

            public void Recalculate(NetInfo netInfo) {
                try {
                    Assertion.Assert(Helpers.InMainThread(), "in main thread");
                    Log.Called(netInfo, $"inMainthread={Helpers.InMainThread()}");
                    this.ParentInfo = netInfo;
                    Assertion.NotNull(ParentInfo, "ParentInfo");
                    this.m_trackMesh = this.m_mesh;
                    if(this.m_mesh) {
                        float corner1 = netInfo.m_minHeight - netInfo.m_maxSlope * 64f - 10f;
                        float corner2 = netInfo.m_maxHeight + netInfo.m_maxSlope * 64f + 10f;
                        this.m_mesh.bounds = new Bounds(new Vector3(0f, (corner1 + corner2) * 0.5f, 0f), new Vector3(128f, corner2 - corner1, 128f));
                    }
                    string tag = this.m_material?.GetTag("NetType", searchFallbacks: false);
                    if(tag == "PowerLine") {
                        this.m_requireWindSpeed = true;
                        this.m_preserveUVs = true;
                        this.m_generateTangents = false;
                        this.m_layer = LayerMask.NameToLayer("PowerLines");
                    } else if(tag == "MetroTunnel") {
                        this.m_requireWindSpeed = false;
                        this.m_preserveUVs = false;
                        this.m_generateTangents = false;
                        this.m_layer = LayerMask.NameToLayer("MetroTunnels");
                    } else {
                        this.m_requireWindSpeed = false;
                        this.m_preserveUVs = false;
                        this.m_generateTangents = false;
                        this.m_layer = netInfo.m_prefabDataLayer;
                    }
                    if(this.m_material) {
                        this.m_trackMaterial = new Material(this.m_material);
                        this.m_trackMaterial.EnableKeyword("NET_SEGMENT");
                        Color color = this.m_material.color;
                        color.a = 0f;
                        this.m_trackMaterial.color = color;
                        Texture2D texture2D = this.m_material.mainTexture as Texture2D;
                        if(texture2D != null && texture2D.format == TextureFormat.DXT5) {
                            CODebugBase<LogChannel>.Warn(LogChannel.Core, "Segment diffuse is DXT5: " + netInfo.gameObject.name, netInfo.gameObject);
                        }
                    } else {
                        m_trackMaterial = null;
                    }
                    LaneCount = EnumBitMaskExtensions.CountOnes(LaneIndeces);
                } catch(Exception ex) { ex.Log(); }
            }

            public const VehicleInfo.VehicleType TRACK_VEHICLE_TYPES =
                VehicleInfo.VehicleType.Tram |
                VehicleInfo.VehicleType.Metro |
                VehicleInfo.VehicleType.Train |
                VehicleInfo.VehicleType.Monorail |
                VehicleInfo.VehicleType.Trolleybus | VehicleInfo.VehicleType.TrolleybusLeftPole | VehicleInfo.VehicleType.TrolleybusRightPole;

            #region materials
            public Mesh m_mesh;

            public Mesh m_lodMesh;

            public Material m_material;

            public Material m_lodMaterial;

            [NonSerialized]
            public NetInfo.LodValue m_combinedLod;

            [NonSerialized]
            public Mesh m_trackMesh;

            [NonSerialized]
            public Material m_trackMaterial;
            #endregion

            [NonSerialized]
            public NetInfo ParentInfo;

            [NonSerialized]
            public int CachedArrayIndex;

            [NonSerialized]
            public float m_lodRenderDistance;

            //[NonSerialized]
            //public bool m_requireSurfaceMaps; // terrain network

            //[NonSerialized]
            //public bool m_requireHeightMap; // fence

            [NonSerialized]
            public bool m_requireWindSpeed;

            [NonSerialized]
            public bool m_preserveUVs;

            [NonSerialized]
            public bool m_generateTangents;

            [NonSerialized]
            public int m_layer;
            [CustomizableProperty("Vertical Offset")]
            public float VerticalOffset;

            [CustomizableProperty("Anti-flickering")]
            [Hint("moves the tracks up and down by a random few millimeters to avoid z-fighting")]
            public bool AntiFlickering;

            [CustomizableProperty("Scale to lane width")]
            [Hint("the width of the rendered mesh is proportional to the width of the lane.\n" +
                "1 unit in blender means the mesh will be as wide as the lane")]
            public bool ScaleToLaneWidth;


            //[CustomizableProperty("Low Priority")]
            [Hint("Other tracks with DC node take priority")]
            public bool IgnoreDC;

            [CustomizableProperty("Lanes to apply to")]
            [BitMaskLanes]
            public UInt64 LaneIndeces;

            [NonSerialized]
            public int LaneCount;

            public bool HasTrackLane(int laneIndex) => ((1ul << laneIndex) & LaneIndeces) != 0;

            #region flags
            [CustomizableProperty("Render On Segments")]
            public bool RenderSegment = true;

            [CustomizableProperty("Render On Junctions")]
            public bool RenderJunction = true;

            [CustomizableProperty("Segment")]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Extension")]
            [Hint("checked on segments.\n" +
                "flags for source segment is also checked on nodes.")]
            public SegmentInfoFlags SegmentFlags;

            [CustomizableProperty("Node")]
            public VanillaNodeInfoFlags VanillaNodeFlags;

            [CustomizableProperty("Node Extension")]
            [Hint("Only checked on nodes (including bend nodes)")]
            public NodeInfoFlags NodeFlags;

            public bool CheckNodeFlags
                (NetNodeExt.Flags nodeFlags, NetNode.Flags vanillaNodeFlags,
                NetSegmentExt.Flags sourceSegmentFlags, NetSegment.Flags startVanillaSegmentFlags) =>
                RenderJunction && NodeFlags.CheckFlags(nodeFlags) && VanillaNodeFlags.CheckFlags(vanillaNodeFlags) &&
                SegmentFlags.CheckFlags(sourceSegmentFlags) && VanillaSegmentFlags.CheckFlags(startVanillaSegmentFlags);


            public bool CheckSegmentFlags(NetSegmentExt.Flags segmentFlags, NetSegment.Flags vanillaSegmentFlags) =>
                RenderSegment && SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags);

            public CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                Node = NodeFlags.UsedCustomFlags,
            };
            #endregion

        }
        #endregion

        #region static
        private static IEnumerable<NetLaneProps.Prop> IterateProps(NetInfo.Lane lane)
        => lane?.m_laneProps?.m_props ?? Enumerable.Empty<NetLaneProps.Prop>();

        private static IEnumerable<NetLaneProps.Prop> IterateProps(NetInfo info) {
            foreach(var lane in info?.m_lanes ?? Enumerable.Empty<NetInfo.Lane>()) {
                foreach(var prop in IterateProps(lane))
                    yield return prop;
            }
        }

        public static bool IsAdaptive(this NetInfo info) {
            return info?.GetMetaData() != null;
            //Assertion.AssertNotNull(info);
            //if(info.GetMetaData() != null)
            //    return true;
            //foreach(var item in info.m_nodes) {
            //    if(item.GetMetaData() != null)
            //        return true;
            //}
            //foreach(var item in info.m_segments) {
            //    if(item.GetMetaData() != null)
            //        return true;
            //}
            //foreach(var item in IterateProps(info)) {
            //    if(item.GetMetaData() != null)
            //        return true;
            //}
            //return false;
        }

        public static NetInfo EditedNetInfo =>
            ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;

        public static IEnumerable<NetInfo> EditedNetInfos =>
            AllElevations(EditedNetInfo);

        public static IEnumerable<NetInfo> AllElevations(this NetInfo ground) {
            if(ground == null) yield break;

            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

            yield return ground;
            if(elevated != null) yield return elevated;
            if(bridge != null) yield return bridge;
            if(slope != null) yield return slope;
            if(tunnel != null) yield return tunnel;
        }

        public static void InvokeEditPrefabChanged() {
            // invoke eventEditPrefabChanged
            SimulationManager.instance.m_ThreadingWrapper.QueueMainThread(delegate () {
                var tc = ToolsModifierControl.toolController;
                ReflectionHelpers.EventToDelegate<ToolController.EditPrefabChanged>(
                    tc, nameof(tc.eventEditPrefabChanged))
                    ?.Invoke(tc.m_editPrefabInfo);
            });
        }


        const NetNode.Flags vanillaNode = NetNode.Flags.Sewage | NetNode.Flags.Deleted;
        const NetSegment.Flags vanillaSegment = NetSegment.Flags.AccessFailed | NetSegment.Flags.Deleted;
        const NetLane.Flags vanillaLane = NetLane.Flags.Created | NetLane.Flags.Deleted;

        public static void ApplyVanillaForbidden(this NetInfo info) {
            foreach(var node in info.m_nodes) {
                if(node.GetMetaData() is Node metadata) {
                    bool vanillaForbidden = metadata.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.NodeFlags.Forbidden.IsFlagSet(NetNodeExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.SegmentEndFlags.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    if(vanillaForbidden)
                        node.m_flagsRequired |= vanillaNode;
                }
            }
            foreach(var segment in info.m_segments) {
                if(segment.GetMetaData() is Segment metadata) {
                    bool forwardVanillaForbidden = metadata.Forward.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    bool backwardVanillaForbidden = metadata.Backward.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    bool vanillaForbidden = metadata.Head.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    vanillaForbidden |= metadata.Tail.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    forwardVanillaForbidden |= vanillaForbidden;
                    backwardVanillaForbidden |= vanillaForbidden;

                    if(forwardVanillaForbidden)
                        segment.m_forwardRequired |= vanillaSegment;
                    if(backwardVanillaForbidden)
                        segment.m_backwardRequired |= vanillaSegment;
                }
            }
            foreach(var prop in IterateProps(info)) {
                if(prop.GetMetaData() is LaneProp metadata) {
                    bool vanillaForbidden = metadata.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.LaneFlags.Forbidden.IsFlagSet(NetLaneExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.StartNodeFlags.Forbidden.IsFlagSet(NetNodeExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.EndNodeFlags.Forbidden.IsFlagSet(NetNodeExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.SegmentStartFlags.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    vanillaForbidden |= metadata.SegmentEndFlags.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);

                    if(vanillaForbidden)
                        prop.m_flagsRequired |= vanillaLane;
                }
            }
        }

        public static void UndoVanillaForbidden(this NetInfo info) {
            foreach(var node in info.m_nodes) {
                if(node.m_flagsRequired.CheckFlags(vanillaNode))
                    node.m_flagsRequired &= ~vanillaNode;
            }
            foreach(var segment in info.m_segments) {
                if(segment.m_forwardRequired.CheckFlags(vanillaSegment))
                    segment.m_forwardRequired &= ~vanillaSegment;
                if(segment.m_backwardRequired.CheckFlags(vanillaSegment))
                    segment.m_backwardRequired &= ~vanillaSegment;
            }
            foreach(var prop in IterateProps(info)) {
                if(prop.m_flagsRequired.CheckFlags(vanillaLane))
                    prop.m_flagsRequired &= ~vanillaLane;
            }
        }

        public static void EnsureExtended(this NetInfo netInfo) {
            try {
                Assertion.Assert(netInfo);
                Log.Debug($"EnsureExtended({netInfo}): called "/* + Environment.StackTrace*/);
                for(int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if(!(netInfo.m_nodes[i] is IInfoExtended))
                        netInfo.m_nodes[i] = netInfo.m_nodes[i].Extend() as NetInfo.Node;
                }
                for(int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if(!(netInfo.m_segments[i] is IInfoExtended))
                        netInfo.m_segments[i] = netInfo.m_segments[i].Extend() as NetInfo.Segment;
                }
                foreach(var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    int n = props?.Length ?? 0;
                    for(int i = 0; i < n; ++i) {
                        if(!(props[i] is IInfoExtended))
                            props[i] = props[i].Extend() as NetLaneProps.Prop;
                    }
                }
                netInfo.GetOrCreateMetaData();

                Log.Debug($"EnsureExtended({netInfo}): successful");
            } catch(Exception e) {
                Log.Exception(e);
            }
        }

        public static void UndoExtend(this NetInfo netInfo) {
            try {
                Log.Debug($"UndoExtend({netInfo}): called");
                for(int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if(netInfo.m_nodes[i] is IInfoExtended<NetInfo.Node> ext)
                        netInfo.m_nodes[i] = ext.UndoExtend();
                }
                for(int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if(netInfo.m_segments[i] is IInfoExtended<NetInfo.Segment> ext)
                        netInfo.m_segments[i] = ext.UndoExtend();
                }
                foreach(var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps.m_props;
                    int n = props?.Length ?? 0;
                    for(int i = 0; i < n; ++i) {
                        if(props[i] is IInfoExtended<NetLaneProps.Prop> ext)
                            props[i] = ext.UndoExtend();
                    }
                }
                netInfo.RemoveMetadataContainer();

            } catch(Exception e) {
                Log.Exception(e);
            }
        }

        public static void Ensure_EditedNetInfos() {
            LogCalled();
            if(VanillaMode) {
                UndoExtend_EditedNetInfos();
            } else {
                EnsureExtended_EditedNetInfos();
            }
        }

        public static void EnsureExtended_EditedNetInfos() {
            if(VanillaMode) {
                Log.Debug($"EnsureExtended_EditedNetInfos() because we are in vanilla mode");
                return;
            }
            Log.Debug($"EnsureExtended_EditedNetInfos() was called");
            foreach(var info in EditedNetInfos)
                EnsureExtended(info);
            Log.Debug($"EnsureExtended_EditedNetInfos() was successful");
        }

        public static void UndoExtend_EditedNetInfos() {
            Log.Debug($"UndoExtend_EditedNetInfos() was called");
            foreach(var info in EditedNetInfos)
                UndoExtend(info);
            Verify_UndoExtend();
            Log.Debug($"UndoExtend_EditedNetInfos() was successful");
        }

        public static void Verify_UndoExtend() {
            //test if UndoExtend_EditedNetInfos was successful.
            foreach(var netInfo in NetInfoExtionsion.EditedNetInfos) {
                for(int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if(!(netInfo.m_nodes[i].GetType() == typeof(NetInfo.Node)))
                        throw new Exception($"reversal unsuccessful. nodes[{i}]={netInfo.m_nodes[i]}");
                }
                for(int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if(!(netInfo.m_segments[i].GetType() == typeof(NetInfo.Segment)))
                        throw new Exception($"reversal unsuccessful. segments[{i}]={netInfo.m_segments[i]}");
                }
                foreach(var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    int n = props?.Length ?? 0;
                    for(int i = 0; i < n; ++i) {
                        if(!(props[i].GetType() == typeof(NetLaneProps.Prop)))
                            throw new Exception($"reversal unsuccessful. props[{i}]={props[i]}");
                    }
                }
            }
        }
        #endregion
    }
}
