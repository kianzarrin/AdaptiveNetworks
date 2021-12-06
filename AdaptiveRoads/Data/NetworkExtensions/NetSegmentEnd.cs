namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using CSUtil.Commons;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Linq;
    using TrafficManager;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using AdaptiveRoads.Data.NetworkExtensions;
    using UnityEngine;
    using Log = KianCommons.Log;

    public struct NetSegmentEnd {
        #region shortcuts for dummies
        public ref NetNodeExt Node => ref NodeID.ToNodeExt();
        public ref NetSegmentExt Segment => ref SegmentID.ToSegmentExt();
        #endregion

        [Flags]
        public enum Flags : Int64 {
            None = 0,

            [Hide]
            [Hint("[Obsolete] " + HintExtension.VANILLA)]
            Vanilla = 1 << 0,            // priority signs
            [Hint("checks if TMPE rules requires vehicles to yield to upcoming traffic\n" +
                "different than the vanilla YieldStart/YieldEnd (Stop) flag.")]
            Yield = 1 << 4,

            [Hint("checks if TMPE rules requires vehicles to Stop at junctions\n" +
                "different than the vanilla YieldStart/YieldEnd (Stop) flag.")]
            Stop = 1 << 5,

            [Hint("checks if TMPE rules gives priority to vehicles on this segment-end")]
            PriorityMain = 1 << 6,

            // junction restrictions.
            [Hint("TMPE allows pedestrian to cross.")]
            ZebraCrossing = 1 << 7,

            [Hint("TMPE bans vehicles from entering blocked junction (requires them to keep clear)")]
            KeepClear = 1 << 8,

            [Hint("vehicles can take near turn (right turn with Right Hand Traffic) even when traffic light is red")]
            NearTurnAtRed = 1 << 9,

            [Hint("in a one-way road vehicles can take far turn even when traffic light is red\n" +
                "far turn is left turn with right hand traffic")]
            FarTurnAtRed = 1 << 10,

            [Hint("vehicles can change lanes in the middle of the junction")]
            LaneChangingGoingStraight = 1 << 11,

            [Hint("cars can make a U-turn at this segment-end\n" +
                "(provided that there is a lane with left lane arrow)")]
            Uturn = 1 << 12,

            // directions
            [Hint("there is a segment to the right (regardless of lane arrows or segment's direction)")]
            HasRightSegment = 1 << 13,

            [Hint("TMPE lane arrow manager allows at least one lane to turn right")]
            CanTurnRight = 1 << 14,

            [Hint("there is a segment to the left (regardless of lane arrows or segment's direction)")]
            HasLeftSegment = 1 << 15,

            [Hint("TMPE lane arrow manager allows at least one lane to turn left")]
            CanTurnLeft = 1 << 16,

            [Hint("TMPE lane arrow manager allows at least one lane to go straight")]
            HasForwardSegment = 1 << 17,
            CanGoForward = 1 << 18,

            [Hint("the start node from which road is placed (does not take into account StartNode/LHT/Invert)")]
            IsStartNode = 1 << 19,

            [Hint("traffic drives from tail node to head node (takes into account StartNode/LHT/Invert)")]
            IsTailNode = 1 << 20,

            [Hide]
            [Hint("[Obsolete] the junction only has two segments.\n")]
            [Obsolete("moved to node")]
            TwoSegments = 1 << 21,

            [Hide]
            [Hint("[Obsolete] the junction has segments with different speed limits.\n")]
            [Obsolete("moved to node")]
            SpeedChange = 1 << 22,

            [Hint("next segment has more lanes (only valid when there are two segments)")]
            LanesIncrase = 1L << 32,

            [Hint("next segment has more lanes (only valid when there are two segments)")]
            LanesDecrease = 1L << 33,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1L << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,
        }

        public Flags m_flags;
        public ushort SegmentID;
        public bool StartNode;

        public ushort NodeID => SegmentID.ToSegment().GetNode(StartNode);
        public NetSegmentExt[] Segments => NodeID.ToNode().IterateSegments()
            .Select(_segmentId => NetworkExtensionManager.Instance.SegmentBuffer[_segmentId]).ToArray();

        const int CUSTOM_FLAG_SHIFT = 24;
        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;
        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(ushort segmentID, bool startNode) {
            m_flags = Flags.None;
            SegmentID = segmentID;
            StartNode = startNode;
        }

        public static JunctionRestrictionsManager JRMan => JunctionRestrictionsManager.Instance;
        public static TrafficPriorityManager PMan => TrafficPriorityManager.Instance;

        public void UpdateFlags() {
            var flags = m_flags;

            if(PMan != null) {
                PriorityType p = PMan.GetPrioritySign(SegmentID, StartNode);
                flags = flags.SetFlags(Flags.Yield, p == PriorityType.Yield);
                flags = flags.SetFlags(Flags.Stop, p == PriorityType.Stop);
                flags = flags.SetFlags(Flags.PriorityMain, p == PriorityType.Main);
            }

            if(JRMan != null) {
                flags = flags.SetFlags(Flags.KeepClear, !JRMan.IsEnteringBlockedJunctionAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.ZebraCrossing, JRMan.IsPedestrianCrossingAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.NearTurnAtRed, JRMan.IsNearTurnOnRedAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.FarTurnAtRed, JRMan.IsFarTurnOnRedAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.Uturn, JRMan.IsUturnAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.LaneChangingGoingStraight, JRMan.IsLaneChangingAllowedWhenGoingStraight(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.LaneChangingGoingStraight, JRMan.IsLaneChangingAllowedWhenGoingStraight(SegmentID, StartNode));
            }
            flags = flags.SetFlags(Flags.IsStartNode, StartNode);
            flags = flags.SetFlags(Flags.IsTailNode, NetUtil.GetTailNode(SegmentID) == NodeID);

            bool speedChange = TMPEHelpers.SpeedChanges(NodeID);
            bool twoSegments = NodeID.ToNode().CountSegments() == 2;

#pragma warning disable CS0618 // Type or member is obsolete
            flags = flags.SetFlags(Flags.SpeedChange, speedChange);
            flags = flags.SetFlags(Flags.TwoSegments, twoSegments);
#pragma warning restore CS0618 // Type or member is obsolete

            bool lanesIncrease = false, lanesDecrease = false;
            if(twoSegments) {
                var sourceLanes = new LaneDataIterator(
                    SegmentID,
                    StartNode, // going toward node:NodeID
                    LaneArrowManager.LANE_TYPES,
                    LaneArrowManager.VEHICLE_TYPES);
                var segmentID2 = NodeID.ToNode().GetAnotherSegment(SegmentID);
                bool startNode2 = segmentID2.ToSegment().IsStartNode(NodeID);
                var targetLanes = new LaneDataIterator(
                    segmentID2,
                    !startNode2, // lanes that are going away from node:NodeID
                    LaneArrowManager.LANE_TYPES,
                    LaneArrowManager.VEHICLE_TYPES);
                int nSource = sourceLanes.Count;
                int nTarget = targetLanes.Count;
                lanesIncrease = nTarget > nSource;
                lanesDecrease = nTarget < nSource;
            }
            flags = flags.SetFlags(Flags.LanesIncrase, lanesIncrease);
            flags = flags.SetFlags(Flags.LanesDecrease, lanesDecrease);

            m_flags = flags;
        }

        public override string ToString() {
            return $"NetSegmentEnd(segment:{SegmentID} node:{NodeID} StartNode:{StartNode} flags={m_flags})";
        }

        public void UpdateDirections() {
            CheckSegmentsInEachDirection(
                segmentId: SegmentID, nodeId: NodeID,
                right: out bool right, forward: out bool forward, left: out bool left);
            m_flags = m_flags.SetFlags(Flags.HasRightSegment, right);
            m_flags = m_flags.SetFlags(Flags.HasLeftSegment, left);
            m_flags = m_flags.SetFlags(Flags.HasForwardSegment, forward);

            LaneArrows arrows = AllArrows(SegmentID, StartNode);
            m_flags = m_flags.SetFlags(Flags.CanGoForward, arrows.IsFlagSet(LaneArrows.Forward));
            m_flags = m_flags.SetFlags(Flags.CanTurnRight, arrows.IsFlagSet(LaneArrows.Right));
            m_flags = m_flags.SetFlags(Flags.CanTurnLeft, arrows.IsFlagSet(LaneArrows.Left));
        }

        private static LaneArrows AllArrows(ushort segmentId, bool startNode) {
            LaneArrows ret = LaneArrows.None;
            foreach(var lane in NetUtil.IterateLanes(
                segmentId: segmentId, startNode: startNode,
                laneType: LaneArrowManager.LANE_TYPES, vehicleType: LaneArrowManager.VEHICLE_TYPES)) {
                LaneArrows arrows = LaneArrowManager.Instance.GetFinalLaneArrows(lane.LaneID);
                ret |= arrows;
            }
            return ret;
        }

        private static void CheckSegmentsInEachDirection(
            ushort segmentId, ushort nodeId,
            out bool right, out bool forward, out bool left) {
            bool startNode = segmentId.ToSegment().m_startNode == nodeId;
            IExtSegmentEndManager segEndMan = Constants.ManagerFactory.ExtSegmentEndManager;
            ExtSegmentEnd segEnd = segEndMan.ExtSegmentEnds[segEndMan.GetIndex(segmentId, startNode)];

            forward = left = right = false;

            for(int i = 0; i < 8; ++i) {
                ushort otherSegmentId = nodeId.ToNode().GetSegment(i);
                if(otherSegmentId == 0) continue;
                bool isRoad = otherSegmentId.ToSegment().Info.m_netAI is RoadBaseAI;
                if(!isRoad) continue;
                ArrowDirection dir = segEndMan.GetDirection(ref segEnd, otherSegmentId);
                switch(dir) {
                    case ArrowDirection.Forward:
                        forward = true;
                        break;
                    case ArrowDirection.Right:
                        right = true;
                        break;
                    case ArrowDirection.Left:
                        left = true;
                        break;
                    default:
                        break;
                        //throw new Exception("Unreachable Code. dir = " + dir);
                } //end switch
            } // end for
        } // end method

        #region cache corners
        public CornerPairData Corner;
        public float DeltaAngle; // in radians
        public float Angle0; // in radians
        public float TotalAngle => Angle0 + DeltaAngle; // in radians

        /// <summary>
        /// Precondition: SegmentEnd is initialized. (There is no need to have called Updated anything)
        /// </summary>
        public void UpdateCorners() {
            ref var segment = ref SegmentID.ToSegment();
            segment.CalculateCorner(SegmentID, heightOffset: true, start: StartNode, leftSide: true, out Corner.Left.Position, out Corner.Left.Direction, out Corner.smooth);
            segment.CalculateCorner(SegmentID, heightOffset: true, start: StartNode, leftSide: false, out Corner.Right.Position, out Corner.Right.Direction, out Corner.smooth);
            Vector3 v = Corner.Right.Position - Corner.Left.Position;
            Angle0 = (0.5f * Mathf.PI) - Vector3.Angle(Vector3.up, v) * Mathf.Deg2Rad; // angle between 3d vector and the horizontal plane
            //Log.Debug($"UpdateCorners({SegmentID},{NodeID}): Angle0={Angle0} DeltaAngle={DeltaAngle} TotalAngle={TotalAngle} angleUP={Vector3.Angle(Vector3.up, v)}");
        }
        #endregion

    }
}

