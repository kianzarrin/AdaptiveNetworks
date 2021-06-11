namespace AdaptiveRoads.Patches.Corner {
    using AdaptiveRoads.Manager;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using System.Reflection;


    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch2(typeof(NetNode), typeof(RefreshJunctionData))]
    static class RefreshJunctionDataDCPatch {
        delegate void RefreshJunctionData(
            ushort nodeID, int segmentIndex, int segmentIndex2,
            NetInfo info, NetInfo info2,
            ushort nodeSegment, ushort nodeSegment2,
            ref uint instanceIndex, ref RenderManager.Instance data);

        static MethodInfo mCaclculateCorner = typeof(NetSegment)
            .GetMethod(nameof(NetSegment.CalculateCorner), BindingFlags.Public | BindingFlags.Instance, throwOnError: true);

        public static float Shift { get; private set; }
        public static ushort TargetSegmentID;

        static void Prefix(ushort nodeSegment, ushort nodeSegment2) {
            Shift = nodeSegment.ToSegment().Info.GetMetaData()?.Shift ?? 0; // target segment uses source shift.
            TargetSegmentID = nodeSegment2;
        }

        static void Postfix() {
            TargetSegmentID = 0;
        }

    }
}
