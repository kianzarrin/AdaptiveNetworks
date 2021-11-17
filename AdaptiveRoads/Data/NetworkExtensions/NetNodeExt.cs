namespace AdaptiveRoads.Manager {
    using ColossalFramework;
    using ColossalFramework.IO;
    using ColossalFramework.Math;
    using CSUtil.Commons;
    using KianCommons;
    using System;
    using TrafficManager;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using UnityEngine;
    using Log = KianCommons.Log;
    using System.Linq;
    using AdaptiveRoads.Util;
    using KianCommons.Serialization;
    using KianCommons.Plugins;
    using System.Reflection;
    using AdaptiveRoads.Data;

    public struct NetNodeExt {
        public ushort NodeID;
        public Flags m_flags;

        const int CUSTOM_FLAG_SHIFT = 24;
        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;
        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(ushort nodeID) => NodeID = nodeID;

        [Flags]
        public enum Flags {
            None = 0,
            [Hint(HintExtension.VANILLA)]
            Vanilla = 1 << 0,

            [Hide]
            [Hint("Hide Crossings mod is active")]
            HC_Mod = 1 << 1,

            [Hint("Direct Connect Roads mod is active")]
            DCR_Mod = 1 << 2,

            [Hint("Hide Unconnected Tracks mod is active")]
            HUT_Mod = 1 << 3,

            [Hint("all entering segment ends keep clear of the junction." +
                "useful for drawing pattern on the junction.")]
            KeepClearAll = 1 << 10,

            [Hint("the junction only has two segments.")]
            TwoSegments = 1 << 11,

            [Hint("the junction has segments with different speed limits.")]
            SpeedChange = 1 << 12,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1 << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,
        }

        public static IJunctionRestrictionsManager JRMan =>
            TrafficManager.Constants.ManagerFactory.JunctionRestrictionsManager;

        public void UpdateFlags() {
            m_flags = m_flags.SetFlags(Flags.HC_Mod, NetworkExtensionManager.Instance.HTC);
            m_flags = m_flags.SetFlags(Flags.DCR_Mod, NetworkExtensionManager.Instance.DCR);
            m_flags = m_flags.SetFlags(Flags.HUT_Mod, NetworkExtensionManager.Instance.HUT);

            if (JRMan != null) {
                bool keepClearAll = true;
                foreach(var segmentID in NetUtil.IterateNodeSegments(NodeID)) {
                    bool startNode = NetUtil.IsStartNode(segmentId: segmentID, nodeId: NodeID);
                    bool keppClear = JRMan.IsEnteringBlockedJunctionAllowed(segmentID, startNode);
                    keepClearAll &= keppClear;

                }
                m_flags = m_flags.SetFlags(Flags.KeepClearAll, keepClearAll);


                bool speedChange = TMPEHelpers.SpeedChanges(NodeID);
                bool twoSegments = NodeID.ToNode().CountSegments() == 2;

                m_flags = m_flags.SetFlags(Flags.SpeedChange, speedChange);
                m_flags = m_flags.SetFlags(Flags.TwoSegments, twoSegments);
            }
        }

        public override string ToString() {
            return $"NetNodeExt({NodeID} flags={m_flags})";
        }
    }
}

