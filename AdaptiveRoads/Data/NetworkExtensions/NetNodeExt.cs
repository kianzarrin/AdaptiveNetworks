namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data.NetworkExtensions;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using ColossalFramework.Math;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;
    using Log = KianCommons.Log;

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
            try {
                Transitions = null;
                if(!NetUtil.IsNodeValid(NodeID)) {
                    if(NodeID.ToNode().m_flags.IsFlagSet(NetSegment.Flags.Created))
                        Log.Debug("Skip updating invalid node:" + NodeID);
                    return;
                }
                m_flags = m_flags.SetFlags(Flags.HC_Mod, NetworkExtensionManager.Instance.HTC);
                m_flags = m_flags.SetFlags(Flags.DCR_Mod, NetworkExtensionManager.Instance.DCR);
                m_flags = m_flags.SetFlags(Flags.HUT_Mod, NetworkExtensionManager.Instance.HUT);

                if(JRMan != null) {
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

                    GetTrackConnections();
                    if(Log.VERBOSE) Log.Debug($"NetNodeExt.UpdateFlags() succeeded for {this}" /*Environment.StackTrace*/, false);
                }
            } catch(Exception ex) {
                ex.Log("node=" + this);
            }
        }

        public override string ToString() {
            return $"NetNodeExt({NodeID} flags={m_flags})";
        }
        #region track
        /* terminology:
         * - connection does not care about source/target.
         * - transition/routing care
         *    - transition is between two lanes.
         *    - routing is a set of transitions.
         */
        public struct Connection {
            public uint LaneID1;
            public uint LaneID2;
            public override bool Equals(object obj) {
                if(obj is Connection rhs) {
                    if(LaneID1 == rhs.LaneID1 && LaneID2 == rhs.LaneID2)
                        return true;
                    if(LaneID1 == rhs.LaneID2 && LaneID2 == rhs.LaneID1)
                        return true;
                }
                return false;
            }
            public override int GetHashCode() => (int)(LaneID1 ^ LaneID2);
        }
        public static HashSet<Connection> tempConnections_ = new HashSet<Connection>();
        public LaneTransition[] Transitions;
        public void GetTrackConnections() {
            try {
                Transitions = null;
                ref var node = ref NodeID.ToNode();
                if(!node.IsValid())
                    return;
                if(!node.m_flags.IsFlagSet(NetNode.Flags.Junction | NetNode.Flags.Bend))
                    return;

                tempConnections_.Clear();
                foreach(var segmentID in NodeID.ToNode().IterateSegments()) {
                    var infoExt = segmentID.ToSegment().Info?.GetMetaData();
                    if(infoExt == null) continue;
                    //Assertion.Assert(infoExt == segmentID.ToSegmentExt().NetInfoExt, $"{infoExt} == {segmentID.ToSegmentExt().NetInfoExt}"); // asset initialized.
                    var lanes = new LaneIDIterator(segmentID).ToArray();
                    for(int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                        uint laneID = lanes[laneIndex];
                        var routings = TMPEHelpers.GetForwardRoutings(laneID, NodeID);
                        if(routings == null) continue;
                        if(IsNodeless(segmentID: segmentID, nodeID: NodeID)) continue;
                        foreach(LaneTransitionData routing in routings) {
                            if(routing.type == LaneEndTransitionType.Invalid || routing.type == LaneEndTransitionType.Relaxed)
                                continue;
                            var infoExt2 = routing.segmentId.ToSegment().Info?.GetMetaData();
                            if(infoExt2 == null) continue;
                            if(IsNodeless(segmentID: routing.segmentId, nodeID: NodeID)) continue;
                            if(infoExt.HasTrackLane(laneIndex) || infoExt2.HasTrackLane(routing.laneIndex)) {
                                if(GoodTurnAngle(segmentID, routing.segmentId, NodeID)) {
                                    tempConnections_.Add(new Connection { LaneID1 = laneID, LaneID2 = routing.laneId });
                                }
                            }
                        }
                    }
                }

                int n = tempConnections_.Count;
                var transitions = new LaneTransition[n];
                int n2 = n >> 1; // n/2
                int index = 0;
                foreach(var connection in tempConnections_) {
                    transitions[index++].Init(connection.LaneID1, connection.LaneID2, index - n2); // also calculates
                }
                Transitions = transitions;
                if(Log.VERBOSE) Log.Debug($"NetNodeExt.GetTrackConnections() succeeded for node:{NodeID} transitions.len={transitions.Length}", false);
            } catch(Exception ex) {
                throw ex;
            }
        }

        public static bool IsNodeless(ushort segmentID, ushort nodeID) {
            ref var corner = ref segmentID.ToSegmentExt().GetEnd(nodeID).Corner;
            var center = (corner.Left.Position + corner.Right.Position) * 0.5f;
            var diff = center - nodeID.ToNode().m_position;
            return VectorUtils.LengthSqrXZ(diff) < 0.00001f;
        }

        public static bool GoodTurnAngle(ushort segmentID1, ushort segmentID2, ushort nodeID) {
            ref var segment1 = ref segmentID1.ToSegment();
            ref var segment2 = ref segmentID2.ToSegment();
            var info1 = segment1.Info;
            var info2 = segment2.Info;
            var dir1 = segment1.GetDirection(nodeID);
            var dir2 = segment2.GetDirection(nodeID);

            var dot = VectorUtils.DotXZ(dir1, dir2);
            float maxTurnAngleCos = Mathf.Min(info1.m_maxTurnAngleCos, info2.m_maxTurnAngleCos);
            float turnThreshold = 0.01f - maxTurnAngleCos;
            return dot < turnThreshold;
        }

        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo, int layerMask) {
            if(!NodeID.ToNode().IsValid())
                return;
            if(!NodeID.ToNode().Info.CheckNetLayers(layerMask))
                return;
            if(Transitions.IsNullorEmpty())
                return;
            foreach(var transition in Transitions)
                transition.RenderTrackInstance(cameraInfo);
        }

        public bool CalculateGroupData(int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            if(!NodeID.ToNode().IsValid())
                return false;
            if(!NodeID.ToNode().Info.CheckNetLayers(1 << layer))
                return false;
            if(Transitions.IsNullorEmpty())
                return false;
            bool result = false;
            foreach(var transtion in Transitions) {
                result |= transtion.CalculateGroupData(ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
            }
            return result;
        }
        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            if(!NodeID.ToNode().IsValid())
                return;
            if(!NodeID.ToNode().Info.CheckNetLayers(1 << layer))
                return;
            if(Transitions.IsNullorEmpty())
                return;
            foreach(var transtion in Transitions) {
                transtion.PopulateGroupData(groupX, groupZ, ref vertexIndex, ref triangleIndex, groupPosition, data);
            }
        }
        #endregion
    }
}

