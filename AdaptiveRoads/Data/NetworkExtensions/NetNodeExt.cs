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

            [Hint("has continues junction median between at least a pair of segments (provided that there are DC nodes)")]
            HasUnbrokenMedian = 1L << 18,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1L << 31,
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
                    if (NodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.Created))
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

                    {
                        bool hasUnbrokenMedian = false;
                        foreach (ushort segmentId in SegmentIDs) {
                            if (segmentId.ToSegment().Info?.m_netAI is RoadBaseAI) {
                                hasUnbrokenMedian |= DirectConnectUtil.HasUnbrokenMedian(segmentID: segmentId, nodeID: NodeID);
                            }
                        }
                        m_flags = m_flags.SetFlags(Flags.HasUnbrokenMedian, hasUnbrokenMedian);
                    }

                    GetTrackConnections();
                }
                if (Log.VERBOSE) Log.Debug($"NetNodeExt.UpdateFlags() succeeded for {this}" /*Environment.StackTrace*/, false);
            } catch (Exception ex) {
                ex.Log("node=" + this);
            }
        }

        public void ShiftAndRotatePillar() {
            NetInfo info = VanillaNode.Info;
            ushort buildingId = VanillaNode.m_building;
            ref var building = ref BuildingManager.instance.m_buildings.m_buffer[VanillaNode.m_building];
            bool isValid = info != null && buildingId != 0 &&
                (building.m_flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created;
            if (!isValid || !info.IsAdaptive())
                return;

            /************************
            /* shift */
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

            /************************
            /* rotate */
            Vector3 dir;
            if (SegmentIDs.IsNullorEmpty()) {
                return;
            } else if (SegmentIDs.Length == 1) {
                ref var segment = ref SegmentIDs[0].ToSegment();
                dir = segment.GetDirection(NodeID);
                if (segment.GetHeadNode() == NodeID)
                    dir = -dir;
            } else {
                var sortedSegments = SegmentIDs.ToList();
                sortedSegments.Sort(CompareSegments);

                if (sortedSegments[0].ToSegment().GetHeadNode() != NodeID &&
                   sortedSegments[1].ToSegment().GetHeadNode() == NodeID) {
                    sortedSegments.Swap(0, 1);
                }

                var dir0 = -sortedSegments[0].ToSegment().GetDirection(NodeID);
                var dir1 = sortedSegments[1].ToSegment().GetDirection(NodeID);
                dir = (dir0 + dir1) * 0.5f;
            }

            float angle = Mathf.Atan2(dir.z, dir.x);
            angle += Mathf.PI * 0.5f;

            BuildingUtil.RelocatePillar(buildingId, center, angle);

            static int CompareSegments(ushort seg1Id, ushort seg2Id) {
                ref NetSegment seg1 = ref seg1Id.ToSegment();
                ref NetSegment seg2 = ref seg2Id.ToSegment();
                int diff = (int)Mathf.RoundToInt(seg2.Info.m_halfWidth - seg1.Info.m_halfWidth);
                if (diff == 0) {
                    diff = CountRoadVehicleLanes(seg2Id) - CountRoadVehicleLanes(seg1Id);
                }
                return diff;
            }

            static int CountRoadVehicleLanes(ushort segmentId) {
                ref NetSegment segment = ref segmentId.ToSegment();
                int forward = 0, backward = 0;
                segment.CountLanes(
                    segmentId,
                    NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                    VehicleInfo.VehicleType.Car,
                    VehicleInfo.VehicleCategory.All,
                    ref forward,
                    ref backward);
                return forward + backward;
            }
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

                if (transitions_.IsNullorEmpty())
                    return;
                for (int i = 0; i < transitions_.Length; ++i) {
                    transitions_[i].UpdateScriptedFlags(i);
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

            public override string ToString() => $"lane connection key {LaneID1}->{LaneID2}";
        }

        private static HashSet<Connection> tempConnections_ = new (Connection.Comparer);
        private LaneTransition[] transitions_;
        public ref LaneTransition GetLaneTransition(int index) => ref transitions_[index];
        

        public void GetTrackConnections() {
            try {
                if(Log.VERBOSE) Log.Called();
                transitions_ = null;
                ref var node = ref NodeID.ToNode();
                if(!node.IsValid())
                    return;
                    if(!node.m_flags.IsFlagSet(NetNode.Flags.Junction | NetNode.Flags.Bend))
                        return;

                tempConnections_.Clear();

                // TMPE lane transitions
                foreach(var segmentID in NodeID.ToNode().IterateSegments()) {
                    var infoExt = segmentID.ToSegment().Info?.GetMetaData();
                    var lanes = new LaneIDIterator(segmentID).ToArray();
                    for(int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                        bool hasTrackLane = infoExt?.HasTrackLane(laneIndex) ?? false;
                        uint laneID = lanes[laneIndex];
                        var laneInfo = segmentID.ToSegment().Info.m_lanes[laneIndex];
                        var routings = TMPEHelpers.GetForwardRoutings(laneID, NodeID);
                        if(routings == null) continue;
                        if(IsNodeless(segmentID: segmentID, nodeID: NodeID)) continue;
                        //Log.Debug($"routings for lane:{laneID} are " + routings.ToSTR());
                        foreach(LaneTransitionData routing in routings) {
                            if(routing.type is LaneEndTransitionType.Invalid or LaneEndTransitionType.Relaxed)
                                continue;
                            var infoExt2 = routing.segmentId.ToSegment().Info?.GetMetaData();
                            bool hasTrackLane2 = infoExt2?.HasTrackLane(routing.laneIndex) ?? false;
                            if (!(hasTrackLane || hasTrackLane2)) {
                                continue;
                            }

                            var laneInfo2 = routing.laneId.ToLane().m_segment.ToSegment().Info.m_lanes[routing.laneIndex];
                            if (LanesConnect(laneInfo, laneInfo2, routing.group)) {
                                if (IsNodeless(segmentID: routing.segmentId, nodeID: NodeID)) continue;
                                var key = new Connection { LaneID1 = laneID, LaneID2 = routing.laneId };
                                //Log.Debug("routed " + key);
                                tempConnections_.Add(key);
                            }
                        }
                    }
                }

                // bicycle transitions
                {
                    foreach (ushort segmentId1 in SegmentIDs) {
                        //Log.Debug($"source: {segmentId1}");
                        ref NetSegment segment1 = ref segmentId1.ToSegment();
                        bool headNode1 = segment1.GetHeadNode() == NodeID;
                        var infoExt1 = segment1.Info?.GetMetaData();
                        foreach (var lane1 in new LaneDataIterator(segmentId1, null, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Bicycle)) {
                            bool hasTrackLane1 = infoExt1?.HasTrackLane(lane1.LaneIndex) ?? false;
                            NetInfo.Direction outoingDir = headNode1 ? NetInfo.Direction.Forward : NetInfo.Direction.Backward;
                            bool outgoing = lane1.LaneInfo.m_finalDirection.IsFlagSet(outoingDir);
                            //Log.Debug($"{lane1.LaneID} outgoing={outgoing}");
                            if (!outgoing) {
                                continue;
                            }

                            foreach (ushort segmentId2 in SegmentIDs) {
                                if (segmentId2 == segmentId1) continue;
                                //Log.Debug($"target: {segmentId2}");
                                ref NetSegment segment2 = ref segmentId2.ToSegment();
                                var infoExt2 = segment2.Info?.GetMetaData();
                                bool headNode2 = segment2.GetHeadNode() == NodeID;
                                foreach (var lane2 in new LaneDataIterator(segmentId2, null, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Bicycle)) {
                                    bool hasTrackLane2 = infoExt2?.HasTrackLane(lane2.LaneIndex) ?? false;
                                    if (!(hasTrackLane1 || hasTrackLane2)) {
                                        continue;
                                    }

                                    NetInfo.Direction incommingDir = headNode2 ? NetInfo.Direction.Backward : NetInfo.Direction.Forward;
                                    bool incoming = lane2.LaneInfo.m_finalDirection.IsFlagSet(incommingDir);
                                    //Log.Debug($"{lane2.LaneID} incoming={incoming}");
                                    if (!incoming) {
                                        continue;
                                    }

                                    //Log.Debug($"{lane1} -> {lane2}");
                                    var key = new Connection { LaneID1 = lane1.LaneID, LaneID2 = lane2.LaneID };
                                    //Log.Debug("bicycle " + key);
                                    tempConnections_.Add(key);
                                }
                            }
                        }
                    }
                }

                // pedestrian transitions
                {
                    //Log.Debug("pedestrian transitions at node:" + NodeID);
                    foreach (ushort segmentId1 in SegmentIDs) {
                        //Log.Debug($"source: {segmentId1}");
                        ref NetSegment segment1 = ref segmentId1.ToSegment();
                        bool headNode1 = segment1.GetHeadNode() == NodeID;
                        var infoExt1 = segment1.Info?.GetMetaData();
                        foreach (var lane1 in new LaneDataIterator(segmentId1, null, NetInfo.LaneType.Pedestrian)) {
                            if (lane1.LaneInfo.m_centerPlatform) continue;
                            bool hasTrackLane1 = infoExt1?.HasTrackLane(lane1.LaneIndex) ?? false;
                            //Log.Debug($"lane1={lane1.LaneID}");
                            foreach (ushort segmentId2 in SegmentIDs) {
                                if (segmentId2 == segmentId1) continue;
                                //Log.Debug(message: $"target: {segmentId2}");
                                ref NetSegment segment2 = ref segmentId2.ToSegment();
                                var infoExt2 = segment2.Info?.GetMetaData();
                                bool headNode2 = segment2.GetHeadNode() == NodeID;
                                foreach (var lane2 in new LaneDataIterator(segmentId2, null, NetInfo.LaneType.Pedestrian)) {
                                    if (lane2.LaneInfo.m_centerPlatform) continue;
                                    //Log.Debug($"lane2={lane2.LaneID}");
                                    bool hasTrackLane2 = infoExt2?.HasTrackLane(lane2.LaneIndex) ?? false;
                                    if (!(hasTrackLane1 || hasTrackLane2)) {
                                        //Log.Debug($"skipping hasTrackLane1={hasTrackLane1} hasTrackLane2={hasTrackLane2}");
                                        continue;
                                    }

                                    if (!RoadUtils.IsNearCurb(lane1, lane2, NodeID)) {
                                        //Log.Debug($"skipping because {lane1.LaneID} -> {lane2.LaneID} is not near curb");
                                        continue;
                                    }

                                    //Log.Debug($"{lane1} -> {lane2}");
                                    var key = new Connection { LaneID1 = lane1.LaneID, LaneID2 = lane2.LaneID };
                                    //Log.Debug("pedestrian " + key);
                                    tempConnections_.Add(key);
                                }
                            }
                        }
                    }
                }

                // None Transitions based on lane tags
                {
                    foreach (ushort segmentId1 in SegmentIDs) {
                        ref NetSegment segment1 = ref segmentId1.ToSegment();
                        var infoExt1 = segment1.Info?.GetMetaData();
                        bool headNode1 = segment1.GetHeadNode() == NodeID;
                        foreach (var lane1 in new LaneDataIterator(segmentId1)) {
                            if (lane1.LaneInfo.m_laneType != NetInfo.LaneType.None) continue;
                            bool hasTrackLane1 = infoExt1?.HasTrackLane(lane1.LaneIndex) ?? false;
                            foreach (ushort segmentId2 in SegmentIDs) {
                                if (segmentId2 == segmentId1) continue;
                                ref NetSegment segment2 = ref segmentId2.ToSegment();
                                var infoExt2 = segment2.Info?.GetMetaData();
                                foreach (var lane2 in new LaneDataIterator(segmentId2)) {
                                    if (lane2.LaneInfo.m_laneType != NetInfo.LaneType.None) continue;
                                    bool hasTrackLane2 = infoExt2?.HasTrackLane(lane2.LaneIndex) ?? false;
                                    if (!(hasTrackLane1 || hasTrackLane2)) continue;
                                    
                                    if (CheckTagsNoneLanes(lane1, lane2)) {
                                        Log.Debug($"{lane1} -> {lane2}");
                                        var key = new Connection { LaneID1 = lane1.LaneID, LaneID2 = lane2.LaneID };
                                        Log.Debug(message: "NONE " + key);
                                        tempConnections_.Add(key);
                                    }                                }
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
                //Log.Debug(transitions_.ToSTR());
                if(Log.VERBOSE) Log.Debug($"NetNodeExt.GetTrackConnections() succeeded for node:{NodeID} transitions.len={transitions.Length}", false);
            } catch(Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// checks if any track has lane tags of the other lane.
        /// </summary>
        public static bool CheckTagsNoneLanes(LaneData lane1, LaneData lane2) {
            return CheckAB(lane1, lane2) || CheckAB(lane2, lane1);
            static bool CheckAB(LaneData laneA, LaneData laneB) {
                var infoExtA = laneA.Segment.Info?.GetMetaData();
                var infoExtB = laneB.Segment.Info?.GetMetaData();
                var tracksA = infoExtA?.Tracks;
                var tagsB = infoExtB?.Lanes[laneB.LaneInfo]?.LaneTags;

                if (tracksA != null && tagsB != null) {
                    foreach (var track in tracksA) {
                        if (track == null) continue;
                        if (track.HasTrackLane(laneA.LaneIndex)) {
                            if (track.LaneTags.Check(tagsB.Flags)) {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        public static bool IsNodeless(ushort segmentID, ushort nodeID) {
            ref var corner = ref segmentID.ToSegmentExt().GetEnd(nodeID).Corner;
            var center = (corner.Left.Position + corner.Right.Position) * 0.5f;
            var diff = center - nodeID.ToNode().m_position;
            return VectorUtils.LengthSqrXZ(diff) < 0.00001f;
        }

        private bool LanesConnect(NetInfo.Lane laneInfo1, NetInfo.Lane laneInfo2, LaneEndTransitionGroup group) {
            if (!laneInfo1.CheckType(laneInfo2.m_laneType, laneInfo2.m_vehicleType, laneInfo2.vehicleCategory)) {
                // can't connect
                return false;
            }

            var group1 = laneInfo1.GetLaneEndTransitionGroup();
            var group2 = laneInfo2.GetLaneEndTransitionGroup();
            if (!group.IsFlagSet(group1) || !group.IsFlagSet(group2)) {
                // workaround: TMPE redundant connections
                return false;
            }

            bool track1 = laneInfo1.MatchesTrack();
            bool track2 = laneInfo2.MatchesTrack();
            bool trackRouting = group.IsFlagSet(LaneEndTransitionGroup.Track);
            if ((track1 || track2) && !trackRouting) {
                // car+tram lanes can't connect to car lanes
                return false;
            }

            return true;
        }

        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo, int layerMask) {
            if(!NodeID.ToNode().IsValid())
                return;
            if(!NodeID.ToNode().Info.CheckNetLayers(layerMask))
                return;
            if(transitions_.IsNullorEmpty())
                return;
            foreach(var transition in transitions_)
                transition.RenderTrackInstance(cameraInfo, layerMask);
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

        public static NetNode.Flags CalculateDCAsymFlags(ushort nodeId, ushort segmentId1, ushort segmentId2) {
            CountLanes(nodeId, segmentId1, out int toward1, out int away1);
            CountLanes(nodeId, segmentId2, out int toward2, out int away2);
            if (toward1 > away1 && toward2 > away2)
                return NetNode.Flags.AsymBackward;
            else if (toward1 < away1 && toward2 < away2)
                return NetNode.Flags.AsymForward;
            else
                return default;

            static void CountLanes(ushort nodeId, ushort segmentId, out int toward, out int away) {
                ref NetSegment segment = ref segmentId.ToSegment();
                bool startNode = segment.IsStartNode(nodeId);
                bool invert = segment.m_flags.IsFlagSet(NetSegment.Flags.Invert);
                bool reverse = startNode ^ invert;
                NetInfo netInfo = segment.Info;
                if (!reverse) {
                    away = netInfo.m_backwardVehicleLaneCount;
                    toward = netInfo.m_forwardVehicleLaneCount;
                } else {
                    away = netInfo.m_forwardVehicleLaneCount;
                    toward = netInfo.m_backwardVehicleLaneCount;
                }
            }
        }
        #endregion
    }
}

