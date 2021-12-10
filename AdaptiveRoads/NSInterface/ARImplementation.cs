namespace AdaptiveRoads.NSInterface {
    using System;
    using System.Collections.Generic;
    using ColossalFramework.UI;
    using NetworkSkins.Helpers;
    using KianCommons;
    using KianCommons.Serialization;
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using UnityEngine;
    using NetworkSkins.Persistence;
    using TextureUtil = KianCommons.UI.TextureUtil;
    using AdaptiveRoads.NSInterface.UI;
    using AdaptiveRoads.Data.NetworkExtensions;


    public class ARImplementation : NSIntegrationBase<ARImplementation> {
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
        public override string Tooltip => "Adaptive Roads";

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
        internal NetInfo Prefab => NetUtil.netTool.m_prefab;

        internal ARCustomFlags ARCustomFlags;
        internal CustomFlags PrefabUsedCustomFlags => Prefab?.GetMetaData()?.UsedCustomFlags ?? default;
        internal NetLaneExt.Flags GetPrefabLaneUsedCustomFlags(int laneIndex) =>
            Prefab?.GetMetaData()?.GetUsedCustomFlagsLane(laneIndex) ?? default;
        private static string GetFlagKey(Enum flag) =>
            $"MOD_AR_{flag.GetType().DeclaringType.Name}_{flag}";
        

        private static string GetLaneFlagKey(int laneIndex, Enum flag) =>
            $"MOD_AR_lanes[{laneIndex}]_{flag}";
        

        public bool IsDefault => ARCustomFlags.IsDefault();

        public override bool Enabled => !PrefabUsedCustomFlags.IsDefault();

        public override void LoadWithData(ICloneable data) {
            try {
                Log.Called();
                if(data is ARCustomFlags customData) {
                    ARCustomFlags = customData;
                } else {
                    ARCustomFlags = new ARCustomFlags(Prefab.m_lanes.Length);
                }
            } catch(Exception ex) { ex.Log(); }
        }

        public override void LoadActiveSelection() {
            try {
                Log.Called();
                ARCustomFlags = new ARCustomFlags(Prefab.m_lanes.Length);
                foreach(var usedFlag in PrefabUsedCustomFlags.Segment.ExtractPow2Flags()) {
                    var value = ActiveSelectionData.Instance.GetBoolValue(Prefab, GetFlagKey(usedFlag));
                    if(value.HasValue && value == true) {
                        ARCustomFlags.Segment = ARCustomFlags.Segment.SetFlags(usedFlag);
                    }
                }
                foreach(var usedFlag in PrefabUsedCustomFlags.Node.ExtractPow2Flags()) {
                    var value = ActiveSelectionData.Instance.GetBoolValue(Prefab, GetFlagKey(usedFlag));
                    if(value.HasValue && value == true) {
                        ARCustomFlags.Node = ARCustomFlags.Node.SetFlags(usedFlag);
                    }
                }
                foreach(var usedFlag in PrefabUsedCustomFlags.SegmentEnd.ExtractPow2Flags()) {
                    var value = ActiveSelectionData.Instance.GetBoolValue(Prefab, GetFlagKey(usedFlag));
                    if(value.HasValue && value == true) {
                        ARCustomFlags.SegmentEnd = ARCustomFlags.SegmentEnd.SetFlags(usedFlag);
                    }
                }
                if(PrefabUsedCustomFlags.Lane != default) {
                    foreach(var laneIndex in Prefab.m_sortedLanes) {
                        var usedLaneFlags = GetPrefabLaneUsedCustomFlags(laneIndex);
                        foreach(var usedFlag in usedLaneFlags.ExtractPow2Flags()) {
                            var value = ActiveSelectionData.Instance.GetBoolValue(Prefab, GetLaneFlagKey(laneIndex, usedFlag));
                            if(value.HasValue && value == true) {
                                ARCustomFlags.Lanes[laneIndex] = ARCustomFlags.Lanes[laneIndex].SetFlags(usedFlag);
                            }
                        }
                    }
                }

            } catch(Exception ex) { ex.Log(); }

        }
        public override void SaveActiveSelection() {
            try {
                Log.Called();
                foreach(var usedFlag in PrefabUsedCustomFlags.Segment.ExtractPow2Flags()) {
                    if(ARCustomFlags.Segment.IsFlagSet(usedFlag))
                        ActiveSelectionData.Instance.SetBoolValue(Prefab, GetFlagKey(usedFlag), true);
                    else
                        ActiveSelectionData.Instance.ClearValue(Prefab, GetFlagKey(usedFlag));
                }
                foreach(var usedFlag in PrefabUsedCustomFlags.Node.ExtractPow2Flags()) {
                    if(ARCustomFlags.Node.IsFlagSet(usedFlag))
                        ActiveSelectionData.Instance.SetBoolValue(Prefab, GetFlagKey(usedFlag), true);
                    else
                        ActiveSelectionData.Instance.ClearValue(Prefab, GetFlagKey(usedFlag));
                }
                foreach(var usedFlag in PrefabUsedCustomFlags.SegmentEnd.ExtractPow2Flags()) {
                    if(ARCustomFlags.SegmentEnd.IsFlagSet(usedFlag))
                        ActiveSelectionData.Instance.SetBoolValue(Prefab, GetFlagKey(usedFlag), true);
                    else
                        ActiveSelectionData.Instance.ClearValue(Prefab, GetFlagKey(usedFlag));
                }
                if(PrefabUsedCustomFlags.Lane != default) {
                    foreach(var laneIndex in Prefab.m_sortedLanes) {
                        var usedLaneFlags = GetPrefabLaneUsedCustomFlags(laneIndex);
                        foreach(var usedFlag in usedLaneFlags.ExtractPow2Flags()) {
                            if(ARCustomFlags.Lanes[laneIndex].IsFlagSet(usedFlag))
                                ActiveSelectionData.Instance.SetBoolValue(Prefab, GetLaneFlagKey(laneIndex, usedFlag), true);
                            else
                                ActiveSelectionData.Instance.ClearValue(Prefab, GetLaneFlagKey(laneIndex, usedFlag));
                        }
                    }
                }

            } catch(Exception ex) { ex.Log(); }
        }

        public override void Reset() {
            try {
                Log.Called();
                ARCustomFlags = new ARCustomFlags(Prefab.m_lanes.Length);
            } catch(Exception ex) { ex.Log(); }
        }

        public void Change() => this.OnControllerChanged();

        public override Dictionary<NetInfo, ICloneable> BuildCustomData() {
            try {
                Log.Called("CustomSegmentFlags are " + ARCustomFlags.Segment);
                var ret = new Dictionary<NetInfo, ICloneable>();
                if(!IsDefault) {
                    ret[Prefab] = ARCustomFlags.Clone() as ARCustomFlags; 
                }
                return ret.LogRet();
            } catch(Exception ex) { ex.Log();}
            return null;
        }
    #endregion

    }
}
