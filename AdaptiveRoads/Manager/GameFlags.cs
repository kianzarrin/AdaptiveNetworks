using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using CSUtil.Commons;
using KianCommons;
using Log = KianCommons.Log;

namespace AdaptiveRoads.Manager {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class HideAttribute :Attribute {
        bool Get = true;
        bool Set = true;
        public HideAttribute() { }
        public HideAttribute(bool get, bool set) {
            Get = get;
            Set = set;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HintAttribute : Attribute {
        public string Text;
        public HintAttribute(string text) => Text = text;
    }

    public enum NetSegmentFlags {
        //Created = 1,
        //Deleted = 2,
        //Original = 4,

        [Hint("Destroyed due to disaster")]
        Collapsed = 8,

        Invert = 16,
        Untouchable = 32,
        End = 64,
        Bend = 128,
        WaitingPath = 256,
        PathFailed = 512,
        PathLength = 1024,
        AccessFailed = 2048,
        TrafficStart = 4096,
        TrafficEnd = 8192,
        CrossingStart = 16384,
        CrossingEnd = 32768,
        StopRight = 65536,
        StopLeft = 131072,
        StopRight2 = 262144,
        StopLeft2 = 524288,
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
        YieldStart = 1073741824,
        YieldEnd = -2147483648,
        StopBoth = 196608,
        StopBoth2 = 786432,
        StopAll = 983040,
        CombustionBan = 256,
    }


}
