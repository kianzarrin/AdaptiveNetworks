/*
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

namespace AdvancedRoads {
    using ColossalFramework;
    using CSUtil.Commons;
    using KianCommons;
    using System;
    using System.ComponentModel;
    using TrafficManager;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;

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
        public enum Flags : UInt32 {
            None,

            ParkingAllowed,

            // Vehicle restrictions
            PassengerCar,
            SOS,
            Taxi,
            Bus,
            CargoTruck,

            CargoTrain,
            PassengerTrain,

            // misc
            MergesWithInnerLane,
            MergesWithOuterLane,

            All = UInt32.MaxValue,
        }

        public Flags m_flags;
        public void SetBit(Flags bit, bool set) {
            if (set)
                m_flags |= bit;
            else
                m_flags &= ~bit;
        }

        public float SpeedLimitKPH;
        public float SpeedLimitMPH;

        public object OuterMarking;
        public object InnerMarking;

        public LaneData LaneData;

        public void Init(uint laneID, int laneIndex) {
            LaneData.LaneID = laneID;
            LaneData.LaneIndex = laneIndex;
            LaneData.LaneInfo = null;
            m_flags = Flags.None;
        }

        static ParkingRestrictionsManager PMan => ParkingRestrictionsManager.Instance;
        static VehicleRestrictionsManager VRMan => VehicleRestrictionsManager.Instance;
        public void UpdateLane() {
            LaneData.LaneInfo = LaneData.Segment.Info.m_lanes[LaneData.LaneIndex];

            SetBit(Flags.ParkingAllowed, PMan.IsParkingAllowed(LaneData.SegmentID, LaneData.LaneInfo.m_finalDirection));

            var mask = VRMan.GetDefaultAllowedVehicleTypes(
                segmentId:LaneData.SegmentID,
                segmentInfo: LaneData.Segment.Info,
                laneIndex:(uint)LaneData.LaneIndex,
                laneInfo:LaneData.LaneInfo,
                busLaneMode: VehicleRestrictionsMode.Configured);


            SetBit(Flags.PassengerCar, VRMan.IsPassengerCarAllowed(mask));
            SetBit(Flags.SOS, VRMan.IsEmergencyAllowed(mask));
            SetBit(Flags.Bus, VRMan.IsBusAllowed(mask));
            SetBit(Flags.CargoTruck, VRMan.IsCargoTruckAllowed(mask));
            SetBit(Flags.Taxi, VRMan.IsTaxiAllowed(mask));
            SetBit(Flags.CargoTrain, VRMan.IsCargoTrainAllowed(mask));
            SetBit(Flags.PassengerTrain, VRMan.IsPassengerTrainAllowed(mask));

            //TODO lane connections // speed limits.

        }

    }

    [Serializable]
    public struct NetNodeExt {
        public ushort NodeID;

        [Flags]
        public enum Flags : UInt32 {
            None,
            Vanilla,
            KeepClearAll, // all entering segment ends keep clear of the junction.
            All = UInt32.MaxValue,
        }

        public Flags m_flags;
    }

    [Serializable]
    public struct NetSegmentExt {
        public ushort SegmentID;

        [Flags]
        public enum Flags : UInt32 {
            None,
            Vanilla,
            UniformSpeedLimit,
            SpeedLimitMPH,
            SpeedLimitKPH,
            All = UInt32.MaxValue,
        }

        public float AverageSpeedLimitMPH;
        public float AverageSpeedLimitKPH;

        public Flags m_flags;

        public ref NetSegmentEnd Start => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, true);
        public ref NetSegmentEnd End => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, false);

        public ref NetSegmentEnd GetEnd(ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(segmentId: SegmentID, nodeId: nodeID);
            if (startNode)
                return ref Start;
            else
                return ref End;
        }
    }

    [Serializable]
    public struct NetSegmentEnd {
        [Flags]
        public enum Flags : UInt32 {
            None = 0,

            // priority signs
            Yield = 1 << 0,
            Stop = 1 << 1,
            Main = 1 << 2,

            // junction restrictions.
            ZebraCrossing = 1 << 3,
            KeepClear = 1 << 4,
            NearTurnAtRed = 1 << 5,
            FartTurnAtRed = 1 << 6,
            LaneChangingGoingStraight = 1 << 7,
            Uturn = 1 << 8,

            // directions
            HasRightSegment = 1 << 9,
            CanTurnRight = 1 << 10,
            HasLeftSegment = 1 << 11,
            CanTurnLeft = 1 << 12,
            HasForwardSegment = 1 << 13,
            CanGoForward = 1 << 14,

            All = UInt32.MaxValue,
        }

        public Flags m_flags;

        public void SetBit(Flags bit, bool set) {
            if (set)
                m_flags |= bit;
            else
                m_flags &= ~bit;
        }

        public ushort SegmentID, NodeID;
        public bool StartNode;

        public static JunctionRestrictionsManager JRMan => JunctionRestrictionsManager.Instance;
        public static TrafficPriorityManager PMan => TrafficPriorityManager.Instance;

        public void Init(ushort segmentID, bool startNode) {
            m_flags = Flags.None;
            SegmentID = segmentID;
            StartNode = startNode;
            NodeID = startNode ? segmentID.ToSegment().m_startNode : segmentID.ToSegment().m_endNode;
        }

        public void UpdateFlags() {
            PriorityType p = PMan.GetPrioritySign(SegmentID, StartNode);
            SetBit(Flags.Yield, p == PriorityType.Yield);
            SetBit(Flags.Stop, p == PriorityType.Stop);
            SetBit(Flags.Main, p == PriorityType.Main);

            SetBit(Flags.KeepClear, !JRMan.IsEnteringBlockedJunctionAllowed(SegmentID, StartNode));
            SetBit(Flags.ZebraCrossing, JRMan.IsPedestrianCrossingAllowed(SegmentID, StartNode));
            SetBit(Flags.NearTurnAtRed, JRMan.IsNearTurnOnRedAllowed(SegmentID, StartNode));
            SetBit(Flags.FartTurnAtRed, JRMan.IsFarTurnOnRedAllowed(SegmentID, StartNode));
            SetBit(Flags.Uturn, JRMan.IsUturnAllowed(SegmentID, StartNode));
            SetBit(Flags.LaneChangingGoingStraight, JRMan.IsLaneChangingAllowedWhenGoingStraight(SegmentID, StartNode));
        }

        public void UpdateDirections() { 
            CheckSegmentsInEachDirection(
                segmentId: SegmentID, nodeId: NodeID,
                right: out bool right, forward: out bool forward, left: out bool left);
            SetBit(Flags.HasRightSegment, right);
            SetBit(Flags.HasLeftSegment, left);
            SetBit(Flags.HasForwardSegment, forward);

            LaneArrows arrows = AllArrows(SegmentID, StartNode);
            SetBit(Flags.CanGoForward, arrows.IsFlagSet(LaneArrows.Forward));
            SetBit(Flags.CanTurnRight, arrows.IsFlagSet(LaneArrows.Right));
            SetBit(Flags.CanTurnLeft, arrows.IsFlagSet(LaneArrows.Left));
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
                        throw new Exception("Unreachable Code");
                } //end switch
            } // end for
        } // end method

    }
}

