namespace AdaptiveRoads.Patches.Corner {
    using JetBrains.Annotations;
    using KianCommons.Patches;

    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch2(typeof(NetNode), typeof(RefreshJunctionData), instance: true)]
    static class RefreshJunctionDataDCPatch {
        delegate void RefreshJunctionData(NetNode instance,
            ushort nodeID, int segmentIndex, int segmentIndex2,
            NetInfo info, NetInfo info2,
            ushort nodeSegment, ushort nodeSegment2,
            ref uint instanceIndex, ref RenderManager.Instance data);

        static void Prefix(ushort nodeSegment, ushort nodeSegment2) =>
            ShiftData.Prefix(nodeSegment, nodeSegment2);

        static void Postfix() => ShiftData.Postfix();
    }
}
