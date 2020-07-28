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
    using KianCommons;
    using System;

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
    public class NetLaneExt {
        [Flags]
        public enum Flags {
            None,

            ParkingAllowed,

            // Vehicle restrictions
            Cars,
            SOS,
            Taxi,
            Bus,
            CargoTruck,

            CargoTrain,
            PassengerTrain,

            // speed limits
            SpeedLimitMPH,
            SpeedLimitKPH,

            // misc
            MergesWithInnerLane,
            MergesWithOuterLane,
        }
        public object OuterMarking;
        public object InnerMarking;

        public class PropExt {
            public float SpeedLimitMPH;
            public float SpeedLimitKPH;
        }

    }
    [Serializable]
    public class NetNodeExt {
        public ushort NodeID;

        [Flags]
        public enum Flags {
            None,
            Vanilla,
            KeepClearAll, // all entering segment ends keep clear of the junction.
        }

        public Flags m_flags;
    }

    [Serializable]
    public class NetSegmentExt {
        public ushort SegmentID;

        [Flags]
        public enum Flags {
            None,
            Vanilla,
            UniformSpeedLimit,
            SpeedLimitMPH,
            SpeedLimitKPH,
        }

        float AverageSpeedLimitMPH;
        float AverageSpeedLimitKPH;

        public Flags m_flags;

        public NetSegmentEnd Start, End;

        public NetSegmentEnd GetEnd(ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(segmentId: SegmentID, nodeId: nodeID);
            return startNode ? Start : End;
        }
    }

    [Serializable]
    public class NetSegmentEnd {
        [Flags]
        public enum Flags {
            None = 0,

            // priority signs
            Yield = 1,
            Stop = 2,
            Priority = 4,

            // junction restrictions.
            ZebraCrossing = 8,
            KeepClear = 16,
            TurnRightAtRed = 32,
            TurnLeftAtRed = 64,
            SwitchLanesGoingStraight = 128,
            Uturn = 256,

            // directions
            HasRightSegment,
            CanTurnRught,
            HasLeftSegment,
            CanTurnLeft,
            HasForwardSegment,
            CanGoForward,
        }

        public Flags m_flags;
    }
}

