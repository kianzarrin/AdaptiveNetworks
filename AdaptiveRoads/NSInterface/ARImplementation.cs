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
    using AdaptiveRoads.UI.Tool;
    using TextureUtil = KianCommons.UI.TextureUtil;

    public class ARImplementation : INSImplementation {
        public static ARImplementation Instance;
        public static void CreateOnReady() {
            try {
                Log.Called();
                Assertion.Assert(Instance == null, "instance should not exists");
                NSHelpers.DoOnNSEnabled(() => new ARImplementation());
            }catch(Exception ex) {
                ex.Log();
            }
        }

        public void Release() {
            Log.Called();
            this.Remove();
            Instance = null;
        }


        public ARImplementation() {
            try {
                Log.Called();
                Instance = this;
                this.Register();
            }catch(Exception ex) { ex.Log(); }
        }

        public string ID { get; }
        public int Index { get; set; }

        public void OnBeforeNSLoaded() {
            Log.Called();

        }

        public void OnAfterNSLoaded() {
            Log.Called();

        }

        public void OnNSDisabled() {
            Log.Called();
            Release();
            CreateOnReady();
        }

        public void OnSkinApplied(ICloneable data, InstanceID instanceID) {
            Log.Called();
            if(data is ARCustomData customData) {
                if(instanceID.Type == InstanceType.NetSegment) {
                    ref var segmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[instanceID.NetSegment];
                    segmentExt.m_flags = segmentExt.m_flags.SetMaskedFlags(customData.SegmentExtFlags, NetSegmentExt.Flags.CustomsMask);
                }
            }
        }

        #region persistancy
        public Version DataVersion => this.VersionOf();
        public string Encode64(ICloneable data) {
            Log.Called();
            return data is ARCustomData customData ? XMLSerializerUtil.Serialize(customData) : null;
        }

        public ICloneable Decode64(string base64Data, Version dataVersion) {
            Log.Called();
            return base64Data != null ? XMLSerializerUtil.Deserialize<ARCustomData>(base64Data) : null;
        }
        #endregion


        #region GUI

        public Texture2D Icon => TextureUtil.GetTextureFromFile("B1.png");
        public string Tooltip => "Adaptive Roads";

        public void BuildPanel(UIPanel panel) {
            Log.Called();
            Assertion.NotNull(panel, "container");

            foreach(var flag in UsedCustomSegmentFlags.ExtractPow2Flags()) {
                SegmentFlagToggle.Add(panel, 0, null, flag);
            }
            RefreshUI();
        }

        public void RefreshUI() {
            Log.Called();
            throw new NotImplementedException();
        }
        #endregion

        #region controller
        NetInfo Prefab => NetUtil.netTool.m_prefab;

        public NetSegmentExt.Flags CustomSegmentFlags;
        private NetSegmentExt.Flags UsedCustomSegmentFlags => Prefab?.GetMetaData()?.UsedCustomFlags.Segment ?? default;

        private static string GetFlagKey(NetSegmentExt.Flags flag) {
            return "MOD_AR_SEGMENTEXT_" + flag;
        }

        public bool IsDefault => CustomSegmentFlags == default;

        public bool Enabled => UsedCustomSegmentFlags != default;

        public void LoadWithData(ICloneable data) {
            Log.Called();
            if(data is ARCustomData customData) {
                CustomSegmentFlags = customData.SegmentExtFlags;
            } else {
                CustomSegmentFlags = default;
            }
        }

        public void LoadActiveSelection() {
            Log.Called();
            CustomSegmentFlags = default;
            foreach(var usedFlag in UsedCustomSegmentFlags.ExtractPow2Flags()) {
                var value = ActiveSelectionData.Instance.GetBoolValue(Prefab, GetFlagKey(usedFlag));
                if(value.HasValue && value == true) {
                    CustomSegmentFlags = CustomSegmentFlags.SetFlags(usedFlag);
                }
            }
        }

        public void Reset() {
            Log.Called();
            CustomSegmentFlags = default;
            SaveActiveSelection();
        }

        public Dictionary<NetInfo, ICloneable> BuildCustomData() {
            Log.Called();
            var ret = new Dictionary<NetInfo, ICloneable>();
            if(!IsDefault) {
                ret[Prefab] = new ARCustomData { SegmentExtFlags = CustomSegmentFlags };
            }
            return ret;
        }

        public void SaveActiveSelection() {
            Log.Called();
            foreach(var usedFlag in UsedCustomSegmentFlags.ExtractPow2Flags()) {
                if(CustomSegmentFlags.IsFlagSet(usedFlag))
                    ActiveSelectionData.Instance.SetBoolValue(Prefab, GetFlagKey(usedFlag), true);
                else
                    ActiveSelectionData.Instance.ClearValue(Prefab, GetFlagKey(usedFlag));
            }
        }

        #endregion

    }
}
