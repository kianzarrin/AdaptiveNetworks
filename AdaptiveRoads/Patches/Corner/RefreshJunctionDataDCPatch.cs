namespace AdaptiveRoads.Patches.Corner {
    using AdaptiveRoads.Manager;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using System.Reflection;

    public static class ShiftData {
        public static float Shift;
        public static ushort TargetSegmentID;
    }

    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch2(typeof(NetNode), typeof(RefreshJunctionData), instance:true)]
    static class RefreshJunctionDataDCPatch {
        delegate void RefreshJunctionData( NetNode instance,
            ushort nodeID, int segmentIndex, int segmentIndex2,
            NetInfo info, NetInfo info2,
            ushort nodeSegment, ushort nodeSegment2,
            ref uint instanceIndex, ref RenderManager.Instance data);

        static MethodInfo mCaclculateCorner = typeof(NetSegment)
            .GetMethod(nameof(NetSegment.CalculateCorner), BindingFlags.Public | BindingFlags.Instance, throwOnError: true);

        static void Prefix(ushort nodeSegment, ushort nodeSegment2) {
            ShiftData.Shift = nodeSegment.ToSegment().Info.GetMetaData()?.Shift ?? 0; // target segment uses source shift.
            ShiftData.TargetSegmentID = nodeSegment2;
        }

        static void Postfix() {
            ShiftData.TargetSegmentID = 0;
        }

    }
}
