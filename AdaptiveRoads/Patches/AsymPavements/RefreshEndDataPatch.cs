namespace AdaptiveRoads.Patches {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;

    /// <summary>
    /// Modifies data vectors to match asymmetric pavements.
    /// </summary>
    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch(typeof(NetNode), "RefreshEndData")]
    static class RefreshEndDataPatch {
        //private void NetNode.RefreshEndData
        //  (ushort nodeID, NetInfo info, uint instanceIndex, ref RenderManager.Instance data)
        static void Postfix(ushort nodeID, NetInfo info, ref RenderManager.Instance data) {
            var net = info.GetMetaData();
            if (net == null) return;
            float pwL = info.m_pavementWidth;
            float pwR = net.PavementWidthRight;
            if (pwL == pwR) return;

            float w = info.m_halfWidth * 2;

            float pwM = pwR; // TODO calculate
            float r = pwM / w;

            ref var segment = ref nodeID.ToNode().GetFirstSegment().ToSegment();
            bool reverse = segment.IsStartNode(nodeID) ^ segment.IsInvert();
            if (!reverse)
                data.m_dataVector2.x = r;
            else
                data.m_dataVector2.z = r;
        }
    }
}