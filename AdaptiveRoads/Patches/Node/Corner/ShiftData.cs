namespace AdaptiveRoads.Patches.Corner {
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System.Reflection;

    public static class ShiftData {
        public static float Shift { get; private set; }
        public static ushort TargetSegmentID;

        public static MethodInfo mPrefix = typeof(ShiftData).GetMethod(nameof(Prefix), throwOnError: true);
        public static MethodInfo mPostfix = typeof(ShiftData).GetMethod(nameof(Postfix), throwOnError: true);

        public static void Prefix(ushort SourceSegmentID, ushort targetSegmentID) {
            Shift = SourceSegmentID.ToSegment().Info.GetMetaData()?.Shift ?? 0; // target segment uses source shift.
            TargetSegmentID = targetSegmentID;
        }

        public static void Postfix() {
            TargetSegmentID = 0;
        }
    }
}