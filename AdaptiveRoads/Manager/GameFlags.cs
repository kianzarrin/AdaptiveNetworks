namespace AdaptiveRoads.Manager {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using KianCommons;
    using static HintExtension;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class HideAttribute :Attribute {
        [Flags]
        public enum HideMode {
            None=0,
            Read=1,
            Write=2,
            Both = Read | Write,
        }
        public HideMode Mode = HideMode.Both;
        public HideAttribute() { }
        public HideAttribute(HideMode mode) => Mode = mode;
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class HintAttribute : Attribute {
        public string Text;
        public HintAttribute(string text) => Text = text;
    }

    public static class HintExtension {
        public static List<string> GetHints(this MemberInfo info) {
            return info
                .GetCustomAttributes(typeof(HintAttribute), true)
                .Select(_item => (_item as HintAttribute).Text)
                .ToList();
        }
        public const string LANE_HEAD_TAIL =
            "cars drive from tail to head.\n"+
            "head/tail swap when:\n" +
            "    - segment is inverted\n" +
            "    - lane is backward" +
            "    - lane is uni-directional (that excludes pavements/medians)\n" +
            "      and traffic drives on left (LHT)\n";

        const string YIELD_HEAD = nameof(NetLaneFlags.YieldHead); //former end
        const string YIELD_TAIL = nameof(NetLaneFlags.YieldTail); // former start
        public const string LANE_YIELD_HEAD_TAIL =
            YIELD_HEAD + " and " + YIELD_TAIL + "swap when segment is inverted\n" +
            "adidtionally if lane.final_direction is forward, " + YIELD_HEAD + " is removed\n" +
            "and if lane.final_direction is backward, " + YIELD_TAIL + " is removed\n" +
            "pavement/median lanes arn't uni-directional so no yield flags is removed on them.\n" +
            "lane.final_direction is like lane.direction but considers LHT(Left Hand Traffic)\n" +
            "this is different than TMPE's yield/stop flags";

        public const string SEGMNET_START_END =
            "actual start/end nodes. this is not influenced by segment.invert or LHT";
    }

    [Flags]
    [Hint("Vanilla Segment Flags")]
    public enum NetSegmentFlags {
        None = 0,
        //[Hide]
        Created = 1,
        [Hide]
        Deleted = 2,
        [Hide]
        Original = 4,
        [Hint("Segment has been destroyed due to a disaster")]
        Collapsed = 8,
        [Hint(
                "Active for every other segment of a continuously drawn network\n" +
                "Can be used to prevent the segment mesh from flipping every other segment"
        )]
        Invert = 16,
        [Hint(
                "Active for nodes of networks which are placed within buildings,\n" +
                "and therefore can't be deleted or upgraded under normal circumstances."
        )]
        Untouchable = 32,
        [Hint("Segment has a dead end")]
        End = 64,
        [Hint("Active for nodes for sharp corners where a road changes direction suddenly and nodes where an asymmetric network changes direction")]
        Bend = 128,
        [Hide]
        WaitingPath = 256,
        [Hide]
        PathFailed = 512,
        [Hide]
        PathLength = 1024,
        [Hide]
        AccessFailed = 2048,
        [Hide]
        TrafficStart = 4096,
        [Hide]
        TrafficEnd = 8192,
        [Hide]
        CrossingStart = 16384,
        [Hide]
        CrossingEnd = 32768,
        [Hint("Has Bus stop on right side")]
        BusStopRight = 65536,
        [Hint("Has Bus stop on left side")]
        BusStopLeft = 131072,
        [Hint("Has Tram stop on right side")]
        TramStopRight = 262144,
        [Hint("Has Tram stop on left side")]
        TramStopLeft = 524288,
        [Hint(
                "Heavy traffic is not allowed here\n" +
                "Related to \"City Planning, Heavy Traffic Ban\" Policy"
        )]
        HeavyBan = 1048576,
        [Hint("There are too many vehicles on this segment")]
        Blocked = 2097152,
        [Hint("Segment has been flooded with water")]
        Flooded = 4194304,
        [Hint(
                "Cyclists are not allowed on the pavement here\n" +
                "Related to \"City Planning, Bike Ban On Sidewalks\" Policy"
        )]
        BikeBanOnSidewalk = 8388608,
        [Hint(
                "Only cars from local residents are allowed to drive here\n" +
                "Related to \"City Planning, Old Town\" Policy"
        )]
        CarBan = 16777216,
        [Hint(
                "Active for nodes where an asymmetric network changes direction from backward to forward\n" +
                "Related to the direction in which the player draws the road"
        )]
        AsymmetricalForward = 33554432,
        [Hint(
                "Active for nodes where an asymmetric network changes direction from forward to backward\n" +
                "Related to the direction in which the player draws the road")]
        AsymmetricalBackward = 67108864,
        [Hint("Street has a custom name")]
        CustomStreetName = 134217728,
        [Hide]
        NameVisible1 = 268435456,
        [Hide]
        NameVisible2 = 536870912,
        [Hint(
                "Active for segments which have a stop sign assigned by the player at the start of the segment\n" +
                "Relative to the direction in which the player draws the road"
        )]
        [Hide]
        YieldStart = 1073741824,
        [Hint(
                "Active for segments which have a stop sign assigned by the player at the end of the segment\n" +
                "Relative to the direction in which the player draws the road"
        )]
        [Hide]
        YieldEnd = -2147483648,
        [Hint("Has (Sightseeing) Bus stops on both sides")]
        BusStopBoth = BusStopLeft | BusStopRight,
        [Hint("Has Tram stops on both sides")]
        TramStopBoth = TramStopLeft | TramStopRight,
        [Hint("Has (Sightseeing) Bus stops on both sides as well as Tram stops on both sides")]
        StopAll = BusStopBoth | TramStopBoth,
        [Hint(
                "Cars with a combustion engine are not allowed to drive here\n" +
                "Related to \"City Planning, Combustion Engine Ban\" Policy"
        )]
        CombustionEngineBan = 256,
        All = -1
    }

    [Flags]
    public enum NetNodeFlags {
        None = 0,

        [Hide]
        Created = 1,
        [Hide]
        Deleted = 2,
        [Hide]
        Original = 4,
        [Hide]
        Disabled = 8,

        End = 16,

        Middle = 32,

        // HINT: this node is used where two compatible roads are connected at an angle
        // also when asymmeterical road changes direction (see also AsymForward and AsymBackward)
        // bend nodes use segment texture
        Bend = 64,
        Junction = 128,
        Moveable = 256,
        Untouchable = 512,
        Outside = 1024,
        Temporary = 2048,
        Double = 4096,

        // HINT: explanation
        Fixed = 8192,

        // HINT: 
        OnGround = 16384,

        Ambiguous = 32768,
        Water = 65536,
        Sewage = 131072,
        ForbidLaneConnection = 262144,
        Underground = 524288,
        Transition = 1048576,
        LevelCrossing = 2097152,
        OneWayOut = 4194304,
        TrafficLights = 8388608,
        OneWayIn = 16777216,
        Heating = 33554432,
        Electricity = 67108864,
        Collapsed = 134217728,
        DisableOnlyMiddle = 268435456,
        AsymForward = 536870912,
        AsymBackward = 1073741824,
        CustomTrafficLights = -2147483648,
        OneWayOutTrafficLights = 12582912,
        UndergroundTransition = 1572864,
        All = -1
    }


    [Flags]
    public enum NetLaneFlags {
        None = 0,

        Created = 1,

        Deleted = 2,

        // HINT: bla bla bla
        Inverted = 4,

        // show only: lane panel
        JoinedJunction = 8,

        // show only: prop panel
        JoinedJunctionInverted = 12,

        // HINT: forward lane arrow
        Forward = 16,
        Left = 32,
        Right = 64,
        Merge = 128,
        LeftForward = 48,
        LeftRight = 96,
        ForwardRight = 80,
        LeftForwardRight = 112,
        Stop = 256,
        Stop2 = 512,
        Stops = 768,
        [Hint(LANE_YIELD_HEAD_TAIL)]
        YieldTail = 1024,
        [Hint(LANE_YIELD_HEAD_TAIL)]
        YieldHead = 2048,
        StartOneWayLeft = 4096,
        StartOneWayRight = 8192,
        EndOneWayLeft = 16384,
        EndOneWayRight = 32768,
        StartOneWayLeftInverted = 4100,
        StartOneWayRightInverted = 8196,
        EndOneWayLeftInverted = 16388,
        EndOneWayRightInverted = 32772
    }


}
