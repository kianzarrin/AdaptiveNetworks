namespace AdaptiveRoads.Util {
    using KianCommons;
    using System.Collections.Generic;
    using System.Linq;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Enums;
    using ColossalFramework.Math;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using TrafficManager;

    internal static class LaneHelpers {
        static IManagerFactory TMPE => Constants.ManagerFactory;
        static ILaneConnectionManager LCMan => TMPE?.LaneConnectionManager;
        static IRoutingManager RMan => TMPE?.RoutingManager;

        internal static bool HasProps(this NetInfo.Lane lane) =>
            (lane?.m_laneProps?.m_props?.Length ?? 0) > 0;

        /// <summary>
        /// note: does not return null
        /// </summary>
        /// <returns>valid forward transition from the given lane end to other lanes</returns>
        internal static IEnumerable<LaneTransitionData> GetForwardTransisions(uint laneID, bool startNode) {
            Assertion.NotNull(RMan);
            uint index = RMan.GetLaneEndRoutingIndex(laneID, startNode);
            var transisions = RMan.LaneEndForwardRoutings[index].transitions
                ?.Where(t => t.type != LaneEndTransitionType.Invalid);
            return transisions ?? Enumerable.Empty<LaneTransitionData>();
        }

        /// <summary>
        /// note: does not return null
        /// </summary>
        /// <returns>valid backward transitions from other lanes to the given lane end</returns>
        internal static IEnumerable<LaneTransitionData> GetBackwardTransisions(uint laneID, bool startNode) {
            Assertion.NotNull(RMan);
            uint index = RMan.GetLaneEndRoutingIndex(laneID, startNode);
            var transisions = RMan.LaneEndBackwardRoutings[index].transitions
                ?.Where(t => t.type != LaneEndTransitionType.Invalid);
            return transisions ?? Enumerable.Empty<LaneTransitionData>();
        }

        // splits into multiple lanes at least one of which has only one transitions to it.
        internal static bool IsSplitsUnique(this LaneData lane) {
            var transitions = GetForwardTransisions(lane.LaneID, lane.StartNode);

            if (transitions.Count() < 2)
                return false; // not split

            // any target lane that has only one transition to it?
            foreach (var transition in transitions) {
                var targetBackwardTransitions = GetBackwardTransisions(transition.laneId, transition.startNode);
                if (targetBackwardTransitions.Count() == 1)
                    return true;
            }

            return false;
        }

        // has single transition which merges into another lane.
        internal static bool IsMergesUnique(this LaneData lane) {
            var transitions = GetForwardTransisions(lane.LaneID, lane.StartNode);
            if (transitions.Count() != 1)
                return false; // not single transition

            var transition = transitions.First();
            var targetBackwardTransitions = GetBackwardTransisions(transition.laneId, transition.startNode);
            return targetBackwardTransitions.Count() >= 2; // merge
        }

        internal static NetLaneExt.Flags GetArrowsExt(ref this LaneData lane) {
            NetLaneExt.Flags arrows = 0;
            ushort segmentID = lane.SegmentID;
            foreach(var transition in GetForwardTransisions(lane.LaneID, lane.StartNode)) {
                if (transition.type is LaneEndTransitionType.Default or LaneEndTransitionType.LaneConnection) {
                    arrows |= ArrowDirectionUtil.GetArrowExt(segmentID, transition.segmentId);
                }
            }
            return arrows;//.LogRet($"GetArrowsExt(lane:{lane.LaneID} segment:{lane.SegmentID})");
        }

        internal static bool HasAnyConnections(this uint laneId) {
            if (LCMan != null) {
                return LCMan.HasConnections(laneId, true) || LCMan.HasConnections(laneId, false);
            } else {
                return false;
            }
        }



        public static float CalculateCurve(this Bezier3 bezier) {
            // see NetLane.UpdateLength()
            Vector3 d1 = bezier.b - bezier.a;
            Vector3 d2 = bezier.c - bezier.b;
            Vector3 d3 = bezier.d - bezier.c;
            float m1 = d1.magnitude;
            float m2 = d2.magnitude;
            float m3 = d3.magnitude;
            if (m1 > 0.1f) d1 /= m1;
            if (m3 > 0.1f) d3 /= m3;

            var length = m1 + m2 + m3;
            var curve = (Mathf.PI * 0.5f) * (1f - Vector3.Dot(d1, d3));
            if (length > 0.1f) curve /= length;

            return curve;
        }

        public static IEnumerable<LaneData> GetSimilarLanes(int laneIndex, IEnumerable<ushort> segmentIDs_) {
            foreach(var segmentID in segmentIDs_ ?? new ushort[0]) {
                foreach(var lane2 in NetUtil.IterateSegmentLanes(segmentID)) {
                    if(lane2.LaneIndex == laneIndex) {
                        yield return lane2;
                        break; // optimisation.
                    }
                }
            }
        }

        public static IEnumerable<LaneData> GetSimilarLanes(int laneIndex, NetInfo prefab) {
            for(ushort segmentID = 1; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                if(segmentID.ToSegment().IsValid() && segmentID.ToSegment().Info == prefab) {
                    foreach(var lane2 in NetUtil.IterateSegmentLanes(segmentID)) {
                        if(lane2.LaneIndex == laneIndex) {
                            yield return lane2;
                            break; // optimisation.
                        }
                    }
                }
            }
        }
    }
}
