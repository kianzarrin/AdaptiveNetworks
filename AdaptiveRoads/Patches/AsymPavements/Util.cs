namespace AdaptiveRoads.Patches.AsymPavements {
    using System;

    public static class Util {
        public enum Operation {
            Vanilla, //return input width
            PWBig,
            PWSmall,
            PWAR,
            PWAR2,
            PWForced,
        }

        [Flags]
        public enum Geometry {
            None = 0,
            Reverse = 1,
            BiggerLeft = 2,
        }

        public static Geometry GetGeometry(bool reverse, bool biggerLeft) {
            Geometry ret = Geometry.None;
            if (reverse)
                ret |= Geometry.Reverse;
            if (biggerLeft)
                ret |= Geometry.BiggerLeft;
            return ret;
        }

        const int CASE_COUNT = 6;
        const int GEOMETRY_COUNT = 4;
        public static Operation[,] Operations = new Operation[CASE_COUNT, GEOMETRY_COUNT] {
                //     right          right-reverse       left             left-reverse    */                                                
                { Operation.PWBig  , Operation.PWAR   , Operation.PWAR   , Operation.PWBig  }, // case 1
                { Operation.PWSmall, Operation.PWBig  , Operation.PWBig  , Operation.PWSmall}, // case 2
                { Operation.PWAR   , Operation.PWBig  , Operation.PWBig  , Operation.PWAR   }, // case 3
                { Operation.PWBig  , Operation.PWSmall, Operation.PWSmall, Operation.PWBig  }, // case 4
                { Operation.PWBig  , Operation.PWBig  , Operation.PWBig  , Operation.PWBig  }, // case 5
                { Operation.PWBig  , Operation.PWBig  , Operation.PWBig  , Operation.PWBig  }, // case 6
            };
        public static float[,] Forced = new float[CASE_COUNT, GEOMETRY_COUNT];

        public static Operation GetOperation(int occurance, bool reverse, bool biggerLeft) {
            int index = occurance - 1;
            var geometry = GetGeometry(reverse: reverse, biggerLeft: biggerLeft);
            return Operations[index, (int)geometry];
        }

        public static float GetForced(int occurance, bool reverse, bool biggerLeft) {
            int index = occurance - 1;
            var geometry = GetGeometry(reverse: reverse, biggerLeft: biggerLeft);
            return Forced[index, (int)geometry];
        }
    }
}
