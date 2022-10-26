namespace AdaptiveRoads.Patches.Track {
    using AdaptiveRoads.Data.NetworkExtensions;
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using System;
    using UnityEngine;

    [HarmonyPatch(typeof(NetTool), "RenderSegment")]
    [InGamePatch]
    public static class NeTTool_RenderSegment {
        public static void Postfix(NetInfo info, NetSegment.Flags flags, Vector3 startPosition, Vector3 endPosition, Vector3 startDirection, Vector3 endDirection, bool smoothStart, bool smoothEnd) {
            OutlineData outline = new(
                    startPosition, endPosition,
                    startDirection, -endDirection,
                    info.m_halfWidth * 2,
                    smoothStart, smoothEnd);

            NetSegmentExt.Render(info, flags, in outline, smoothStart, smoothEnd);
        }
    }
}
