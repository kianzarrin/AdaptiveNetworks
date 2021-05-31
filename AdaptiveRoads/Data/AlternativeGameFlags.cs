namespace AdaptiveRoads.Manager {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static HintExtension;
    using KianCommons;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class HideAttribute : Attribute { }

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
            "cars drive from tail to head.\n" +
            "head/tail swap when:\n" +
            "    - segment is inverted\n" +
            "    - lane is backward (or avoid forward)" +
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

        public const string COMPATIBLE_SEGMENT =
            "  * compatible segments have equal widths(pavement,\n" +
            "    asphalt, center area)and compatible vehicle types";

        public const string HEAD_TAIL_DIRECTION =
            "  * Left Hand traffic only swaps head/tail nodes on uni-directional lanes\n" +
            "    that excludes pavements/medians";

        public const string VANILLA = "if required, it will not be rendered when Adaptive Roads mod is enabled.\n" +
                "if forbidden, it will only be rendered when Adaptive Roads mod is enabled."


        public static Type GetMappedEnumWithHints(Type enumType) {
            Assertion.Assert(enumType.IsEnum, "enumType.IsEnum");
            if (enumType == typeof(NetSegment.Flags))
                return typeof(NetSegmentFlags);
            else if (enumType == typeof(NetLane.Flags))
                return typeof(NetLaneFlags);
            else if (enumType == typeof(NetNode.Flags))
                return typeof(NetNodeFlags);
            else if (enumType == typeof(NetInfo.Direction))
                return typeof(Direction);
            else if (enumType == typeof(VehicleInfo.VehicleType))
                return typeof(VehicleType);
            else
                return enumType;
        }

        public static string GetEnumMappedHint(Type enumType, string key) {
            object value = Enum.Parse(enumType, key);
            var enumType2 = GetMappedEnumWithHints(enumType);
            return enumType2.GetEnumMemberInfo(value).GetHints().JoinLines();
        }

        public static string GetHintSwichLHT_RHT(bool unidirectional) {
            if(unidirectional) {
                return "swaps: required.Inverted<->foribdden.inverted left<->right\n" +
                "negates: position.x";
            } else {
                return "swaps: required.Inverted<->foribdden.inverted start<->end left<->right\n" +
                "negates: position.z offset angle";
            }
        }
    }

    [Flags]
    public enum NetSegmentFlags {
        None = 0,
        //[Hide]
        Created = 1, // 1 >> 0
        [Hide]
        Deleted = 2, // 1 >> 1
        [Hide]
        [Hint("This segment has not been touched yet since the map was loeded.\n" +
              "Therefore there is no maintanace cost.")]
        Original = 4, // 1 >> 2
        [Hint("Segment has been destroyed due to a disaster")]
        Collapsed = 8, // 1 >> 3
        [Hint("Active for every other segment of a continuously drawn network\n" +
              "Can be used to prevent the segment mesh from flipping every other segment")]
        Invert = 16, // 1 >> 4
        [Hint("Active for nodes of networks which are placed within buildings,\n" +
                "and therefore can't be deleted or upgraded under normal circumstances.")]
        Untouchable = 32,
        [Hint("Segment has a dead end")]
        End = 64,
        [Hint("Active for nodes for sharp corners where a road changes direction suddenly\n" +
              " and nodes where an asymmetric network changes direction")]
        Bend = 128,

        [Hint( "Cars with a combustion engine are not allowed to drive here\n" +
               "Related to \"City Planning, Combustion Engine Ban\" Policy"
        )]
        CombustionEngineBan = 256, // 1 >> 
        //[Hide] WaitingPath = 256,  // two flags have same value. hide the useless one.
        [Hide]
        PathFailed = 512,
        [Hide]
        PathLength = 1024, // 1>>10
        [Hide]
        AccessFailed = 2048,// 1>>11
        [Hide]
        TrafficStart = 4096,// 1>>12
        [Hide]
        TrafficEnd = 8192, // 1 >> 13
        [Hide]
        CrossingStart = 16384, // 1 >> 14
        [Hide]
        CrossingEnd = 32768, // 1 >> 15
        [Hint("Has Bus stop on right side")]
        BusStopRight = 65536, // 1 >> 16
        [Hint("Has Bus stop on left side")]
        BusStopLeft = 131072, // 1 >> 17
        [Hint("Has Tram stop on right side")]
        TramStopRight = 262144, // 1 >> 18
        [Hint("Has Tram stop on left side")]
        TramStopLeft = 524288, // 1 >> 19
        [Hint("Has (Sightseeing) Bus stops on both sides")]
        BusStopBoth = BusStopLeft | BusStopRight,
        [Hint("Has Tram stops on both sides")]
        TramStopBoth = TramStopLeft | TramStopRight,
        [Hint("Has (Sightseeing) Bus stops on both sides as well as Tram stops on both sides")]
        StopAll = BusStopBoth | TramStopBoth,
        [Hint(
                "Heavy traffic is not allowed here\n" +
                "Related to \"City Planning, Heavy Traffic Ban\" Policy"
        )]
        HeavyBan = 1048576, // 1 >> 20
        [Hint("There are too many vehicles on this segment")]
        Blocked = 2097152, // 1 >> 21
        [Hint("Segment has been flooded with water")]
        Flooded = 4194304, // 1 >> 22
        [Hint(
                "Cyclists are not allowed on the pavement here\n" +
                "Related to \"City Planning, Bike Ban On Sidewalks\" Policy"
        )]
        BikeBanOnSidewalk = 8388608, // 1 >> 23
        [Hint(
                "Only cars from local residents are allowed to drive here\n" +
                "Related to \"City Planning, Old Town\" Policy"
        )]
        CarBan = 16777216, // 1 >> 24
        [Hint("Active for nodes where an asymmetric network changes direction from backward to forward\n" +
              "Relative to the direction in which the player draws the road")]
        AsymForward = 33554432, // 1 >> 25
        [Hint("Active for nodes where an asymmetric network changes direction from forward to backward\n" +
              "Relative to the direction in which the player draws the road")]
        AsymBackward = 67108864, // 1 >> 26
        [Hint("Street has a custom name")]
        CustomStreetName = 134217728, // 1 >> 27
        [Hide]
        NameVisible1 = 268435456, // 1 >> 28
        [Hide]
        NameVisible2 = 536870912, // 1 >> 29
        [Hint(
                "Active for segments which have a stop sign assigned by the player at the start of the segment\n" +
                "Relative to the direction in which the player draws the road"
        )]
        [Hide]
        YieldStart = 1073741824, // 1 >> 30
        [Hint(
                "Active for segments which have a stop sign assigned by the player at the end of the segment\n" +
                "Relative to the direction in which the player draws the road"
        )]
        [Hide]
        YieldEnd = -2147483648, // 1 >> 31

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
        [Hint("This node is part of a disabled building.")]
        Disabled = 8,

        [Hint("node has traffic lights (default or custom)")]
        TrafficLights = 8388608,

        [Hint("node has custom traffic lights")]
        CustomTrafficLights = -2147483648,

        [Hint("node with only one segment")]
        End = 16,

        [Hint("node between 2 compatible* segments that meet at 180 degree angle." +
              "Middle node is not rendered.\n" + COMPATIBLE_SEGMENT)]
        Middle = 32,

        [Hint("this node is used where two compatible* roads are connected at a bent angle\n" +
              "also when asymmeterical road changes direction(see also AsymForward and AsymBackward)\n" +
              "bend nodes use bend segment texture\n" + COMPATIBLE_SEGMENT)]
        Bend = 64,

        [Hint("intersections or transitions between 2 incompatible* networks.\n" + COMPATIBLE_SEGMENT)]
        Junction = 128,

        [Hint("nodes which are at the edge of the map and connected to the 'outside world'")]
        Outside = 1024,

        [Hint("`middle` node of a double length network.\n" +
               "eg: node where the big pillar for highway suspension bridges.")]
        Double = 4096,

        [Hint("nodes between networks which have different `levels`.\n" +
              "useful for highway entrance sigh")]
        Transition = 1048576,

        [Hint(" train/road intersections")]
        LevelCrossing = 2097152,

        [Hint("all segments but one are one-way toward the node")]
        OneWayOut = 4194304,

        [Hint("all segments but one are one-way going away from the node")]
        OneWayIn = 16777216,

        [Hint("Destroyed due to disaster")]
        Collapsed = 134217728,

        [Hint("main-side of the first segment is going toward the node.\n" +
              "This is relative to the direction in which the player draws the road")]
        AsymForward = 536870912,

        [Hint("main-side of the first segment is going away from the node.\n" +
              "This is relative to the direction in which the player draws the road")]
        AsymBackward = 1073741824,

        OneWayOutTrafficLights = 12582912,
        UndergroundTransition = 1572864,

        [Hint("nodes which will be automatically moved when \n" +
              "creating an intersection which splits a segment into two.")]
        [Hide]
        Moveable = 256,

        [Hint("nodes of networks which are placed within buildings,\n" +
              "and therefore can't be deleted or upgraded under normal circumstances")]
        [Hide]
        Untouchable = 512,

        [Hide]
        [Hint("these nodes are ignored when rendering/path-finding.")]
        Temporary = 2048,

        [Hide]
        [Hint("This node has not been touched yet since the map was loeded.\n" +
              "Therefore there is no maintanace cost.")]
        Original = 4,

        [Hint("node that is placed on the ground and is not elevated or underground.\n" +
              "such nodes can create dirt on ground.\n" +
              "also paths can connect to roads pavement at such ndoes")]
        OnGround = 16384,

        [Hint("node is underground.")]
        Underground = 524288,

        [Hide]
        Ambiguous = 32768,

        [Hide]
        Water = 65536,

        [Hide]
        Sewage = 131072,

        [Hide]
        Heating = 33554432,

        [Hide]
        Electricity = 67108864,

        [Hide]
        Fixed = 8192,

        [Hide]
        ForbidLaneConnection = 262144,

        [Hide]
        DisableOnlyMiddle = 268435456,

        All = -1
    }


    [Flags]
    public enum NetLaneFlags {
        None = 0,
        [Hide]
        Created = 1,

        [Hide]
        Deleted = 2,

        [Hint("Left hand traffic.\n" +
              "inverts lane final direction")]
        Inverted = 4,

        [Hint("when two junctions are very close to each other.")]
        JoinedJunction = 8,


        JoinedJunctionInverted = 12,

        [Hint("lane arrow")] Forward = 16,
        [Hint("lane arrow")] Left = 32,
        [Hint("lane arrow")] Right = 64,
        [Hint("2+ lanes merge together.\n"+
              "use in conjuction with TwoSegments extension flag to place merge arrows")]
        Merge = 128,

        LeftForward = 48,
        LeftRight = 96,
        ForwardRight = 80,
        LeftForwardRight = 112,

        [Hint("bus top (only works on pedestrian lanes where stop type is set to car)")]
        Stop = 256,

        [Hint("tram top (only works on pedestrian lanes where stop type is set to tram)")]
        Stop2 = 512,

        Stops = 768, // stop | stop2

        [Hint(LANE_YIELD_HEAD_TAIL)]
        YieldTail = 1024,
        [Hint(LANE_YIELD_HEAD_TAIL)]
        YieldHead = 2048,

        [Hint("useful for no left/right turn signs\n" + 
              "on tail* node,\n" +
              "left (when facting the node) segment is oneway toward the node" +
              HEAD_TAIL_DIRECTION)]
        TailOneWayLeft = 4096, // StartOneWayLeft

        [Hint("useful for no left/right turn signs\n" +
              "on tail* node,\n" +
              "right (when facting the node) segment is oneway toward the node" +
              HEAD_TAIL_DIRECTION)]
        TailtOneWayRight = 8192, // StartOneWayRight

        [Hint("useful for no left/right turn signs\n" +
              "on head* node,\n" +
              "left (when facting the node) segment is oneway toward the node" +
              HEAD_TAIL_DIRECTION)]
        HeadOneWayLeft = 16384, //EndOneWayLeft

        [Hint("useful for no left/right turn signs\n" +
              "on head* node,\n" +
              "right (when facting the node) segment is oneway toward the node" +
              HEAD_TAIL_DIRECTION)]
        HeadOneWayRight = 32768, //EndOneWayRight

        StartOneWayLeftInverted = 4100,
        StartOneWayRightInverted = 8196,
        EndOneWayLeftInverted = 16388,
        EndOneWayRightInverted = 32772
    }

    [Flags]
    [Hint("determines vehicle direction\n" +
          "on stations determines stop driection and bypass direction")]
    public enum Direction : byte {
        None = 0,
        Forward = 0b01,
        Backward = 0b10,
        [Hint("bi-directional")]
        Both = 0b11,
        [Hint("do not bypass")]
        Avoid = 0b1100,
        [Hint("avoid backward bypass + stop both directions")]
        AvoidBackward = 0b111,
        [Hint("avoid forward bypass + stop both directions")]
        AvoidForward = 0b1011,
        [Hint("do not bypass + stop both directions")]
        AvoidBoth = 0b1111
    }

    [Flags]
    public enum VehicleType {
        None = 0,
        [Hint("Road vehicle (eg: private car, bus, taxi, SOS, Services)")]
        Car = 1,
        Metro = 2,
        Train = 4,
        Ship = 8,
        Plane = 16,
        Bicycle = 32,
        Tram = 64,
        Helicopter = 128,
        Meteor = 256,
        Vortex = 512,
        Ferry = 1024,
        Monorail = 2048,
        CableCar = 4096,
        Blimp = 8192,
        Balloon = 16384,
        Rocket = 32768,
        Trolleybus = 65536,
        TrolleybusLeftPole = 131072,
        TrolleybusRightPole = 262144,
        All = 1048575
    }
}
