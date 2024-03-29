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
            if (Log.VERBOSE) Log.Debug($"RefreshJunctionData(segments: {SourceSegmentID} -> {targetSegmentID})");
            Shift = SourceSegmentID.ToSegment().Info.GetFinalShift(); // target segment uses source shift.
            TargetSegmentID = targetSegmentID;
        }

        public static void Postfix() {
            TargetSegmentID = 0;
        }
    }
}