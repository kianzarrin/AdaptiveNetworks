namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.CustomScript;
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
    using static AdaptiveRoads.Util.Shortcuts;

    public struct NetNodeExt {
        public ushort NodeID;
        public Flags m_flags;
        #region expression for dummies
        public ref NetNode VanillaNode => ref NodeID.ToNode();
        public ushort[] SegmentIDs => new NodeSegmentIterator(NodeID).ToArray();
        #endregion

        const int CUSTOM_FLAG_SHIFT = 24;
        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;
        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(ushort nodeID) {
            this = default;
            NodeID = nodeID;
        }

        [Flags]
        public enum Flags : Int64 {
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

            [Hint("all segments at the junction have same prefab")]
            SamePrefab = 1 << 13,

            [Hint("node has lane connections (car/track, outgoing/incoming/dead-end)")]
            LaneConnections = 1 << 14,

            [Hint("number of incoming lanes incoming toward the node is equal to outgoing lanes")]
            EqualLaneCount = 1 << 15,

            [Hint("number of incoming lanes incoming toward the node is exactly one more than outgoing lanes")]
            OneExtraIncommingLane = 1 << 16,

            [Hint("number of incoming lanes incoming toward the node is one less than outgoing lanes")]
            OneExtraOutgoingLane = 1 << 17,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1 << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,

            [ExpressionFlag] Expression0 = 1L << 32,
            [ExpressionFlag] Expression1 = 1L << 33,
            [ExpressionFlag] Expression2 = 1L << 34,
            [ExpressionFlag] Expression3 = 1L << 35,
            [ExpressionFlag] Expression4 = 1L << 36,
            [ExpressionFlag] Expression5 = 1L << 37,
            [ExpressionFlag] Expression6 = 1L << 38,
            [ExpressionFlag] Expression7 = 1L << 39,
            ExpressionMask = Expression0 | Expression1 | Expression2 | Expression3 | Expression4 | Expression5 | Expression6 | Expression7,
        }

        private void HandleInvalidNode() => Init(NodeID);

        public void UpdateFlags() {
            try {
                transitions_ = null;
                if(!NetUtil.IsNodeValid(NodeID)) {
                    if (NodeID.ToNode().m_flags.IsFlagSet(NetSegment.Flags.Created))
                        Log.Debug("Skip updating invalid node:" + NodeID);
                    else
                        HandleInvalidNode();
                    return;
                }
                m_flags = m_flags.SetFlags(Flags.HC_Mod, NetworkExtensionManager.Instance.HTC);
                m_flags = m_flags.SetFlags(Flags.DCR_Mod, NetworkExtensionManager.Instance.DCR);
                m_flags = m_flags.SetFlags(Flags.HUT_Mod, NetworkExtensionManager.Instance.HUT);

                var info = VanillaNode.Info;
                bool samePrefab = SegmentIDs.All(item=>item.ToSegment().Info == info);
                m_flags = m_flags.SetFlags(Flags.SamePrefab, samePrefab);

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
                    m_flags = m_flags.SetFlags(Flags.LaneConnections, LCMan.HasNodeConnections(NodeID));

                    {
                        LaneHelpers.CountNodeLanes(NodeID, out int incoming, out int outgoing);
                        m_flags = m_flags.SetFlags(Flags.EqualLaneCount, incoming == outgoing);
                        m_flags = m_flags.SetFlags(Flags.OneExtraIncommingLane, incoming + 1 == outgoing);
                        m_flags = m_flags.SetFlags(Flags.OneExtraOutgoingLane, incoming == outgoing + 1);
                    }


                    GetTrackConnections();
                    ShiftPilar();
                    if (Log.VERBOSE) Log.Debug($"NetNodeExt.UpdateFlags() succeeded for {this}" /*Environment.StackTrace*/, false);
                }
            } catch(Exception ex) {
                ex.Log("node=" + this);
            }
        }

        public void ShiftPilar() {
            NetInfo info = VanillaNode.Info;
            ushort buildingId = VanillaNode.m_building;
            ref var building = ref BuildingManager.instance.m_buildings.m_buffer[VanillaNode.m_building];
            bool isValid = info != null && buildingId != 0 &&
                (building.m_flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created;
            if (!isValid || !info.IsAdaptive())
                return;
            info.m_netAI.GetNodeBuilding(NodeID, ref VanillaNode, out BuildingInfo buildingInfo, out float heightOffset);
            Vector3 center = default;
            int counter = 0;
            foreach(var segmentId in SegmentIDs) {
                ref NetSegmentEnd segmentEnd = ref segmentId.ToSegmentExt().GetEnd(NodeID);
                center += segmentEnd.Corner.Left.Position + segmentEnd.Corner.Right.Position;
                counter += 2;
            }
            center /= counter;
            center.y += heightOffset;
            building.m_position = center;
        }

        public void UpdateScriptedFlags() {
            try {
                var net = NodeID.ToNode().Info?.GetMetaData();
                if (net == null) return;
                foreach (var scriptedFlag in Flags.ExpressionMask.ExtractPow2Flags()) {
                    bool condition = false;
                    if (net.ScriptedFlags.TryGetValue(scriptedFlag, out var expression)) {
                        condition = expression.Condition(segmentID: 0, nodeID: NodeID);
                    }
                    m_flags = m_flags.SetFlags(scriptedFlag, condition);
                }
            } catch (Exception ex) {
                ex.Log();
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
        private struct Connection : IEquatable<Connection> {
            public uint LaneID1;
            public uint LaneID2;
            public override int GetHashCode() => (int)(LaneID1 ^ LaneID2);
            public override bool Equals(object obj) => obj is Connection rhs && this.Equals(rhs);
            public bool Equals(Connection rhs) {
                if (LaneID1 == rhs.LaneID1 && LaneID2 == rhs.LaneID2)
                    return true;
                else if (LaneID1 == rhs.LaneID2 && LaneID2 == rhs.LaneID1)
                    return true;
                else
                    return false;
            }

            public static IEqualityComparer<Connection> Comparer { get; } = new EqualityComparer();

            private sealed class EqualityComparer : IEqualityComparer<Connection> {
                public bool Equals(Connection x, Connection y) => x.Equals(y);
                public int GetHashCode(Connection obj) => obj.GetHashCode();
            }
        }

        private static HashSet<Connection> tempConnections_ = new (Connection.Comparer);
        private LaneTransition[] transitions_;
        public void GetTrackConnections() {
            try {
                transitions_ = null;
                ref var node = ref NodeID.ToNode();
                if(!node.IsValid())
                    return;
                    if(!node.m_flags.IsFlagSet(NetNode.Flags.Junction | NetNode.Flags.Bend))
                        return;

                tempConnections_.Clear();
                foreach(var segmentID in NodeID.ToNode().IterateSegments()) {
                    var infoExt = segmentID.ToSegment().Info?.GetMetaData();
                    //if(infoExt == null) continue;
                    var lanes = new LaneIDIterator(segmentID).ToArray();
                    for(int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                        uint laneID = lanes[laneIndex];
                        var routings = TMPEHelpers.GetForwardRoutings(laneID, NodeID);
                        if(routings == null) continue;
                        if(IsNodeless(segmentID: segmentID, nodeID: NodeID)) continue;
                        //Log.Debug($"routigns for lane:{laneID} are " + routings.ToSTR());
                        foreach(LaneTransitionData routing in routings) {
                            if(routing.type == LaneEndTransitionType.Invalid /*|| routing.type == LaneEndTransitionType.Relaxed*/)
                                continue;
                            var infoExt2 = routing.segmentId.ToSegment().Info?.GetMetaData();
                            //if(infoExt2 == null) continue;
                            if(IsNodeless(segmentID: routing.segmentId, nodeID: NodeID)) continue;
                            bool hasTrackLane = infoExt != null && infoExt.HasTrackLane(laneIndex);
                            bool hasTrackLane2 = infoExt2 != null && infoExt2.HasTrackLane(routing.laneIndex);
                            if(hasTrackLane || hasTrackLane2) {
                                if(LanesConnect(laneID, routing.laneId)) {
                                    var key = new Connection { LaneID1 = laneID, LaneID2 = routing.laneId };
                                    tempConnections_.Add(key);
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
                    transitions[index++].Init(connection.LaneID1, connection.LaneID2, NodeID, index - n2); // also calculates
                }
                transitions_ = transitions;
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

        private bool GoodTurnAngle(ushort segmentID1, ushort segmentID2) {
            if(segmentID1 == segmentID2)
                return false; // u-turn.
            ref var segment1 = ref segmentID1.ToSegment();
            ref var segment2 = ref segmentID2.ToSegment();
            var info1 = segment1.Info;
            var info2 = segment2.Info;
            var dir1 = segment1.GetDirection(NodeID);
            var dir2 = segment2.GetDirection(NodeID);

            var dot = VectorUtils.DotXZ(dir1, dir2);
            float maxTurnAngleCos = Mathf.Min(info1.m_maxTurnAngleCos, info2.m_maxTurnAngleCos);
            float turnThreshold = 0.01f - maxTurnAngleCos;
            return dot < turnThreshold;
        }

        private bool LanesConnect(uint laneIDSource, uint laneIDTarget) {
            ref var lane = ref laneIDSource.ToLaneExt().LaneData;
            ref var lane2 = ref laneIDTarget.ToLaneExt().LaneData;
            bool hasTracks = lane.LaneInfo.m_vehicleType.IsFlagSet(NetInfoExtionsion.Track.TRACK_VehicleTypes);
            bool hasTracks2 = lane2.LaneInfo.m_vehicleType.IsFlagSet(NetInfoExtionsion.Track.TRACK_VehicleTypes);
            if(hasTracks != hasTracks2)
                return false;

            if(hasTracks) {
                return GoodTurnAngle(lane.SegmentID, lane2.SegmentID);
            }

            return true; // bike
        }

        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo, int layerMask) {
            if(!NodeID.ToNode().IsValid())
                return;
            if(!NodeID.ToNode().Info.CheckNetLayers(layerMask))
                return;
            if(transitions_.IsNullorEmpty())
                return;
            foreach(var transition in transitions_)
                transition.RenderTrackInstance(cameraInfo);
        }

        public bool CalculateGroupData(int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            if(!NodeID.ToNode().IsValid())
                return false;
            if(!NodeID.ToNode().Info.CheckNetLayers(1 << layer))
                return false;
            if(transitions_.IsNullorEmpty())
                return false;
            bool result = false;
            foreach(var transtion in transitions_) {
                result |= transtion.CalculateGroupData(layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
            }
            return result;
        }
        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            if(!NodeID.ToNode().IsValid())
                return;
            if(!NodeID.ToNode().Info.CheckNetLayers(1 << layer))
                return;
            if(transitions_.IsNullorEmpty())
                return;
            foreach(var transtion in transitions_) {
                transtion.PopulateGroupData(groupX, groupZ, layer: layer, ref vertexIndex, ref triangleIndex, groupPosition, data);
            }
        }
        #endregion
    }
}

