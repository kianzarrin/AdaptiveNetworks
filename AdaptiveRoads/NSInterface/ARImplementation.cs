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
        internal static ARImplementation Instance;
        internal static void CreateOnReady() {
            try {
                Log.Called();
                Assertion.Assert(Instance == null, "instance should not exists");
                NSHelpers.DoOnNSEnabled(() => new ARImplementation());
            }catch(Exception ex) {
                ex.Log();
            }
        }

        internal void Release() {
            Log.Called();
            this.Remove();
            Instance = null;
        }


        internal ARImplementation() {
            try {
                Log.Called();
                Instance = this;
                this.Register();
            }catch(Exception ex) { ex.Log(); }
        }

        public string ID {
            get {
                Log.Called();
                return "Adaptive Roads";
            }
        }

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
            Log.Called(data.ToSTR(), instanceID.ToSTR());
            if(instanceID.Type == InstanceType.NetSegment) {
                ref var segmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[instanceID.NetSegment];
                if(data is ARCustomData customData) {
                    segmentExt.m_flags = segmentExt.m_flags.SetMaskedFlags(customData.SegmentExtFlags, NetSegmentExt.Flags.CustomsMask);
                } else if(data is null) {
                    segmentExt.m_flags = segmentExt.m_flags.SetMaskedFlags(NetSegmentExt.Flags.None, NetSegmentExt.Flags.CustomsMask);
                }
                Log.Info("OnSkinApplied: segmentExt.m_flags=" + segmentExt.m_flags);
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

        public Texture2D Icon {
            get {
                Log.Called();
                return TextureUtil.GetTextureFromFile("B1.png");
            }
        }
        public string Tooltip {
            get {
                Log.Called();
                return "Adaptive Roads";
            }
        }

        UIPanel container_;
        public void BuildPanel(UIPanel panel) {
            try {
                Log.Called();
                Assertion.NotNull(panel, "container");
                container_ = panel;
                foreach(var flag in UsedCustomSegmentFlags.ExtractPow2Flags())
                    SegmentFlagToggle.Add(panel, flag);
                RefreshUI();
            } catch(Exception ex) { ex.Log(); }
        }

        public void RefreshUI() {
            try {
                Log.Called();
                foreach(var toggle in container_.GetComponentsInChildren<SegmentFlagToggle>())
                    toggle.Refresh(CustomSegmentFlags);
            } catch(Exception ex) { ex.Log(); }
        }
        #endregion

        #region controller
        internal NetInfo Prefab => NetUtil.netTool.m_prefab;

        internal NetSegmentExt.Flags CustomSegmentFlags;
        internal NetSegmentExt.Flags UsedCustomSegmentFlags => Prefab?.GetMetaData()?.UsedCustomFlags.Segment ?? default;

        private static string GetFlagKey(NetSegmentExt.Flags flag) {
            return "MOD_AR_SEGMENTEXT_" + flag;
        }

        public bool IsDefault => CustomSegmentFlags == default;

        public bool Enabled => (UsedCustomSegmentFlags != default).LogRet();

        public void LoadWithData(ICloneable data) {
            try {
                Log.Called();
                if(data is ARCustomData customData) {
                    CustomSegmentFlags = customData.SegmentExtFlags;
                } else {
                    CustomSegmentFlags = default;
                }
            } catch(Exception ex) { ex.Log(); }
        }

        public void LoadActiveSelection() {
            try {
                Log.Called();
                CustomSegmentFlags = default;
                foreach(var usedFlag in UsedCustomSegmentFlags.ExtractPow2Flags()) {
                    var value = ActiveSelectionData.Instance.GetBoolValue(Prefab, GetFlagKey(usedFlag));
                    if(value.HasValue && value == true) {
                        CustomSegmentFlags = CustomSegmentFlags.SetFlags(usedFlag);
                    }
                }
            } catch(Exception ex) { ex.Log(); }

        }

        public void Reset() {
            try {
                Log.Called();
                CustomSegmentFlags = default;
                SaveActiveSelection();
            } catch(Exception ex) { ex.Log(); }
        }

        public Dictionary<NetInfo, ICloneable> BuildCustomData() {
            try {
                Log.Called();
                var ret = new Dictionary<NetInfo, ICloneable>();
                if(!IsDefault) {
                    ret[Prefab] = new ARCustomData { SegmentExtFlags = CustomSegmentFlags };
                }
                return ret;
            } catch(Exception ex) { ex.Log();}
            return null;
        }

        public void SaveActiveSelection() {
            try {
                Log.Called();
                foreach(var usedFlag in UsedCustomSegmentFlags.ExtractPow2Flags()) {
                    if(CustomSegmentFlags.IsFlagSet(usedFlag))
                        ActiveSelectionData.Instance.SetBoolValue(Prefab, GetFlagKey(usedFlag), true);
                    else
                        ActiveSelectionData.Instance.ClearValue(Prefab, GetFlagKey(usedFlag));
                }
            } catch(Exception ex) { ex.Log(); }
        }

    #endregion

    }
}
