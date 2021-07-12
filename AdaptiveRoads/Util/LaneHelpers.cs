namespace AdaptiveRoads.Util {
    using KianCommons;
    using System.Collections.Generic;
    using System.Linq;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using AdaptiveRoads.Manager;
    using ColossalFramework.Math;
    using UnityEngine;

    internal static class LaneHelpers {
        static RoutingManager rman => RoutingManager.Instance;
        internal static bool HasProps(this NetInfo.Lane lane) =>
            (lane?.m_laneProps?.m_props?.Length ?? 0) > 0;

        /// <summary>
        /// note: does not return null
        /// </summary>
        /// <returns>valid forward transions from the given lane end to other lanes</returns>
        internal static IEnumerable<LaneTransitionData> GetForwardTransisions(uint laneID, bool startNode) {
            Assertion.NotNull(rman);
            uint index = rman.GetLaneEndRoutingIndex(laneID, startNode);
            var transisions = rman.LaneEndForwardRoutings[index].transitions
                ?.Where(t => t.type != LaneEndTransitionType.Invalid);
            return transisions ?? Enumerable.Empty<LaneTransitionData>();
        }

        /// <summary>
        /// note: does not return null
        /// </summary>
        /// <returns>valid backward transions from other lanes to the given lane end</returns>
        internal static IEnumerable<LaneTransitionData> GetBackwardTransisions(uint laneID, bool startNode) {
            Assertion.NotNull(rman);
            uint index = rman.GetLaneEndRoutingIndex(laneID, startNode);
            var transisions = rman.LaneEndBackwardRoutings[index].transitions
                ?.Where(t => t.type != LaneEndTransitionType.Invalid);
            return transisions ?? Enumerable.Empty<LaneTransitionData>();
        }

        // splits into multiple lanes at least one of which has only one transion to it.
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
                ushort segmentID2 = transition.segmentId;
                arrows |= ArrowDirectionUtil.GetArrowExt(segmentID, segmentID2);
            }
            return arrows;//.LogRet($"GetArrowsExt(lane:{lane.LaneID} segment:{lane.SegmentID})");
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


    }
}
