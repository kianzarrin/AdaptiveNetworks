namespace AdaptiveRoads.Patches.Corner {
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System.Reflection;
    using UnityEngine;
    using static ColossalFramework.Math.VectorUtils;
    using AdaptiveRoads.Util;
    using AdaptiveRoads.Manager;

    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch]
    static class CalculateCornerPatch {
        [UsedImplicitly]
        [HarmonyBefore("CS.Kian.NodeController")]
        static MethodBase TargetMethod() {
            // public void CalculateCorner(ushort segmentID, bool heightOffset, bool start, bool leftSide,
            // out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
            return typeof(NetSegment).GetMethod(
                    nameof(NetSegment.CalculateCorner),
                    BindingFlags.Public | BindingFlags.Instance) ??
                    throw new System.Exception("CalculateCornerPatch Could not find target method.");
        }

        /// <param name="segmentID">segment to calculate corner</param>
        /// <param name="start">true for start node</param>
        /// <param name="leftSide">going away from the node</param>
        public static void Postfix(
            ushort segmentID, bool start, bool leftSide,
            ref Vector3 cornerPos, ref Vector3 cornerDirection) {
            Shift(
                segmentID: segmentID, start: start, leftSide: leftSide,
                cornerPos: ref cornerPos, cornerDirection: ref cornerDirection);
        }

        public static void Shift(
            ushort segmentID, bool start, bool leftSide,
            ref Vector3 cornerPos, ref Vector3 cornerDirection) {
            ref NetSegment segment = ref segmentID.ToSegment();
            float shift = segment.Info?.GetMetaData()?.Shift ?? 0;
            if (shift == 0) return;

            ushort nodeID = segment.GetNode(start);
            bool headNode = segment.GetHeadNode() == nodeID;
            if (!headNode) shift = -shift;

            CornerUtil.CalculateTransformVectors(cornerDirection, leftSide, out var outward, out var forward);
            bool rightSideGoingTowardTheJunction = !leftSide;
            var rightward = rightSideGoingTowardTheJunction ? outward : -outward;

            cornerPos += shift * rightward;
        }

    }
}
