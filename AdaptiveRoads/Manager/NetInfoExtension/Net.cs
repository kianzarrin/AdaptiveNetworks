namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.CustomScript;
    using AdaptiveRoads.Data;
    using AdaptiveRoads.UI;
    using AdaptiveRoads.UI.RoadEditor.Bitmask;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using KianCommons;
    using KianCommons.Serialization;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using UnityEngine;
    using static AdaptiveRoads.Manager.NetInfoExtionsion;
    using static AdaptiveRoads.UI.ModSettings;
    using static KianCommons.ReflectionHelpers;

    public static partial class NetInfoExtionsion {
        [Serializable]
        [Optional(AR_MODE)]
        public class Net : IMetaData {
            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Net() { }
            public Net Clone() {
                try {
                    var ret = this.ShalowClone();
                    ret.ConnectGroups = ret.ConnectGroups?.ToArray();
                    ret.ConnectGroupsHash = ret.ConnectGroupsHash?.ToArray();
                    ret.NodeConnectGroupsHash = ret.NodeConnectGroupsHash?.ToArray();
                    ret.QuayRoadsProfile = QuayRoadsProfile?.ToArray();
                    ret.CustomFlagNames = ret.CustomFlagNames?.ShallowClone();
                    ret.ScriptedFlags = ret.ScriptedFlags?.ShallowClone();
                    ret.CustomLaneFlagNames0 = ret.CustomLaneFlagNames0?.ShallowClone();
                    //Log.Debug($"CustomLaneFlagNames={CustomLaneFlagNames} before cloning");
                    ret.CustomLaneFlagNames = ret.CustomLaneFlagNames
                        ?.Select(item => item?.ShallowClone())
                        ?.ToArray();
                    ret.UserDataNamesSet = ret.UserDataNamesSet?.Clone();
                    //Log.Debug($"CustomLaneFlagNames={CustomLaneFlagNames} after cloning");
                    for (int i = 0; i < ret.Tracks.Length; ++i) {
                        ret.Tracks[i] = ret.Tracks[i].Clone();
                    }
                    return ret;
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            object ICloneable.Clone() => Clone();
            public Net(NetInfo template) {
                try {
                    Log.Called(template);
                    PavementWidthRight = template.m_pavementWidth;
                    UsedCustomFlags = GatherUsedCustomFlags(template);
                    ParentInfo = template;
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            public void ReleaseModels() {
                try {
                    foreach (var track in Tracks) {
                        track.ReleaseModel();
                    }
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) {
                try {
                    //Log.Called();
                    FillCustomLaneFlagNames();
                    RecalculateLaneTags();
                    SerializationUtil.GetObjectFields(info, this);
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            // deserialization
            public Net(SerializationInfo info, StreamingContext context) {
                try {
                    //Log.Called();
                    SerializationUtil.SetObjectFields(info, this);
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }
            #endregion

            [NonSerialized]
            public NetInfo ParentInfo;

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

            [AfterField(nameof(NetInfo.m_pavementWidth))]
            [CustomizableProperty("Catenary Height", "Properties")]
            [Hint("Catenaries and wires are shifted based on super elevation and height")]
            public float CatenaryHeight = 2.5f;

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

            [AfterField(nameof(RoadBaseAI.m_highwayRules))]
            [CustomizableProperty("Road Rules", "Properties")]
            [Hint("two segment junction with matching direct connect does not have \n" +
                  "u-turn or pedestrian crossing and cars can go through.")]
            public bool RoadRules;

            #region UserData
            public UserDataNamesSet UserDataNamesSet;
            public void RemoveSegmentUserValue(int i) {
                try {
                    Log.Called(i);
                    UserDataNamesSet?.Segment?.RemoveValueAt(i);
                    foreach (var segment in ParentInfo.m_segments) {
                        segment?.GetMetaData()?.UserData?.RemoveValueAt(i);
                    }
                    foreach (var node in ParentInfo.m_nodes) {
                        node?.GetMetaData()?.SegmentUserData?.RemoveValueAt(i);
                    }
                    for (ushort segmentId = 1; segmentId < NetManager.MAX_SEGMENT_COUNT; ++segmentId) {
                        ref NetSegment segment = ref segmentId.ToSegment();
                        if (segment.IsValid() && segment.Info == ParentInfo) {
                            segmentId.ToSegmentExt().UserData.RemoveValueAt(i);
                        }
                    }
                    AllocateUserData();
                } catch (Exception ex) {
                    ex.Log();
                }
            }

            public void AllocateMetadata() {
                try {
                    Log.Called();
                    Assertion.Assert(ParentInfo.GetMetaData() == this, $"ParentInfo={ParentInfo}");
                    foreach (var segment in ParentInfo.m_segments)
                        segment?.GetOrCreateMetaData();
                    foreach (var node in ParentInfo.m_nodes)
                        node?.GetOrCreateMetaData();
                    foreach (var lane in ParentInfo.m_lanes) {
                        var props = lane.m_laneProps?.m_props;
                        if (props == null) continue;
                        foreach (var prop in props)
                            prop?.GetOrCreateMetaData();
                    }
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            /// <summary>call only for editing prefab</summary>
            public void AllocateUserData() {
                try {
                    Log.Called();
                    UserDataNamesSet ??= new();
                    Assertion.Assert(ParentInfo.GetMetaData() == this, $"ParentInfo={ParentInfo}");
                    foreach (var segment in ParentInfo.m_segments) {
                        var segmentMetadata = segment.GetMetaData();
                        Assertion.NotNull(segmentMetadata, "segmentMetadata");
                        segmentMetadata.AllocateUserData(UserDataNamesSet?.Segment);
                    }
                    foreach (var node in ParentInfo.m_nodes) {
                        var nodeMetadata = node.GetMetaData();
                        Assertion.NotNull(nodeMetadata, "nodeMetadata");
                        nodeMetadata.AllocateUserData(UserDataNamesSet?.Segment);
                    }
                    foreach (var track in Tracks) {
                        track.AllocateUserData(UserDataNamesSet?.Segment);
                    }
                    foreach (var lane in ParentInfo.m_lanes) {
                        foreach (var prop in lane.IterateProps()) {
                            var propMetadata = prop.GetMetaData();
                            Assertion.NotNull(propMetadata, "nodeMetadata");
                            propMetadata.AllocateUserData(UserDataNamesSet?.Segment);
                        }
                    }

                    for (ushort segmentId = 1; segmentId < NetManager.MAX_SEGMENT_COUNT; ++segmentId) {
                        ref NetSegment segment = ref segmentId.ToSegment();
                        if (segment.IsValid() && segment.Info == ParentInfo) {
                            segmentId.ToSegmentExt().UserData.Allocate(UserDataNamesSet?.Segment);
                        }
                    }


                } catch (Exception ex) {
                    ex.Log();
                }
            }

            /// <summary>call in game.</summary>
            public void OptimizeUserData() {
                try {
                    Log.Called();
                    foreach (var segment in ParentInfo.m_segments) {
                        segment.GetMetaData()?.OptimizeUserData();
                    }
                    foreach (var node in ParentInfo.m_nodes) {
                        node.GetMetaData()?.OptimizeUserData();
                    }
                    foreach (var track in Tracks) {
                        track.OptimizeUserData();
                    }
                    foreach (var lane in ParentInfo.m_lanes) {
                        foreach (var prop in lane.IterateProps()) {
                            prop.GetMetaData()?.OptimizeUserData();
                        }
                    }
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            #endregion

            #region Custom Flags
            [NonSerialized]
            public CustomFlags UsedCustomFlags;

            public Dictionary<Enum, string> CustomFlagNames = new Dictionary<Enum, string>();

            public Dictionary<Enum, ExpressionWrapper> ScriptedFlags = new Dictionary<Enum, ExpressionWrapper>();

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

            [NonSerialized]
            public bool HasTitlableTracks;

            public bool HasTrackLane(int laneIndex) => ((1ul << laneIndex) & TrackLanes) != 0;

            static CustomFlags GatherUsedCustomFlags(NetInfo info) {
                try {
                    var ret = CustomFlags.None;
                    foreach (var item in info.m_nodes) {
                        if (item.GetMetaData() is Node metaData)
                            ret |= metaData.UsedCustomFlags;
                    }

                    foreach (var item in info.m_segments) {
                        if (item.GetMetaData() is Segment metaData)
                            ret |= metaData.UsedCustomFlags;
                    }

                    foreach (var lane in info.m_lanes) {
                        var props = lane.m_laneProps?.m_props;
                        if (props.IsNullorEmpty()) continue;
                        foreach (var item in props) {
                            if (item.GetMetaData() is LaneProp metaData)
                                ret |= metaData.UsedCustomFlags;
                        }
                    }

                    foreach (var track in info.GetMetaData()?.Tracks ?? Enumerable.Empty<Track>()) {
                        ret |= track.UsedCustomFlags;
                    }

                    return ret;
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            private void FillCustomLaneFlagNames() {
                try {
                    //Log.Called();
                    CustomLaneFlagNames = null;
                    if (CustomLaneFlagNames0.IsNullorEmpty()) return;
                    CustomLaneFlagNames = new Dictionary<NetLaneExt.Flags, string>[ParentInfo.m_lanes.Length];
                    for (int laneIndex = 0; laneIndex < CustomLaneFlagNames.Length; ++laneIndex) {
                        var lane = ParentInfo.m_lanes[laneIndex];
                        if (CustomLaneFlagNames0.TryGetValue(lane, out var dict)) {
                            CustomLaneFlagNames[laneIndex] = dict;
                        }
                    }
                    //Log.Succeeded($"CustomLaneFlagNames0={CustomLaneFlagNames0.ToSTR()} CustomLaneFlagNames={CustomLaneFlagNames}.ToSTR()");
                } catch (Exception ex) { ex.Log(); }
            }

            private void FillCustomLaneFlagNames0() {
                try {
                    //Log.Called();
                    if (!CustomLaneFlagNames0.IsNullorEmpty()) {
                        // already filled.
                        return;
                    }
                    if (CustomLaneFlagNames.IsNullorEmpty()) {
                        // no custom lane flag names exist
                        CustomLaneFlagNames0 = null;
                        return;
                    }
                    CustomLaneFlagNames0 = new Dictionary<NetInfo.Lane, Dictionary<NetLaneExt.Flags, string>>();
                    for (int laneIndex = 0; laneIndex < CustomLaneFlagNames.Length; ++laneIndex) {
                        var lane = ParentInfo.m_lanes[laneIndex];
                        var dict = CustomLaneFlagNames[laneIndex];
                        if (!dict.IsNullorEmpty()) {
                            CustomLaneFlagNames0[lane] = dict;
                        }
                    }
                    //Log.Succeeded($"CustomLaneFlagNames0={CustomLaneFlagNames0.ToSTR()} CustomLaneFlagNames={CustomLaneFlagNames}.ToSTR()");
                } catch (Exception ex) { ex.Log(); }
            }

            public Dictionary<NetLaneExt.Flags, string> GetCustomLaneFlags(int laneIndex) {
                try {
                    //Log.Called();
                    //Log.Debug($"CustomLaneFlagNames0={CustomLaneFlagNames0.ToSTR()} CustomLaneFlagNames={CustomLaneFlagNames.ToSTR()}");
                    if (CustomLaneFlagNames0 is not null) {
                        // edit prefab
                        var lane = ParentInfo.m_lanes[laneIndex];
                        return CustomLaneFlagNames0.GetorDefault(lane);
                    } else if (CustomLaneFlagNames is not null) {
                        // normal
                        Assertion.InRange(CustomLaneFlagNames, laneIndex);
                        return CustomLaneFlagNames[laneIndex];
                    }
                } catch (Exception ex) {
                    ex.Log();
                }
                return null;
            }

            public string GetCustomLaneFlagName(NetLaneExt.Flags flag, int laneIndex) =>
                GetCustomLaneFlags(laneIndex)?.GetorDefault(flag);

            public static string GetCustomFlagName(Enum flag, object target) {
                try {
                    if (flag is NetLaneExt.Flags laneFlag) {
                        if (target is NetLaneProps.Prop prop) {
                            var netInfo = prop.GetParent(laneIndex: out int laneIndex, out _);
                            return netInfo?.GetMetaData()?.GetCustomLaneFlagName(laneFlag, laneIndex);
                        } else if (target is Track track) {
                            var netInfo = track.ParentInfo;
                            for (int laneIndex = 0; laneIndex < netInfo.m_lanes.Length; ++laneIndex) {
                                if (track.HasTrackLane(laneIndex)) {
                                    string name = netInfo?.GetMetaData()?.GetCustomLaneFlagName(laneFlag, laneIndex);
                                    if (!name.IsNullorEmpty()) return name;
                                }
                            }
                            return null;
                        } else {
                            throw new NotImplementedException($"GetCustomFlagName({flag}, {target})");
                        }
                    } else {
                        var netInfo = RoadEditorUtils.GetSelectedNetInfo(out _);
                        //  (target as NetInfo.Node)?.GetParent(out _) ??
                        //  (target as NetInfo.Segment)?.GetParent(out _) ??
                        //  (target as NetLaneProps.Prop)?.GetParent(out _, out _);
                        var dict = netInfo?.GetMetaData()?.CustomFlagNames;
                        if (dict != null && dict.TryGetValue(flag, out string name)) {
                            return name;
                        }
                    }
                } catch (Exception ex) { ex.Log(); }
                return null;
            }

            public static ExpressionWrapper GetExpression(Enum flag, object target) {
                try {
                    var netInfo = RoadEditorUtils.GetSelectedNetInfo(out _);
                    var dict = netInfo?.GetMetaData()?.ScriptedFlags;
                    if (dict != null && dict.TryGetValue(flag, out var exp)) {
                        return exp;
                    }
                } catch (Exception ex) { ex.Log(); }
                return null;
            }

            public static void AssignCSScript(Enum flag, object target, string name, string path) {
                try {
                    Log.Called(flag, target, name, path);
                    path = path/*?.Replace('\\', '/')*/?.RemoveChars('"', '\'')?.Trim();
                    Log.Debug("path=" + path);
                    FileInfo file = null;
                    if (!path.IsNullorEmpty()) {
                        file = new FileInfo(path);
                        if (!file.Exists) {
                            throw new Exception($"{path} does not exists");
                        }
                    }
                    var netInfo = RoadEditorUtils.GetSelectedNetInfo(out _);
                    netInfo.GetMetaData().AssignCSScript(flag: flag, name: name, file: file);
                } catch (Exception ex) { ex.Log(); }
            }

            public void AssignCSScript(Enum flag, string name, FileInfo file) {
                try {
                    Log.Called(flag, name, file);
                    if (file == null)
                        ScriptedFlags.Remove(flag);
                    else {
                        FileInfo dllFile = null;
                        if (file.Extension == ".cs") {
                            if (!ScriptCompiler.CompileSource(file, out dllFile)) {
                                throw new Exception("failed to compile " + file);
                            }
                        } else if (file.Extension == ".dll") {
                            dllFile = file;
                        } else {
                            throw new Exception($"File type not recognized : " + file);
                        }
                        ScriptedFlags[flag] = new ExpressionWrapper(dllFile, name);
                    }
                    OnCustomFlagRenamed?.Invoke();
                    NetworkExtensionManager.Instance.RecalculateARPrefabs();
                    NetworkExtensionManager.Instance.UpdateAllNetworkFlags();
                    Log.Debug(ScriptedFlags.ToSTR());
                } catch (Exception ex) { ex.Log(); }
            }

            public static event Action OnCustomFlagRenamed; // TODO move out
            void RenameCustomFlag(Enum flag, string name) {
                try {
                    CustomFlagNames ??= new Dictionary<Enum, string>();
                    if (name.IsNullOrWhiteSpace() || name == flag.ToString())
                        CustomFlagNames.Remove(flag);
                    else
                        CustomFlagNames[flag] = name;
                    OnCustomFlagRenamed?.Invoke();
                } catch (Exception ex) { ex.Log(); }
            }

            void RenameCustomFlag(int laneIndex, NetLaneExt.Flags flag, string name) {
                try {
                    Assertion.NotNull(ParentInfo, "Template");
                    NetInfo.Lane lane = ParentInfo.m_lanes[laneIndex];
                    Dictionary<NetLaneExt.Flags, string> dict;

                    CustomLaneFlagNames0 ??= new Dictionary<NetInfo.Lane, Dictionary<NetLaneExt.Flags, string>>();
                    if (!CustomLaneFlagNames0.TryGetValue(lane, out dict)) {
                        dict = CustomLaneFlagNames0[lane] = new Dictionary<NetLaneExt.Flags, string>();
                    }

                    if (name.IsNullOrWhiteSpace() || name == flag.ToString())
                        dict.Remove(flag);
                    else
                        dict[flag] = name;

                    OnCustomFlagRenamed?.Invoke();
                } catch (Exception ex) { ex.Log(); }
            }

            public static void RenameCustomFlag(Enum flag, object target, string name) {
                try {
                    if (flag is NetLaneExt.Flags laneFlag) {
                        if (target is NetLaneProps.Prop prop) {
                            var netInfo = prop.GetParent(laneIndex: out int laneIndex, out _);
                            netInfo.GetMetaData().RenameCustomFlag(laneIndex: laneIndex, flag: laneFlag, name: name);
                        } else if (target is Track track) {
                            var netInfo = track.ParentInfo;
                            for (int laneIndex = 0; laneIndex < netInfo.m_lanes.Length; ++laneIndex) {
                                if (track.HasTrackLane(laneIndex)) {
                                    netInfo.GetMetaData().RenameCustomFlag(laneIndex: laneIndex, flag: laneFlag, name: name);
                                }
                            }
                        } else {
                            throw new NotImplementedException($"RenameCustomFlag({flag}, {target}, {name})");
                        }
                    } else {
                        var netInfo = RoadEditorUtils.GetSelectedNetInfo(out _);
                        //var netInfo =
                        //    (target as NetInfo.Node)?.GetParent(out _) ??
                        //    (target as NetInfo.Segment)?.GetParent(out _) ??
                        //    (target as NetLaneProps.Prop)?.GetParent(out _, out _);
                        netInfo.GetMetaData().RenameCustomFlag(flag: flag, name: name);
                    }
                } catch (Exception ex) { ex.Log(); }
            }

            public NetLaneExt.Flags GetUsedCustomFlagsLane(int laneIndex) {
                NetLaneExt.Flags ret = default;
                try {
                    var props = ParentInfo.m_lanes[laneIndex]?.m_laneProps?.m_props;
                    if (props != null) {
                        foreach (var prop in props) {
                            if (prop.GetMetaData() is LaneProp propExt) {
                                ret |= propExt.UsedCustomFlags.Lane;
                            }
                        }
                    }
                    foreach (var track in Tracks) {
                        if (track.HasTrackLane(laneIndex)) {
                            ret |= track.UsedCustomFlags.Lane;
                        }
                    }
                } catch (Exception ex) { ex.Log(); }
                return ret;
            }
            #endregion

            #region lane tags
            [NonSerialized]
            public Dictionary<NetInfo.Lane, LaneTagsT> LaneTags0;

            public LaneTagsT[] LaneTags;

            public void RecalculateLaneTags() {
                var lanes = ParentInfo.m_lanes;
                if (lanes == null || LaneTags0 == null) {
                    LaneTags = new LaneTagsT[0];
                    return;
                }

                List<LaneTagsT> laneTags = new(lanes.Length);
                foreach (var lane in  lanes) {
                    if(LaneTags0.TryGetValue(lane, out var tags)) {
                        tags.Recalculate();
                        laneTags.Add(tags);
                    }
                }
                LaneTags = laneTags.ToArray();
            }

            public void RecalculateLaneTags0() {
                var lanes = ParentInfo.m_lanes;
                LaneTags0 = new();
                if (lanes == null || LaneTags == null) {
                    return;
                }

                for (int laneIndex = 0; laneIndex < LaneTags.Length; ++laneIndex) {
                    LaneTags[laneIndex].Recalculate();
                    LaneTags0.Add(ParentInfo.m_lanes[laneIndex], LaneTags[laneIndex]);
                }
            }

            public void SetLaneTags(int laneIndex, string[] tags) {
                LaneTags0 ??= new();
                LaneTags0.Add(ParentInfo.m_lanes[laneIndex], new LaneTagsT(tags));
                RecalculateLaneTags();
            }

            public static void SetLaneTags(NetInfo.Lane lane, string[] tags) {
                NetInfo netInfo = lane.GetParent(out int laneIndex);
                netInfo.GetMetaData().SetLaneTags(laneIndex, tags);
            }

            #endregion lane tags

            public void Recalculate(NetInfo netInfo) {
                try {
                    Log.Called(netInfo);
                    Assertion.NotNull(netInfo, "netInfo");
                    Assertion.Assert(netInfo.GetMetaData() == this, $"netInfo={netInfo}");
                    ParentInfo = netInfo;
                    if (ParentInfo.IsEditing()) {
                        AllocateMetadata();
                    }
                    FillCustomLaneFlagNames0();
                    RecalculateLaneTags0();
                    RecalculateTracks(netInfo);
                    UpdateTextureScales(netInfo);
                    RefreshLevelOfDetail(netInfo);
                    UsedCustomFlags = GatherUsedCustomFlags(netInfo);
                    RecalculateParkingAngle();
                    RecalculateConnectGroups(netInfo);
                    if (ParentInfo.IsEditing()) {
                        AllocateUserData();
                    } else {
                        OptimizeUserData();
                    }

                } catch(Exception ex) { ex.Log(); }
            }

            public void RefreshLevelOfDetail(NetInfo netInfo) {
                try {
                    if (Tracks != null) {
                        Log.Called();
                        var max = Mathf.Max(netInfo.m_halfWidth * 50f, (netInfo.m_maxHeight - netInfo.m_minHeight) * 80f);
                        // get lod render instance from already existing nodes/segments. if not found then calculate.
                        float lodRenderDistance =
                            netInfo.m_segments?.FirstOrDefault()?.m_lodRenderDistance ??
                            netInfo.m_nodes?.FirstOrDefault()?.m_lodRenderDistance ??
                            Mathf.Clamp(100f + RenderManager.LevelOfDetailFactor * max, 100f, 1000f);

                        bool lodMissing = false;
                        TrackLanes = 0;
                        for (int i = 0; i < Tracks.Length; i++) {
                            var track = Tracks[i];
                            bool hasLod = track.m_mesh;
                            if (hasLod) {
                                track.m_lodRenderDistance = lodRenderDistance;
                            } else {
                                track.m_lodRenderDistance = 100000f;
                                lodMissing = true;
                            }
                            netInfo.m_netLayers |= 1 << track.m_layer;
                            this.TrackLanes |= track.LaneIndeces;
                        }
                        if (lodMissing) {
                        }
                    }
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            void UpdateTextureScales(NetInfo netInfo) {
                try {
                    // tracks are taken care of in RecalculateTracks
                    // here we only deal with node/segment
                    foreach (var nodeInfo in netInfo.m_nodes)
                        nodeInfo?.GetMetaData()?.SetupTiling(nodeInfo);
                    foreach (var segmentInfo in netInfo.m_segments)
                        segmentInfo?.GetMetaData()?.SetupTiling(segmentInfo);
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            void RecalculateTracks(NetInfo netInfo) {
                try {
                    if (Tracks != null) {
                        // has color been already assigned in NetInfo.InitializePrefab() ?
                        bool hasColor = netInfo.m_segments?.Any(item => item.m_material) ?? false;
                        hasColor = hasColor || (netInfo.m_nodes?.Any(item => item.m_material) ?? false);
                        TrackLanes = 0;
                        for (int i = 0; i < Tracks.Length; i++) {
                            var track = Tracks[i];
                            track.Recalculate(netInfo);
                            netInfo.m_requireSurfaceMaps |= track.m_requireSurfaceMaps;
                            netInfo.m_requireHeightMap |= track.m_requireHeightMap;
                            track.CachedArrayIndex = i;
                            if (!hasColor && track.m_material != null) {
                                netInfo.m_color = track.m_material.color;
                                hasColor = true;
                            }
                            netInfo.m_netLayers |= 1 << track.m_layer;
                            this.TrackLanes |= track.LaneIndeces;
                        }

                        TrackLaneCount = EnumBitMaskExtensions.CountOnes(TrackLanes);
                        netInfo.m_requireDirectRenderers |= TrackLaneCount > 0;

                        bool tiltable = false;
                        var lanes = netInfo.m_lanes;
                        if (lanes != null) {
                            for (int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                                if (HasTrackLane(laneIndex) &&
                                    lanes[laneIndex].m_vehicleType.IsFlagSet(TrackUtils.TILTABLE_VEHICLE_TYPES)) {
                                    tiltable = true;
                                    break;
                                }

                            }
                        }
                        HasTitlableTracks = tiltable;
                    }
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            // NetInfo
            // Token: 0x06004ED3 RID: 20179 RVA: 0x00246F24 File Offset: 0x00245324
            [NonSerialized]
            public static NetInfo.Segment tempSegmentInfo_ = new NetInfo.Segment();
            public static NetInfo.Segment TempSegmentInfo(Track trackInfo) {
                try {
                    tempSegmentInfo_.m_lodMesh = trackInfo.m_lodMesh;
                    tempSegmentInfo_.m_lodMaterial = trackInfo.m_lodMaterial;
                    tempSegmentInfo_.m_combinedLod = trackInfo.m_combinedLod;
                    tempSegmentInfo_.m_preserveUVs = trackInfo.m_preserveUVs;
                    tempSegmentInfo_.m_generateTangents = trackInfo.m_generateTangents;
                    tempSegmentInfo_.m_requireWindSpeed = trackInfo.m_requireWindSpeed;
                    tempSegmentInfo_.m_requireSurfaceMaps = trackInfo.m_requireSurfaceMaps;
                    tempSegmentInfo_.m_requireHeightMap = trackInfo.m_requireHeightMap;
                    tempSegmentInfo_.m_layer = trackInfo.m_layer;
                    return tempSegmentInfo_;
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            public void InitMeshData(Track trackInfo, Rect atlasRect, Texture2D rgbAtlas, Texture2D xysAtlas, Texture2D aprAtlas) {
                try {
                    if (Log.VERBOSE)
                    { 
                        Log.Debug("InitMeshData() for trackInfo called");
                        Log.Debug($"trackInfo.m_lodMesh={trackInfo.m_lodMesh} trackInfo.m_lodMaterial={trackInfo.m_lodMaterial}"); 
                    }
                    // work around private fields/methods.
                    var tempSegment = TempSegmentInfo(trackInfo);
                    ParentInfo.InitMeshData(tempSegment, atlasRect, rgbAtlas, xysAtlas, aprAtlas);
                    if (!trackInfo.UseKeywordNETSEGMENT) {
                        tempSegment?.m_combinedLod?.m_material?.DisableKeyword("NET_SEGMENT");
                    }
                    trackInfo.m_combinedLod = tempSegment.m_combinedLod;
                } catch(Exception ex) {
                    ex.Log();
                }
            }

            void RecalculateParkingAngle() {
                try {
                    float sin = Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * ParkingAngleDegrees));
                    if (sin >= Mathf.Sin(30))
                        OneOverSinOfParkingAngle = 1 / sin;
                    else
                        OneOverSinOfParkingAngle = 1;
                } catch(Exception ex) { ex.Log(); }
            }

            void RecalculateConnectGroups(NetInfo netInfo) {
                try {
                    LogCalled();
                    ConnectGroupsHash = ConnectGroups?.Select(item => item.GetHashCode()).ToArray();
                    if (ConnectGroupsHash.IsNullorEmpty()) ConnectGroupsHash = null;

                    foreach (var node in netInfo.m_nodes)
                        node.GetMetaData()?.Update();

                    NodeConnectGroupsHash = GetNodeConnectGroupsHash(netInfo).ToArray();
                    if (NodeConnectGroupsHash.IsNullorEmpty()) NodeConnectGroupsHash = null;

                    var itemSource = ItemSource.GetOrCreate<NetInfo.ConnectGroup>();
                    foreach (var connectGroup in GetAllConnectGroups(netInfo))
                        itemSource.Add(connectGroup);
                } catch (Exception ex) { ex.Log(); }
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

}
