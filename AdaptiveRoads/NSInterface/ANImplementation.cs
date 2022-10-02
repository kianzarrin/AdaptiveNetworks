namespace AdaptiveRoads.NSInterface {
    using System;
    using System.Collections.Generic;
    using ColossalFramework.UI;
    using NetworkSkins.Helpers;
    using KianCommons;
    using KianCommons.Serialization;
    using AdaptiveRoads.Manager;
    using UnityEngine;
    using NetworkSkins.Persistence;
    using TextureUtil = KianCommons.UI.TextureUtil;
    using AdaptiveRoads.NSInterface.UI;
    using AdaptiveRoads.Data.NetworkExtensions;


    public class ANImplementation : NSIntegrationBase<ANImplementation> {
        public override string ID => "Adaptive Roads";

        public override int Index { get; set; }

        public override void OnSkinApplied(ICloneable data, InstanceID instanceID) {
            try {
                Log.Called(data, instanceID);
                if (instanceID.Type == InstanceType.NetSegment) {
                    ushort segmentID = instanceID.NetSegment;
                    var prefab = segmentID.ToSegment().Info;
                    if (prefab?.GetMetaData() == null) return;
                    var customFlags = data as ARCustomFlags ?? new ARCustomFlags(prefab.m_lanes.Length);

                    ref var segmentExt = ref segmentID.ToSegmentExt();
                    segmentExt.m_flags = segmentExt.m_flags.SetMaskedFlags(customFlags.Segment, NetSegmentExt.Flags.CustomsMask);

                    segmentExt.Start.m_flags = segmentExt.Start.m_flags.SetMaskedFlags(customFlags.SegmentEnd, NetSegmentEnd.Flags.CustomsMask);
                    segmentExt.End.m_flags = segmentExt.End.m_flags.SetMaskedFlags(customFlags.SegmentEnd, NetSegmentEnd.Flags.CustomsMask);

                    foreach (var lane in NetUtil.IterateLanes(segmentID)) {
                        ref var laneExt = ref lane.LaneID.ToLaneExt();
                        Log.Debug($"lane:{lane}");
                        Assertion.InRange(customFlags.Lanes, lane.LaneIndex);
                        laneExt.m_flags = laneExt.m_flags.SetMaskedFlags(customFlags.Lanes[lane.LaneIndex], NetLaneExt.Flags.CustomsMask);
                    }
                    Log.Succeeded($"segment:{segmentID} : " + customFlags);

                } else if (instanceID.Type == InstanceType.NetNode) {
                    ushort nodeID = instanceID.NetNode;
                    var prefab = nodeID.ToNode().Info;
                    if (prefab?.GetMetaData() == null) return;
                    var customFlags = data as ARCustomFlags ?? new ARCustomFlags(prefab.m_lanes.Length);

                    ref var nodeExt = ref nodeID.ToNodeExt();
                    nodeExt.m_flags = nodeExt.m_flags.SetMaskedFlags(customFlags.Node, NetNodeExt.Flags.CustomsMask);
                    Log.Succeeded($"node:{nodeID}) : " + customFlags);
                }
            } catch(Exception ex) { ex.Log(); }
        }

        #region persistency
        public override Version DataVersion => this.VersionOf();
        public override string Encode64(ICloneable data) {
            Log.Called();
            return data is ARCustomFlags customData ? XMLSerializerUtil.Serialize(customData) : null;
        }

        public override ICloneable Decode64(string base64Data, Version dataVersion) {
            Log.Called();
            return base64Data != null ? XMLSerializerUtil.Deserialize<ARCustomFlags>(base64Data) : null;
        }
        #endregion


        #region GUI

        public override Texture2D Icon {
            get {
                Log.Called();
                return TextureUtil.GetTextureFromFile("NS.png");
            }
        }
        public override string Tooltip => "Adaptive Networks";

        UIPanel container_;
        UIPanel subContainer_;
        public override void BuildPanel(UIPanel panel) {
            try {
                Log.Called();
                if(!Enabled) return;
                Assertion.Assert(panel, "panel");
                Assertion.NotNull(panel, "container");
                container_ = panel;
                subContainer_ = container_.AddUIComponent<AR_NS_FlagsPanel>();
                Log.Succeeded();
            } catch(Exception ex) { ex.Log(); }
        }

        public override void RefreshUI() {
            try {
                Log.Called();
                if(!Enabled) return;
                GameObject.Destroy(subContainer_?.gameObject);
                BuildPanel(container_);
            } catch(Exception ex) { ex.Log(); }
        }
        #endregion

        #region controller
        internal NetInfo BasePrefab => NetUtil.netTool.m_prefab;

        internal ARCustomFlags ARCustomFlags;
        internal CustomFlags SharedCustomFlags => BasePrefab?.GatherSharedCustomFlags() ?? default;

        private static string GetFlagKey(Enum flag) =>
            $"MOD_AR_{flag.GetType().DeclaringType.Name}_{flag}";

        private static string GetFlagKey(Enum flag, int laneIndex) =>
            laneIndex < 0 ? GetFlagKey(flag) : GetLaneFlagKey(laneIndex: laneIndex, flag: flag);

        private static string GetLaneFlagKey(int laneIndex, Enum flag) =>
            $"MOD_AR_lanes[{laneIndex}]_{flag}";
        

        public bool IsDefault => (ARCustomFlags != null && ARCustomFlags.IsDefault()).LogRet();

        public override bool Enabled => (!SharedCustomFlags.IsDefault()).LogRet();

        public override void LoadWithData(ICloneable data) {
            try {
                Log.Called();
                if(data is ARCustomFlags customData) {
                    ARCustomFlags = customData;
                } else {
                    ARCustomFlags = new ARCustomFlags(BasePrefab.m_lanes.Length);
                }
            } catch(Exception ex) { ex.Log(); }
            Log.Succeeded();
        }

        public override void LoadActiveSelection() {
            try {
                Log.Called();
                ARCustomFlags = new ARCustomFlags(BasePrefab.m_lanes.Length);
                ARCustomFlags shared = BasePrefab?.GatherSharedARCustomFlags();
                foreach (var elevation in BasePrefab.AllElevations()) {
                    var elevationFlags = elevation.GatherARCustomFlags();
                    foreach (var pair in elevationFlags.IterateAll()) {
                        Enum flag = pair.Key;
                        int laneIndex = pair.Value;
                        string key = GetFlagKey(flag, laneIndex);
                        bool? value = ActiveSelectionData.Instance.GetBoolValue(elevation, key);
                        if (value.HasValue && value == true && shared.HasFlag(flag)) {
                            ARCustomFlags.AddFlag(flag, laneIndex);
                        }
                    }
                }
            } catch(Exception ex) { ex.Log(); }
            Log.Succeeded();
        }

        public override void SaveActiveSelection() {
            try {
                Log.Called();
                foreach (var elevation in BasePrefab.AllElevations()) {
                    var elevationFlags = elevation.GatherARCustomFlags();
                    foreach(var pair in elevationFlags.IterateAll()) {
                        Enum flag = pair.Key;
                        int laneIndex = pair.Value;
                        string key = GetFlagKey(flag, laneIndex);
                        if (ARCustomFlags.HasFlag(flag, laneIndex)) {
                            Log.Debug($"ActiveSelectionData.Instance.SetBoolValue({elevation}, {key}, {true})");
                            ActiveSelectionData.Instance.SetBoolValue(elevation, key, true);
                        } else {
                            ActiveSelectionData.Instance.ClearValue(elevation, key);
                        }
                    }
                }
            } catch(Exception ex) { ex.Log(); }
            Log.Succeeded();
        }

        public override void Reset() {
            try {
                Log.Called();
                ARCustomFlags = new ARCustomFlags(BasePrefab.m_lanes.Length);
            } catch(Exception ex) { ex.Log(); }
            Log.Succeeded();
        }

        public void Change() => this.OnControllerChanged();

        public override Dictionary<NetInfo, ICloneable> BuildCustomData() {
            try {
                Log.Called("CustomSegmentFlags are " + ARCustomFlags.Segment);
                var ret = new Dictionary<NetInfo, ICloneable>();
                foreach (var elevationInfo in BasePrefab.AllElevations()) {
                    var elevationFlag = ARCustomFlags & elevationInfo.GatherARCustomFlags();
                    if (!elevationFlag.IsDefault()) {
                        ret[elevationInfo] = elevationFlag;
                    }
                }
                return ret.LogRet();
            } catch(Exception ex) { ex.Log();}
            return null;
        }
    #endregion

    }
}
