namespace AdaptiveRoads.Util {
    using CSUtil.Commons;
    using KianCommons;
    using KianCommons.Math;
    using System.Diagnostics;
    using System.Linq;
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;
    using Log = KianCommons.Log;
    using VectorUtil = KianCommons.Math.VectorUtil;
    using static Util.Shortcuts;

    internal static class DirectConnectUtil {

        #region custom connect groups
        [Conditional("Debug")]
        internal static void AssertNotEmpty(int[] ar, string name) => Assertion.AssertDebug(ar != null && ar.Length == 0, $"{name} must be null if empty");

        // empty array must be null
        internal static bool ConnectGroupsMatch(int[] group1, int[] group2) {
            AssertNotEmpty(group1, "group1");
            AssertNotEmpty(group2, "group2");
            if (group1 == null) return false;
            if (group2 == null) return false;
            foreach (var g1 in group1) {
                foreach (var g2 in group2) {
                    if (g1 == g2)
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Broken Median detection
        public static bool HasUnbrokenMedian(ushort segmentID, ushort nodeID) {
            return new NodeSegmentIterator(nodeID).Any(HasUnbrokenMedianWith);
            bool HasUnbrokenMedianWith(ushort _segmentID2) => !OpenMedian(segmentID, _segmentID2);
        }

        public static bool OpenMedian(ushort segmentID1, ushort segmentID2) {
            ushort nodeID = segmentID1.ToSegment().GetSharedNode(segmentID2);
            bool connected = DoesSegmentGoToSegment(segmentID1, segmentID2, nodeID);
            connected |= DoesSegmentGoToSegment(segmentID2, segmentID1, nodeID);
            //Log.Debug("OpenMedian(): connected=" + connected);
            if (!connected)
                return true;

            return IsMedianBrokenHelper(segmentID1, segmentID2) ||
                   IsMedianBrokenHelper(segmentID2, segmentID1);
        }

        static bool IntersectAny(FastSegmentList segmentsA, FastSegmentList segmentsB) {
            for (int a = 0; a < segmentsA.Count; ++a)
                for (int b = 0; b < segmentsB.Count; ++b)
                    if (segmentsA[a] == segmentsB[b])
                        return true;
            return false;
        }

        public static bool IsMedianBrokenHelper(ushort segmentID, ushort otherSegmentID) {
            ushort nodeID = segmentID.ToSegment().GetSharedNode(otherSegmentID);

            GetGeometry(segmentID, otherSegmentID, out var farSegments, out var nearSegments);
            farSegments.Add(segmentID); // non of the far segments shall go to current segment.
            nearSegments.Add(segmentID); // current segment shall not go to any of the far segments(including itself)
            //Log.Debug($"IsMedianBrokenHelper({segmentID} ,{otherSegmentID}) :\n" +
            //    $"farSegments={farSegments.ToSTR()} , nearSegments={nearSegments.ToSTR()}");

            foreach (ushort nearSegmentID in nearSegments) {
                var targetSegments = GetTargetSegments(nearSegmentID, nodeID);
                if (IntersectAny(targetSegments, farSegments)) {
                    //Log.Debug($"intersection detected nearSegmentID:{nearSegmentID} targetSegments:{targetSegments.ToSTR()} farSegments:{farSegments.ToSTR()}");
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region segment 2 segment connections
        /// <summary>
        /// returns a list of all segments sourceSegmentID is connected to including itself
        /// if uturn is allowed.
        /// </summary>
        /// <param name="sourceSegmentID"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public static FastSegmentList GetTargetSegments(ushort sourceSegmentID, ushort nodeID) {
            var ret = new FastSegmentList();
            foreach (ushort targetSegmentID in new NodeSegmentIterator(nodeID)) {
                if (DoesSegmentGoToSegment(sourceSegmentID, targetSegmentID, nodeID))
                    ret.Add(targetSegmentID);
            }
            return ret;
        }

        /// <summary>
        /// Determines if any lane from source segment goes to the target segment
        /// based on lane arrows and lane connections.
        public static bool DoesSegmentGoToSegment(ushort sourceSegmentID, ushort targetSegmentID, ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(sourceSegmentID, nodeID);
            if (sourceSegmentID == targetSegmentID) {
                return JRMan.IsUturnAllowed(sourceSegmentID, startNode);
            }
            ArrowDirection arrowDir = TMPE.ExtSegmentEndManager.GetDirection(sourceSegmentID, targetSegmentID, nodeID);
            LaneArrows arrow = ArrowDir2LaneArrows(arrowDir);
            var sourceLanes = new LaneDataIterator(
                sourceSegmentID,
                startNode,
                LaneArrowMan.LaneTypes,
                LaneArrowMan.VehicleTypes);
            //Log.Debug("DoesSegmentGoToSegment: sourceLanes=" + sourceLanes.ToSTR());

            foreach (LaneData sourceLane in sourceLanes) {
                bool connected;
                if (LCMan.HasConnections(sourceLane.LaneID, startNode)) {
                    connected = IsLaneConnectedToSegment(sourceLane.LaneID, targetSegmentID);
                    //Log.Debug($"IsLaneConnectedToSegment({sourceLane},{targetSegmentID}) = {connected}");

                } else {
                    LaneArrows arrows = LaneArrowMan.GetFinalLaneArrows(sourceLane.LaneID);
                    connected = (arrows & arrow) != 0;
                }
                if (connected)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if there is any lane connection from source lane to target segment.
        /// </summary>
        public static bool IsLaneConnectedToSegment(uint sourceLaneId, ushort targetSegmentID) {
            ushort sourceSegmentID = sourceLaneId.ToLane().m_segment;
            ushort nodeID = sourceSegmentID.ToSegment().GetSharedNode(targetSegmentID);
            bool sourceStartNode = NetUtil.IsStartNode(sourceSegmentID, nodeID);
            bool targetStartNode = NetUtil.IsStartNode(targetSegmentID, nodeID);


            var targetLanes = new LaneDataIterator(
                targetSegmentID,
                !targetStartNode,// going away from start node.
                LCMan.LaneTypes,
                LCMan.VehicleTypes);
            foreach (LaneData targetLane in targetLanes) {
                if (LCMan.AreLanesConnected(sourceLaneId, targetLane.LaneID, sourceStartNode))
                    return true;
            }
            return false;
        }

        public static LaneArrows ArrowDir2LaneArrows(ArrowDirection arrowDir) {
            switch (arrowDir) {
                case ArrowDirection.Forward:
                    return LaneArrows.Forward;
                case ArrowDirection.Left:
                    return LaneArrows.Left;
                case ArrowDirection.Right:
                    return LaneArrows.Right;
                default:
                    return LaneArrows.None;
            }
        }
        #endregion

        #region Geometry

        public static void GetGeometry(ushort segmentID1, ushort segmentID2,
            out FastSegmentList farSegments, out FastSegmentList nearSegments) {
            farSegments = new FastSegmentList();
            nearSegments = new FastSegmentList();
            ushort nodeID = segmentID1.ToSegment().GetSharedNode(segmentID2);
            var angle0 = GetSegmentsAngle(segmentID1, segmentID2);
            for (int i = 0; i < 8; ++i) {
                ushort segmentID = nodeID.ToNode().GetSegment(i);
                if (segmentID == 0 || segmentID == segmentID1 || segmentID == segmentID2)
                    continue;
                var angle = GetSegmentsAngle(segmentID1, segmentID);
                bool near = VectorUtil.CompareAngles_CCW_Right(source: angle0, target: angle);
                near = near ^ NetUtil.LHT;
                //Log.Debug($"GetGeometry({segmentID1}, {segmentID2}) : segment:{segmentID}\n" +
                //    $" CompareAngles_CCW_Right(angle0:{angle0*Mathf.Rad2Deg}, angle:{angle*Mathf.Rad2Deg}) -> {near}");
                if (near)
                    nearSegments.Add(segmentID);
                else
                    farSegments.Add(segmentID);
            }

        }

        public static float GetSegmentsAngle(ushort from, ushort to) {
            ushort nodeID = from.ToSegment().GetSharedNode(to);
            Vector3 dir1 = from.ToSegment().GetDirection(nodeID);
            Vector3 dir2 = to.ToSegment().GetDirection(nodeID);
            float ret = VectorUtil.SignedAngleRadCCW(dir1.ToCS2D(), dir2.ToCS2D());
            //Log.Debug($"SignedAngleRadCCW({dir1} , {dir2}) => {ret}\n"+
            //           $"GetSegmentsAngle({from} , {to}) => {ret}");
            return ret;
        }
        #endregion

    }
}
