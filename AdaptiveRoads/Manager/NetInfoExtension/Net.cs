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
    using System.Xml.Serialization;
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
                    ret.CustomConnectGroups = ret.CustomConnectGroups?.Clone();
                    ret.QuayRoadsProfile = QuayRoadsProfile?.ToArray();
                    ret.CustomFlagNames = ret.CustomFlagNames?.ShallowClone();
                    ret.ScriptedFlags = ret.ScriptedFlags?.ShallowClone();
                    ret.CustomLaneFlagNames0 = ret.CustomLaneFlagNames0?.ShallowClone();
                    ret.LaneTags = ret.LaneTags?.Select(item => item?.Clone())?.ToArray();
                    ret.LaneTags0 = ret.LaneTags0?.ToDictionary(pair => pair.Key, pair => pair.Value?.Clone());
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
                    SerializationUtil.GetObjectFields(info, this);
                    SerializationUtil.GetObjectProperties(info, this);
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            // deserialization
            public Net(SerializationInfo info, StreamingContext context) {
                try {
                    //Log.Called();
                    SerializationUtil.SetObjectProperties(info, this);
                    SerializationUtil.SetObjectFields(info, this);
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }
            #endregion

            [NonSerialized]
            public NetInfo ParentInfo;

            // serialize CustomConnectGroups
            public string[] ConnectGroups {
                get => CustomConnectGroups.Selected;
                set => CustomConnectGroups = new CustomConnectGroupT(value);
            }

            // work around deserializing Tags.
            public string[] Tags;

            [NonSerialized]
            [XmlIgnore]
            public CustomConnectGroupT CustomConnectGroups = new CustomConnectGroupT(null);

            [NonSerialized]
            [XmlIgnore]
            public DynamicFlags NodeCustomConnectGroups;

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
            [Hint("accepted values are 0 or between 30 to 90 degrees")]
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
                    foreach(var lane in ParentInfo.m_lanes) {
                        foreach (var prop in lane?.m_laneProps?.m_props ?? Enumerable.Empty<NetLaneProps.Prop>()) {
                            prop?.GetOrCreateMetaData()?.SegmentUserData?.RemoveValueAt(i);
                        }
                    }
                    foreach(var track in Tracks ?? Enumerable.Empty<Track>()) {
                        track?.SegmentUserData?.RemoveValueAt(i);
                        foreach(var prop in track?.Props ?? Enumerable.Empty<TransitionProp>()) {
                            prop?.SegmentUserData?.RemoveValueAt(i);
                        }
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
                        } else if (target is TransitionProp tprop) {
                            var netInfo = tprop.GetParent(trackIndex: out int trackIndex, out _);
                            var track2 = netInfo.GetMetaData().Tracks[trackIndex];
                            for (int laneIndex = 0; laneIndex < netInfo.m_lanes.Length; ++laneIndex) {
                                if (track2.HasTrackLane(laneIndex)) {
                                    string name = netInfo?.GetMetaData()?.GetCustomLaneFlagName(laneFlag, laneIndex);
                                    if (!name.IsNullorEmpty()) return name;
                                }
                            }
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
                        } else if (target is TransitionProp tprop) {
                            var netInfo = tprop.GetParent(trackIndex: out int trackIndex, out _);
                            var track2 = netInfo.GetMetaData().Tracks[trackIndex];
                            for (int laneIndex = 0; laneIndex < netInfo.m_lanes.Length; ++laneIndex) {
                                if (track2.HasTrackLane(laneIndex)) {
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
            public Dictionary<NetInfo.Lane, LaneTagsT> LaneTags0 = new();

            public LaneTagsT[] LaneTags {
                get {
                    // serialization
                    var lanes = ParentInfo.m_lanes;
                    if (lanes == null || LaneTags0 == null) {
                        return null;
                    }

                    LaneTagsT[] laneTags = new LaneTagsT[lanes.Length];
                    for(int i = 0; i < lanes.Length; ++i) {
                        var lane = lanes[i];
                        if (LaneTags0.TryGetValue(lane, out var tags)) {
                            tags.Recalculate();
                            laneTags[i] = tags;
                        }
                    }
                    return laneTags;
                }
                set {
                    // deserialization
                    var laneTags = value;
                    var lanes = ParentInfo.m_lanes;
                    LaneTags0 = new();
                    if (lanes == null || laneTags == null) {
                        return;
                    }

                    for (int laneIndex = 0; laneIndex < laneTags.Length; ++laneIndex) {
                        laneTags[laneIndex] ??= new(null);
                        laneTags[laneIndex].Recalculate();
                        LaneTags0.Add(ParentInfo.m_lanes[laneIndex], laneTags[laneIndex]);
                    }
                }
            }

            public LaneTagsT GetLaneTags(NetInfo.Lane lane) {
                if(LaneTags0 != null && LaneTags0.TryGetValue(lane, out var ret))
                    return ret;
                else
                    return null;
            }
            public LaneTagsT GetOrCreateLaneTags(NetInfo.Lane laneInfo) {
                LaneTags0 ??= new();
                if (LaneTags0.TryGetValue(laneInfo, out var ret) && ret != null)
                    return ret;
                else
                    return LaneTags0[laneInfo] = new LaneTagsT(null);
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
                } catch (Exception ex) { ex.Log(); }
            }

            public void LoadVanillaTags() {
                ParentInfo.m_tags = Tags;
                NetInfo.AddTags(ParentInfo.m_tags);
                ParentInfo.m_netTags = NetInfo.GetFlags(ParentInfo.m_tags);
                foreach (var node in ParentInfo.m_nodes) {
                    if (node?.GetMetaData() is Node metadata) {
                        node.m_tagsRequired = metadata.TagsInfo.Required;
                        node.m_tagsForbidden = metadata.TagsInfo.Forbidden;
                        node.m_minSameTags = metadata.TagsInfo.MinMatch;
                        node.m_maxSameTags = metadata.TagsInfo.MaxMatch;
                        node.m_minOtherTags = metadata.TagsInfo.MinMismatch;
                        node.m_maxOtherTags = metadata.TagsInfo.MaxMismatch;
                        NetInfo.AddTags(node.m_tagsRequired);
                        NetInfo.AddTags(node.m_tagsForbidden);
                        node.m_nodeTagsRequired = NetInfo.GetFlags(node.m_tagsRequired);
                        node.m_nodeTagsForbidden = NetInfo.GetFlags(node.m_tagsForbidden);
                    }
                }
            }

            public void SaveVanillaTags() {
                Tags = ParentInfo.m_tags;
                foreach (var node in ParentInfo.m_nodes) {
                    if (node?.GetMetaData() is Node metadata) {
                        metadata.TagsInfo.Required = node.m_tagsRequired;
                        metadata.TagsInfo.Forbidden = node.m_tagsForbidden;
                        metadata.TagsInfo.MinMatch = node.m_minSameTags;
                        metadata.TagsInfo.MaxMatch = node.m_maxSameTags;
                        metadata.TagsInfo.MinMismatch = node.m_minOtherTags;
                        metadata.TagsInfo.MaxMismatch = node.m_maxOtherTags;
                    }
                }
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
                        foreach(var track in Tracks) { 
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
                            // warn
                        }

                        CheckReferences();
                    }
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            public void CheckReferences() {
                foreach (var track in Tracks) {
                    foreach (var prop in track.Props) {
                        if (prop.m_prop != null) {
                            if (prop.m_prop.m_prefabInitialized) {
                                prop.m_finalProp = prop.m_prop;
                            } else {
                                prop.m_finalProp = PrefabCollection<PropInfo>.FindLoaded(prop.m_prop.gameObject.name);
                            }
                            if (prop.m_finalProp == null) {
                                throw new PrefabException(ParentInfo, "Referenced prop is not loaded (" + prop.m_prop.gameObject.name + ")");
                            }
                            ParentInfo.CheckProp(prop.m_finalProp);
                            ParentInfo.m_maxPropDistance = Mathf.Max(ParentInfo.m_maxPropDistance, prop.m_finalProp.m_maxRenderDistance);
                        } else if (prop.m_tree != null) {
                            ParentInfo.m_hasUpgradableTreeLanes |= prop.m_upgradable;
                            if (prop.m_tree.m_prefabInitialized) {
                                prop.m_finalTree = prop.m_tree;
                            } else {
                                prop.m_finalTree = PrefabCollection<TreeInfo>.FindLoaded(prop.m_tree.gameObject.name);
                            }
                            if (prop.m_finalTree == null) {
                                throw new PrefabException(ParentInfo, "Referenced tree is not loaded (" + prop.m_tree.gameObject.name + ")");
                            }
                            ParentInfo.m_propLayers |= 1 << prop.m_finalTree.m_prefabDataLayer;
                        }
                    }
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
                    if (sin >= Mathf.Sin(29 * Mathf.Deg2Rad))
                        OneOverSinOfParkingAngle = 1 / sin;
                    else
                        OneOverSinOfParkingAngle = 1;
                } catch(Exception ex) { ex.Log(); }
            }

            void RecalculateConnectGroups(NetInfo netInfo) {
                try {
                    LogCalled();
                    foreach (var node in netInfo.m_nodes)
                        node.GetMetaData()?.CustomConnectGroups.Recalculate();
                    CustomConnectGroups.Recalculate();
                    NodeCustomConnectGroups = GetNodeCustomConnectGroups(netInfo);
                } catch (Exception ex) { ex.Log(); }
            }

            DynamicFlags GetNodeCustomConnectGroups(NetInfo netInfo) {
                DynamicFlags ret = new DynamicFlags(DynamicFlagsUtil.EMPTY_FLAGS);
                foreach(var node in netInfo.m_nodes) {
                    if(node.GetMetaData() is Node nodeMetaData)
                        ret = ret | nodeMetaData.CustomConnectGroups.Flags;
                }
                return ret;
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
