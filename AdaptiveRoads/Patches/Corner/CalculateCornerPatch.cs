namespace AdaptiveRoads.Patches.Corner {
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System.Reflection;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using KianCommons.Math;

    /// <summary>
    /// Shifting needs to happen as early as possible because it is more than just a visual fix. it determines the position of the cars.
    /// </summary>
    [PreloadPatch]
    [HarmonyPatch]
    static class CalculateCornerPatch {
        [UsedImplicitly]
        [HarmonyBefore("CS.Kian.NodeController")]
        static MethodBase TargetMethod() {
            // public void CalculateCorner(ushort segmentID, bool heightOffset, bool start, bool leftSide,
            // out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
            return typeof(NetSegment).GetMethod(
                    nameof(NetSegment.CalculateCorner),
                    BindingFlags.Public | BindingFlags.Instance, throwOnError: true);
        }

        /// <param name="segmentID">segment to calculate corner</param>
        /// <param name="start">true for start node</param>
        /// <param name="leftSide">going away from the node</param>
        public static void Postfix(bool __runOriginal,
            ushort segmentID, bool start, bool leftSide,
            ref Vector3 cornerPos, ref Vector3 cornerDirection) {
            Shift(
                segmentID: segmentID, start: start, leftSide: leftSide,
                cornerPos: ref cornerPos, cornerDirection: ref cornerDirection);
            if (__runOriginal) {
                // if NCR takes over the node, then don't sharpen it.
                Sharpen(
                    segmentID: segmentID, start: start, leftSide: leftSide,
                    cornerPos: ref cornerPos, cornerDirection: ref cornerDirection);
            }
        }

        public static void Shift(
            ushort segmentID, bool start, bool leftSide,
            ref Vector3 cornerPos, ref Vector3 cornerDirection) {
            ref NetSegment segment = ref segmentID.ToSegment();
            ushort nodeId = segment.GetNode(start);
            float shift = API.GetShift(segmentId: segmentID, nodeId: nodeId);
            if (shift == 0) return;

            CornerUtil.CalculateTransformVectors(cornerDirection, leftSide, outward: out var outward, out var _);
            bool headNode = segment.GetHeadNode() == nodeId;
            if (headNode ^ leftSide)
                shift = -shift;
            cornerPos += shift * outward;
        }

        /// <param name="segmentID">segment to calculate corner</param>
        /// <param name="start">true for start node</param>
        /// <param name="leftSide">going away from the node</param>
        public static void Sharpen(
        ushort segmentID, bool start, bool leftSide,
        ref Vector3 cornerPos, ref Vector3 cornerDirection) {
            ref NetSegment segment = ref segmentID.ToSegment();
            ushort nodeID = segment.GetNode(start);
            bool sharp = segment.Info?.GetMetaData()?.SharpCorners ?? false;
            if (!sharp)
                return;

            ushort otherSegmentID;
            if (leftSide /*right going toward junction*/) {
                otherSegmentID = segment.GetRightSegment(nodeID);
            } else {
                otherSegmentID = segment.GetLeftSegment(nodeID);
            }
            ref NetSegment otherSegment = ref otherSegmentID.ToSegment();

            var angleDegree = VectorUtil.SignedAngleRadCCW(
                segment.GetDirection(nodeID).ToCS2D(), // from
                otherSegment.GetDirection(nodeID).ToCS2D()) * Mathf.Rad2Deg; // to

            if (!leftSide)
                angleDegree = -angleDegree;

            Log.Debug($"p3: node:{nodeID}, segment:{segmentID} otherSegment:{otherSegmentID} leftSide={leftSide} angle={angleDegree}", false);
            float backward;
            if (MathUtil.EqualAprox(angleDegree, 90)) {
                backward = 2;
            } else if (MathUtil.EqualAprox(angleDegree, -90)) {
                float w = 2 * otherSegment.Info.m_halfWidth;
                backward = w + 2;
            } else {
                return;// TODO support more angles.
            }
            cornerPos -= backward * cornerDirection;
        }

    }
}
