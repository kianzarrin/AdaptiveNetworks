/*
 * inverted = LHT
Stop sign:
Inverted\Dir | Forward | Backward
----------------------------------
   False     |         |  
       Yield | End     | End
angle/offset | 0/+1    | 0/+1
----------------------------------   
   True      |         |
       Yield | Start   | Start
angle/offset | 180/-1  | 180/-1

 BusStop  (inverted: dont care)
Forward:   angle:90   
backward:  angle:-90
 */

/*
segment 0 (Normal):
    forward required =
    forward forbidden = StopBoth
    backward required =
    backward forbidden = StopBoth
segment 1 (BusStopSide):
    forward required = StopLeft
    forward forbidden = StopRight
    backward required = StopRight
    backward forbidden = StopLeft
segment 2 (BusStopBoth):
    forward required = StopBoth
    forward forbidden =
    backward required = StopBoth
    backward forbidden =
*/

/*
segment 0 (no parking):
    forward required =
    forward forbidden = ParkingAllowedBoth | vanilla
    backward required =
    backward forbidden = ParkingAllowedBoth | vanilla
segment 1 (1 Side parking):
    forward required = ParkingAllowedLeft 
    forward forbidden = ParkingAllowedRight | vanilla
    backward required = ParkingAllowedRight 
    backward forbidden = ParkingAllowedLeft | vanilla
segment 2 (normal):
    forward required = ParkingAllowedBoth
    forward forbidden =
    backward required = ParkingAllowedBoth
    backward forbidden =
*/


/*
    NetNode.Flags {
        CustomTrafficLights = int.MinValue,
        All = -1,
        None = 0,
        Created = 1,
        Deleted = 2,
        Original = 4, // new game from map: no need to pay
        Disabled = 8, // if netInfo.m_canDisable and is part of a disabled building 
        End = 16,
        Middle = 32,
        Bend = 64,
        Junction = 128,
        Moveable = 256,
        Untouchable = 512,
        Outside = 1024,
        Temporary = 2048,
        Double = 4096,
        Fixed = 8192,
        OnGround = 16384,
        Ambiguous = 32768,
        Water = 65536,
        Sewage = 131072,
        ForbidLaneConnection = 262144,
        Underground = 524288,
        Transition = 1048576, // urban junction: has at leas one segment that does not have NetInfo.m_HighwayRules (useful for highway sign)
        UndergroundTransition = 1572864,
        LevelCrossing = 2097152, // train track connection.
        OneWayOut = 4194304,
        TrafficLights = 8388608,
        OneWayOutTrafficLights = 12582912,
        OneWayIn = 16777216,
        Heating = 33554432,
        Electricity = 67108864, // powerLineAI
        Collapsed = 134217728, // due to disaster
        DisableOnlyMiddle = 268435456, // ShipAI
        AsymForward = 536870912,
        AsymBackward = 1073741824
    }

    NetSegment.Flags {
        All = -1,
        None = 0,
        Created = 1,
        Deleted = 2,
        Original = 4,
        Collapsed = 8,
        Invert = 16,
        Untouchable = 32,
        End = 64,
        Bend = 128,
        WaitingPath = 256,
        CombustionBan = 256,
        PathFailed = 512,
        PathLength = 1024,
        AccessFailed = 2048,
        TrafficStart = 4096,
        TrafficEnd = 8192,
        CrossingStart = 16384,
        CrossingEnd = 32768,

        // bus
        StopRight = 1<<16,
        StopLeft = 1<<17,
        StopBoth = StopRight | StopLeft,

        // tram
        StopRight2 = 1<<18,
        StopLeft2 = 1<<19,
        StopBoth2 = StopRight2  | StopLeft2 ,

        StopAll = StopBoth | StopBoth2,

        HeavyBan = 1048576,
        Blocked = 2097152,
        Flooded = 4194304,
        BikeBan = 8388608,
        CarBan = 16777216,
        AsymForward = 33554432,
        AsymBackward = 67108864,
        CustomName = 134217728,
        NameVisible1 = 268435456,
        NameVisible2 = 536870912,
        YieldStart = 1073741824, //start ndoe 
        YieldEnd = int.MinValue, // end node
    }

    [Flags]
    public enum NetLane.Flags
    {
        None = 0,
        Created = 1,
        Deleted = 2,
        Inverted = 4, //Left Hand Traffic
        JoinedJunction = 8, two nodes are too close
        JoinedJunctionInverted = 12, 
        Forward = 16,
        Left = 32,
        LeftForward = 48,
        Right = 64,
        ForwardRight = 80,
        LeftRight = 96,
        LeftForwardRight = 112,
        Merge = 128, // multiple lanes merge into one in an intersection. never used.
        Stop = 256, // bus
        Stop2 = 512, //tram
        Stops = 768, // bus+tram
        YieldStart = 1024, // yeild at tail(LHT=head)
        YieldEnd = 2048, // yeald at head(LHT=tail)

        
        StartOneWayLeft = 4096,  // RHT: | LHT:
        StartOneWayRight = 8192, // RHT: | LHT:
        EndOneWayLeft = 16384,   // RHT: | LHT:
        EndOneWayRight = 32768,  // RHT: | LHT:

        StartOneWayLeftInverted = 4100, 
        StartOneWayRightInverted = 8196,
        EndOneWayLeftInverted = 16388,
        EndOneWayRightInverted = 32772
    }
 */

namespace AdaptiveRoads.Manager {
    using ColossalFramework;
    using CSUtil.Commons;
    using System;
    using TrafficManager;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using KianCommons;
    using Log = KianCommons.Log;
    using UnityEngine;
    using ColossalFramework.Math;

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

    [Serializable]
    public struct NetLaneExt {
        [Flags]
        public enum Flags {
            None = 0,

            ParkingAllowed = 1 << 0,

            // Vehicle restrictions
            PassengerCar = 1 << 1,
            SOS = 1 << 2,
            Taxi = 1 << 3,
            Bus = 1 << 4,
            CargoTruck = 1 << 5,

            CargoTrain = 1 << 6,
            PassengerTrain = 1 << 7,

            // misc
            //MergesWithInnerLane = 1 << 8,
            //MergesWithOuterLane = 1 << 9,

            All = (1 << 10) -1,
        }

        public Flags m_flags;

        public float SpeedLimit; // game speed limit 1=50kph 20=unlimitted

        //public object OuterMarking;
        //public object InnerMarking;

        public LaneData LaneData;

        public void Init(uint laneID) {
            LaneData.LaneID = laneID;
            LaneData.LaneIndex = 0;
            LaneData.LaneInfo = null;
            m_flags = Flags.None;
        }

        static ParkingRestrictionsManager PMan => ParkingRestrictionsManager.Instance;
        static VehicleRestrictionsManager VRMan => VehicleRestrictionsManager.Instance;
        static SpeedLimitManager SLMan => SpeedLimitManager.Instance;

        public void UpdateLane(LaneData lane) {
            Assertion.AssertEqual(LaneData.LaneID, lane.LaneID, "lane id");
            LaneData = lane;

            m_flags = m_flags.SetFlags(
                Flags.ParkingAllowed,
                LaneData.LaneInfo.m_laneType == NetInfo.LaneType.Parking &&
                PMan.IsParkingAllowed(LaneData.SegmentID, LaneData.LaneInfo.m_finalDirection));

            var mask = VRMan.GetAllowedVehicleTypes(
                segmentId:LaneData.SegmentID,
                segmentInfo: LaneData.Segment.Info,
                laneIndex:(uint)LaneData.LaneIndex,
                laneInfo:LaneData.LaneInfo,
                busLaneMode: VehicleRestrictionsMode.Configured);

            m_flags = m_flags.SetFlags(Flags.PassengerCar, VRMan.IsPassengerCarAllowed(mask));
            m_flags = m_flags.SetFlags(Flags.SOS, VRMan.IsEmergencyAllowed(mask));
            m_flags = m_flags.SetFlags(Flags.Bus, VRMan.IsBusAllowed(mask));
            m_flags = m_flags.SetFlags(Flags.CargoTruck, VRMan.IsCargoTruckAllowed(mask));
            m_flags = m_flags.SetFlags(Flags.Taxi, VRMan.IsTaxiAllowed(mask));
            m_flags = m_flags.SetFlags(Flags.CargoTrain, VRMan.IsCargoTrainAllowed(mask));
            m_flags = m_flags.SetFlags(Flags.PassengerTrain, VRMan.IsPassengerTrainAllowed(mask));

            SpeedLimit = SLMan.GetGameSpeedLimit(LaneData.LaneID);

            //TODO lane connections

            //Log.Debug("NetLaneExt.UpdateLane() result: " + this);
        }

        public override string ToString() {
            return $"NetLaneExt({LaneData} flags={m_flags} speed={SpeedLimit})";
        }
    }

    [Serializable]
    public struct NetNodeExt {
        public ushort NodeID;
        public void Init(ushort nodeID) => NodeID = nodeID;

        [Flags]
        public enum Flags {
            None = 0,
            Vanilla = 1 << 0,
            KeepClearAll = 1 << 1, // all entering segment ends keep clear of the junction.
            TwoSegments = 1 << 2,

            //All = (1 << 3) - 1,
        }

        public static JunctionRestrictionsManager JRMan => JunctionRestrictionsManager.Instance;

        public void UpdateFlags() {
            bool keepClearAll = true;
            foreach (var segmentID in NetUtil.IterateNodeSegments(NodeID)) {
                bool startNode = NetUtil.IsStartNode(segmentId: segmentID, nodeId: NodeID);
                bool keppClear = JRMan.IsEnteringBlockedJunctionAllowed(segmentID, startNode);
                keepClearAll &= keppClear;

            }
            m_flags = m_flags.SetFlags(Flags.KeepClearAll, keepClearAll);
            m_flags = m_flags.SetFlags(Flags.TwoSegments, NodeID.ToNode().CountSegments() == 2);
        }

        public Flags m_flags;
    }

    [Serializable]
    public struct NetSegmentExt {
        public ushort SegmentID;
        public void Init(ushort segmentID) => SegmentID = segmentID;

        [Flags]
        public enum Flags {
            None = 0,
            Vanilla = 1 << 0,

            UniformSpeedLimit = 1 << 1,
            //SpeedLimitMPH = 1 << 2,
            //SpeedLimitKPH = 1 << 3,

            ParkingAllowedRight = 1 << 5,
            ParkingAllowedLeft = 1 << 6,
            ParkingAllowedBoth = ParkingAllowedRight | ParkingAllowedLeft,

            LeftHandTraffic = 1 << 7,
            //All = Vanilla,
        }

        public float AverageSpeedLimit;
        public float Curve;

        public Flags m_flags;

        public ref NetSegmentEnd Start => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, true);
        public ref NetSegmentEnd End => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, false);

        public override string ToString() =>
            $"NetSegmentExt(SegmentID:{SegmentID} flags:{m_flags} AverageSpeedLimit={AverageSpeedLimit})";

        public ref NetSegmentEnd GetEnd(ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(segmentId: SegmentID, nodeId: nodeID);
            if (startNode)
                return ref Start;
            else
                return ref End;
        }

        public void UpdateFlags() {
            m_flags = Flags.None; // not vanila
        }


        public void UpdateAllFlags() {
            UpdateFlags();

            Start.UpdateFlags();
            Start.UpdateDirections();

            End.UpdateFlags();
            End.UpdateDirections();

            bool parkingLeft = false;
            bool parkingRight = false;
            float speed0 = -1;
            float speedLimitAcc = 0;
            int speedLaneCount = 0; 

            bool uniformSpeed = true;
            foreach (LaneData lane in NetUtil.IterateSegmentLanes(SegmentID)) {
                ref NetLaneExt laneExt = ref NetworkExtensionManager.Instance.LaneBuffer[lane.LaneID];
                laneExt.UpdateLane(lane);
                if (laneExt.m_flags.IsFlagSet(NetLaneExt.Flags.ParkingAllowed)) {
                    if (lane.LeftSide)
                        parkingLeft = true;
                    else
                        parkingRight = true;
                }
                if (lane.LaneInfo.m_laneType.IsFlagSet(SpeedLimitManager.LANE_TYPES) &&
                    lane.LaneInfo.m_vehicleType.IsFlagSet(SpeedLimitManager.VEHICLE_TYPES)) {
                    if (speed0 == -1)
                        speed0 = laneExt.SpeedLimit;
                    else
                        uniformSpeed &= laneExt.SpeedLimit == speed0;
                    speedLimitAcc += laneExt.SpeedLimit;
                    speedLaneCount++;
                }
            }

            m_flags = m_flags.SetFlags(Flags.ParkingAllowedLeft, parkingLeft);
            m_flags = m_flags.SetFlags(Flags.ParkingAllowedRight, parkingRight);
            m_flags = m_flags.SetFlags(Flags.UniformSpeedLimit, uniformSpeed);
            m_flags = m_flags.SetFlags(Flags.LeftHandTraffic, NetUtil.LHT);

            AverageSpeedLimit = speedLimitAcc / speedLaneCount;

            Curve = CalculateCurve();

            Log.Debug($"NetSegmentExt.UpdateAllFlags() succeeded for {this}" /*Environment.StackTrace*/,false);

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
            if (m1 > 0.1f) d1 /= m1;
            if (m3 > 0.1f) d3 /= m3;
            
            var length = m1 + m2 + m3;
            var curve = (Mathf.PI * 0.5f) * (1f - Vector3.Dot(d1, d3));
            if (length > 0.1f) curve /= length;
            
            return curve;
        }

    }

    [Serializable]
    public struct NetSegmentEnd {
        [Flags]
        public enum Flags {
            None = 0,

            // priority signs
            Yield = 1 << 0,
            Stop = 1 << 1,
            PriorityMain = 1 << 2,

            // junction restrictions.
            ZebraCrossing = 1 << 3,
            KeepClear = 1 << 4,
            NearTurnAtRed = 1 << 5,
            FarTurnAtRed = 1 << 6,
            LaneChangingGoingStraight = 1 << 7,
            Uturn = 1 << 8,

            // directions
            HasRightSegment = 1 << 9,
            CanTurnRight = 1 << 10,
            HasLeftSegment = 1 << 11,
            CanTurnLeft = 1 << 12,
            HasForwardSegment = 1 << 13,
            CanGoForward = 1 << 14,

            IsStartNode = 1 << 15,
            IsTailNode = 1 << 16,

            //All = (1 << 15)-1,
        }

        public Flags m_flags;

        public ushort SegmentID;
        public bool StartNode;
        public ushort NodeID => SegmentID.ToSegment().GetNode(StartNode);

        public static JunctionRestrictionsManager JRMan => JunctionRestrictionsManager.Instance;
        public static TrafficPriorityManager PMan => TrafficPriorityManager.Instance;

        public void Init(ushort segmentID, bool startNode) {
            m_flags = Flags.None;
            SegmentID = segmentID;
            StartNode = startNode;
        }

        public void UpdateFlags() {
            var flags = m_flags; // TODO is this necessary?

            PriorityType p = PMan.GetPrioritySign(SegmentID, StartNode);
            flags = flags.SetFlags(Flags.Yield, p == PriorityType.Yield);
            flags = flags.SetFlags(Flags.Stop, p == PriorityType.Stop);
            flags = flags.SetFlags(Flags.PriorityMain, p == PriorityType.Main);

            flags = flags.SetFlags(Flags.KeepClear, !JRMan.IsEnteringBlockedJunctionAllowed(SegmentID, StartNode));
            flags = flags.SetFlags(Flags.ZebraCrossing, JRMan.IsPedestrianCrossingAllowed(SegmentID, StartNode));
            flags = flags.SetFlags(Flags.NearTurnAtRed, JRMan.IsNearTurnOnRedAllowed(SegmentID, StartNode));
            flags = flags.SetFlags(Flags.FarTurnAtRed, JRMan.IsFarTurnOnRedAllowed(SegmentID, StartNode));
            flags = flags.SetFlags(Flags.Uturn, JRMan.IsUturnAllowed(SegmentID, StartNode));
            flags = flags.SetFlags(Flags.LaneChangingGoingStraight, JRMan.IsLaneChangingAllowedWhenGoingStraight(SegmentID, StartNode));
            flags = flags.SetFlags(Flags.LaneChangingGoingStraight, JRMan.IsLaneChangingAllowedWhenGoingStraight(SegmentID, StartNode));

            flags = flags.SetFlags(Flags.IsStartNode, StartNode);
            flags = flags.SetFlags(Flags.IsTailNode, NetUtil.GetTailNode(SegmentID) == NodeID);

            m_flags = flags;
            
            //Log.Debug("NetSegmentEnd.UpdateFlags() result: " + this);
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
            foreach (var lane in NetUtil.IterateLanes(
                segmentId:segmentId,startNode: startNode,
                laneType:  LaneArrowManager.LANE_TYPES, vehicleType: LaneArrowManager.VEHICLE_TYPES)) {
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

            for (int i = 0; i < 8; ++i) {
                ushort otherSegmentId = nodeId.ToNode().GetSegment(i);
                if (otherSegmentId == 0) continue;
                bool isRoad = otherSegmentId.ToSegment().Info.m_netAI is RoadBaseAI;
                if (!isRoad) continue;
                ArrowDirection dir = segEndMan.GetDirection(ref segEnd, otherSegmentId);
                switch (dir) {
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

