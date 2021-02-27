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

    public static class AdvanedFlagsExtensions {
        public static bool CheckFlags(this NetLaneExt.Flags value, NetLaneExt.Flags required, NetLaneExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegmentEnd.Flags value, NetSegmentEnd.Flags required, NetSegmentEnd.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegmentExt.Flags value, NetSegmentExt.Flags required, NetSegmentExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetNodeExt.Flags value, NetNodeExt.Flags required, NetNodeExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
    }

    public struct NetLaneExt {
        [Flags]
        public enum Flags {
            None = 0,

            [Hint("when required, the node will not be rendered when Adaptive Roads mod is enabled.\n" +
                "when forbidden, the node will only be rendered when Adaptive Roads mod is enabled.")]
            Vanilla = 1 << 0,

            ParkingAllowed = 1 << 4,

            // Vehicle restrictions
            [Hint("private cars and motorbikes")]
            Car = 1 << 5,

            [Hint("Emergencies (active sirens)\n" +
                "when siren is off, they act as Serive vehicles.")]
            SOS = 1 << 6,

            Taxi = 1 << 7,

            [Hint("passenger, sightseeing and evacuation buses")]
            Bus = 1 << 8,

            [Hint("all types of industry trucks and vans")]
            CargoTruck = 1 << 9,

            [Hint("all services including emergency services whithout active siren")]
            Service = 1 << 10,

            CargoTrain = 1 << 14,
            PassengerTrain = 1 << 15,

            // misc
            //MergesWithInnerLane = 1 << 17,
            //MergesWithOuterLane = 1 << 18,

            All = -1,
        }

        public Flags m_flags;

        public float SpeedLimit; // game speed limit 1=50kph 20=unlimitted

        //public object OuterMarking;
        //public object InnerMarking;

        public LaneData LaneData;

        public bool IsEmpty => m_flags == Flags.None;
        public void Serialize(DataSerializer s) => s.WriteInt32((int)m_flags);
        public void Deserialize(DataSerializer s) => m_flags = (Flags)s.ReadInt32();
        public void Init(uint laneID) {
            LaneData.LaneID = laneID;
            LaneData.LaneIndex = 0;
            LaneData.LaneInfo = null;
            m_flags = Flags.None;
        }

        static IManagerFactory TMPE => Constants.ManagerFactory;
        static IParkingRestrictionsManager PMan => TMPE?.ParkingRestrictionsManager;
        static IVehicleRestrictionsManager VRMan => TMPE?.VehicleRestrictionsManager;
        static ISpeedLimitManager SLMan => TMPE?.SpeedLimitManager as SpeedLimitManager;

        public void UpdateLane(LaneData lane) {
            Assertion.AssertEqual(LaneData.LaneID, lane.LaneID, "lane id");
            LaneData = lane;

            bool parkingAllowed = LaneData.LaneInfo.m_laneType == NetInfo.LaneType.Parking;
            if(PMan != null)
                parkingAllowed &= PMan.IsParkingAllowed(LaneData.SegmentID, LaneData.LaneInfo.m_finalDirection);
            m_flags = m_flags.SetFlags(Flags.ParkingAllowed, parkingAllowed);

            if(VRMan != null) {
                var mask = VRMan.GetAllowedVehicleTypes(
                    segmentId: LaneData.SegmentID,
                    segmentInfo: LaneData.Segment.Info,
                    laneIndex: (uint)LaneData.LaneIndex,
                    laneInfo: LaneData.LaneInfo,
                    busLaneMode: VehicleRestrictionsMode.Configured);

                m_flags = m_flags.SetFlags(Flags.Car, VRMan.IsPassengerCarAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.SOS, VRMan.IsEmergencyAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.Bus, VRMan.IsBusAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.CargoTruck, VRMan.IsCargoTruckAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.Taxi, VRMan.IsTaxiAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.Service, VRMan.IsServiceAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.CargoTrain, VRMan.IsCargoTrainAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.PassengerTrain, VRMan.IsPassengerTrainAllowed(mask));
            }
            if(SLMan != null)
                SpeedLimit = (SLMan as SpeedLimitManager).GetGameSpeedLimit(LaneData.LaneID);
            else
                SpeedLimit = lane.LaneInfo.m_speedLimit;

            //TODO lane connections

            //Log.Debug("NetLaneExt.UpdateLane() result: " + this);
        }

        public override string ToString() {
            return $"NetLaneExt({LaneData} flags={m_flags} speed={SpeedLimit})";
        }
    }

    public struct NetNodeExt {
        public ushort NodeID;
        public Flags m_flags;

        public void Serialize(DataSerializer s) => s.WriteInt32((int)m_flags);
        public void Deserialize(DataSerializer s) => m_flags = (Flags)s.ReadInt32();
        public void Init(ushort nodeID) => NodeID = nodeID;

        [Flags]
        public enum Flags {
            None = 0,

            [Hint("when required, the node will not be rendered when Adaptive Roads mod is enabled.\n" +
                "when forbidden, the node will only be rendered when Adaptive Roads mod is enabled.")]
            Vanilla = 1 << 0,

            [Hint("all entering segment ends keep clear of the junction." +
                "useful for drawing pattern on the junction.")]
            KeepClearAll = 1 << 1,

            //All = -1,
        }

        public static IJunctionRestrictionsManager JRMan =>
            TrafficManager.Constants.ManagerFactory.JunctionRestrictionsManager;

        public void UpdateFlags() {
            if(JRMan != null) {
                bool keepClearAll = true;
                foreach(var segmentID in NetUtil.IterateNodeSegments(NodeID)) {
                    bool startNode = NetUtil.IsStartNode(segmentId: segmentID, nodeId: NodeID);
                    bool keppClear = JRMan.IsEnteringBlockedJunctionAllowed(segmentID, startNode);
                    keepClearAll &= keppClear;

                }
                m_flags = m_flags.SetFlags(Flags.KeepClearAll, keepClearAll);
            }
        }
    }

    public struct NetSegmentExt {
        public ushort SegmentID;
        public float SpeedLimit; // max
        public float Curve;
        public Flags m_flags;

        public void Serialize(DataSerializer s) => s.WriteInt32((int)m_flags);
        public void Deserialize(DataSerializer s) => m_flags = (Flags)s.ReadInt32();
        public void Init(ushort segmentID) => SegmentID = segmentID;


        [Flags]
        public enum Flags {
            None = 0,

            [Hint("if required, it will not be rendered when Adaptive Roads mod is enabled.\n" +
                "if forbidden, it will only be rendered when Adaptive Roads mod is enabled.")]
            Vanilla = 1 << 0,

            [Hint("tests if all lanes have the same speed")]
            UniformSpeedLimit = 1 << 1,

            ParkingAllowedRight = 1 << 5,
            ParkingAllowedLeft = 1 << 6,
            ParkingAllowedBoth = ParkingAllowedRight | ParkingAllowedLeft,

            [Hint("similar to lane inverted flag but for segment. tests if traffic drives on left (right hand drive).")]
            LeftHandTraffic = 1 << 7,
            //All = -1,
        }

        public ref NetSegmentEnd Start => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, true);
        public ref NetSegmentEnd End => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, false);

        public override string ToString() =>
            $"NetSegmentExt(SegmentID:{SegmentID} flags:{m_flags}"
            //+ $"AverageSpeedLimit:{AverageSpeedLimit} " +
            + $"\n\tStart:{Start})" + $"\n\tEnd  :{End}";

        public ref NetSegmentEnd GetEnd(ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(segmentId: SegmentID, nodeId: nodeID);
            if(startNode)
                return ref Start;
            else
                return ref End;
        }

        public void UpdateAllFlags() {
            if(!NetUtil.IsSegmentValid(SegmentID)) {
                //Log.Debug("Skip updating invalid segment:" + SegmentID);
                return;
            }
            Log.Debug($"NetSegmentExt.UpdateAllFlags() called. SegmentID={SegmentID}" /*Environment.StackTrace*/, false);

            bool parkingLeft = false;
            bool parkingRight = false;
            float speed0 = -1;
            float maxSpeedLimit = 0;
            float speedLimitAcc = 0;
            int speedLaneCount = 0;

            bool uniformSpeed = true;
            foreach(LaneData lane in NetUtil.IterateSegmentLanes(SegmentID)) {
                ref NetLaneExt laneExt = ref NetworkExtensionManager.Instance.LaneBuffer[lane.LaneID];
                laneExt.UpdateLane(lane);
                if(laneExt.m_flags.IsFlagSet(NetLaneExt.Flags.ParkingAllowed)) {
                    if(lane.LeftSide)
                        parkingLeft = true;
                    else
                        parkingRight = true;
                }
                if(lane.LaneInfo.m_laneType.IsFlagSet(SpeedLimitManager.LANE_TYPES) &&
                    lane.LaneInfo.m_vehicleType.IsFlagSet(SpeedLimitManager.VEHICLE_TYPES)) {
                    if(speed0 == -1)
                        speed0 = laneExt.SpeedLimit;
                    else
                        uniformSpeed &= laneExt.SpeedLimit == speed0;
                    speedLimitAcc += laneExt.SpeedLimit;
                    speedLaneCount++;
                    maxSpeedLimit = Mathf.Max(maxSpeedLimit, laneExt.SpeedLimit);
                }
            }

            m_flags = m_flags.SetFlags(Flags.ParkingAllowedLeft, parkingLeft);
            m_flags = m_flags.SetFlags(Flags.ParkingAllowedRight, parkingRight);
            m_flags = m_flags.SetFlags(Flags.UniformSpeedLimit, uniformSpeed);
            m_flags = m_flags.SetFlags(Flags.LeftHandTraffic, NetUtil.LHT);

            float averageSpeedLimit = speedLimitAcc / speedLaneCount;
            SpeedLimit = maxSpeedLimit;

            Curve = CalculateCurve();

            Start.UpdateFlags();
            Start.UpdateDirections();

            End.UpdateFlags();
            End.UpdateDirections();


            Log.Debug($"NetSegmentExt.UpdateAllFlags() succeeded for {this}" /*Environment.StackTrace*/, false);

        }

        /// <summary>
        /// Calculates Raduis of a curved segment assuming it is part of a circle.
        /// </summary>
        public float CalculateRadius() {
            // TDOO: to calculate maximum curviture for eleptical road, cut the bezier in 10 portions
            // and then find the bezier with minimum raduis.
            ref NetSegment segment = ref SegmentID.ToSegment();
            Vector2 startDir = VectorUtils.XZ(segment.m_startDirection);
            Vector2 endDir = VectorUtils.XZ(segment.m_endDirection);
            Vector2 startPos = VectorUtils.XZ(segment.m_startNode.ToNode().m_position);
            Vector2 endPos = VectorUtils.XZ(segment.m_endNode.ToNode().m_position);
            float dot = Vector2.Dot(startDir, -endDir);
            float len = (startPos - endPos).magnitude;
            return len / Mathf.Sqrt(2 - 2 * dot); // see https://github.com/CitiesSkylinesMods/TMPE/issues/793#issuecomment-616351792
        }

        public float CalculateCurve() {
            // see NetLane.UpdateLength()
            var bezier = SegmentID.ToSegment().CalculateSegmentBezier3();
            Vector3 d1 = bezier.b - bezier.a;
            Vector3 d2 = bezier.c - bezier.b;
            Vector3 d3 = bezier.d - bezier.c;
            float m1 = d1.magnitude;
            float m2 = d2.magnitude;
            float m3 = d3.magnitude;
            if(m1 > 0.1f) d1 /= m1;
            if(m3 > 0.1f) d3 /= m3;

            var length = m1 + m2 + m3;
            var curve = (Mathf.PI * 0.5f) * (1f - Vector3.Dot(d1, d3));
            if(length > 0.1f) curve /= length;

            return curve;
        }

    }

    public struct NetSegmentEnd {
        [Flags]
        public enum Flags {
            None = 0,

            [Hint("when required, the node will not be rendered when Adaptive Roads mod is enabled.\n" +
                "when forbidden, the node will only be rendered when Adaptive Roads mod is enabled.")]
            Vanilla = 1 << 0,            // priority signs
            [Hint("checks if TMPE rules requires vehicles to yield to upcomming traffic\n" +
                "differet than the vanilla YieldStart/YieldEnd (Stop) flag.")]
            Yield = 1 << 4,

            [Hint("checks if TMPE rules requires vehicles to Stop at junctions\n" +
                "differet than the vanilla YieldStart/YieldEnd (Stop) flag.")]
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

            [Hint("in a oneway road vehicles can take far turn even when traffic light is red\n" +
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

            [Hint("the junction only has two segments.\n")]
            TwoSegments = 1 << 21,

            [Hint("the junction has segments with different speed limits.\n")]
            SpeeedChange = 1 << 22,

            ALL = -1,
        }

        public Flags m_flags;
        public ushort SegmentID;
        public bool StartNode;

        public ushort NodeID => SegmentID.ToSegment().GetNode(StartNode);

        public void Serialize(DataSerializer s) => s.WriteInt32((int)m_flags);
        public void Deserialize(DataSerializer s) => m_flags = (Flags)s.ReadInt32();
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



            flags = flags.SetFlags(Flags.TwoSegments, NodeID.ToNode().CountSegments() == 2);
            //flags = flags.SetFlags(Flags.SpeeedChange,  );

            //NodeID.ToNode().

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

    }
}

