namespace AdaptiveRoads.Patches.Corner {
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System.Reflection;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using KianCommons.Math;
    using ColossalFramework.Math;

    /// <summary>
    /// Shifting needs to happen as early as possible because it is more than just a visual fix. it determines the position of the cars.
    /// </summary>
    [PreloadPatch]
    [HarmonyPatch]
    public static class CalculateCornerPatch {
        public static bool OverideSharpner;

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
        public static void Postfix( bool __runOriginal,
            ushort segmentID, bool start, bool leftSide,
            ref Vector3 cornerPos, ref Vector3 cornerDirection) {
            Shift(
                segmentID: segmentID, start: start, leftSide: leftSide,
                cornerPos: ref cornerPos, cornerDirection: ref cornerDirection);
            // __runOriginal does not work. reported bug to harmony author
             if (!OverideSharpner /*__runOriginal*/) {
                Sharpen2(
                    segmentID1: segmentID, startNode: start, leftSide: leftSide,
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

            var righward = CornerUtil.CalculateRighwardNormal(cornerDirection);
            bool headNode = segment.GetHeadNode() == nodeId;
            if (headNode)
                shift = -shift;
            cornerPos += shift * righward;
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

        /// <param name="segmentID1">segment to calculate corner</param>
        /// <param name="startNode">true for start node</param>
        /// <param name="leftSide">going away from the node</param>
        public static void Sharpen2(
        ushort segmentID1, bool startNode, bool leftSide,
        ref Vector3 cornerPos, ref Vector3 cornerDirection) {
            ref NetSegment segment1 = ref segmentID1.ToSegment();
            ushort nodeID = segment1.GetNode(startNode);
            bool sharp = segment1.Info?.GetMetaData()?.SharpCorners ?? false;
            if (!sharp)
                return;

            ref NetNode node = ref nodeID.ToNode();
            int nSegments = node.CountSegments();
            if(nSegments < 2) {
                return;
            }

            ushort segmentId2;
            if (leftSide /*right going toward junction*/) {
                segmentId2 = segment1.GetRightSegment(nodeID);
            } else {
                segmentId2 = segment1.GetLeftSegment(nodeID);
            }
            ref NetSegment segment2 = ref segmentId2.ToSegment();

            Vector3 pos = node.m_position;
            float hw1 = segment1.Info.m_halfWidth;
            float hw2 = segment2.Info.m_halfWidth;
            Vector3 dir1 = VectorUtils.NormalizeXZ(segment1.GetDirection(nodeID));
            Vector3 dir2 = VectorUtils.NormalizeXZ(segment2.GetDirection(nodeID));
            float sin = Vector3.Cross(dir1, dir2).y;
            sin = -sin;

            static bool SameDir(Vector3 tangent, Vector3 dir) {
                return VectorUtils.DotXZ(tangent, VectorUtils.NormalizeXZ(dir)) > 0.95f;
            }

            Vector3 otherPos1 = segment1.GetOtherNode(nodeID).ToNode().m_position;
            Vector3 otherPos2 = segment2.GetOtherNode(nodeID).ToNode().m_position;
            bool isStraight1 = SameDir(dir1,  otherPos1 - pos);
            bool isStraight2 = SameDir(dir2,  otherPos2 - pos);
            if(!isStraight1 || !isStraight2) {
                //var otherDir1 = otherPos1 - pos;
                //var otherDir1n = VectorUtils.NormalizeXZ(otherDir1);
                //float dotxz1 = VectorUtils.DotXZ(dir1, otherDir1n);
                //Log.Debug($"isStraight1={isStraight1} dir1={dir1} otherDir1={otherDir1} otherDir1.normalizedXZ={otherDir1n} dotxz1={dotxz1}");
                //var otherDir2 = otherPos2 - pos;
                //var otherDir2n = VectorUtils.NormalizeXZ(otherDir2);
                //float dotxz2 = VectorUtils.DotXZ(dir2, otherDir2n);
                //Log.Debug($"isStraight2={isStraight2} dir2={dir2} otherDir2={otherDir2} otherDir2.normalizedXZ={otherDir2n} dotxz2={dotxz2}");
                return;
            }


            Log.Debug($"p3: node:{nodeID}, segment:{segmentID1} segmentId2:{segmentId2} leftSide={leftSide} sin={sin}", false);
            if (Mathf.Abs(sin) > 0.001) {
                float scale = 1 / sin;
                if (!leftSide)
                    scale = -scale;

                pos += dir2 * hw1 * scale;
                pos += dir1 * hw2 * scale; // intersection.

                const float OFFSET_SAFETYNET = 0.02f;
                cornerPos = pos + dir1 * OFFSET_SAFETYNET;
            }
        }
    }
}
