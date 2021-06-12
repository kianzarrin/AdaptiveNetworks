namespace AdaptiveRoads.Util {
    using KianCommons;
    using TrafficManager.Manager.Impl;
    using System.Diagnostics;
    using TrafficManager.API.Manager;
    using System.Collections.Generic;
    using System.Linq;
    using TrafficManager.API.Traffic.Enums;

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
        internal static bool SplitsUnique(this LaneData lane) {
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
        internal static bool MergesUnique(this LaneData lane) {
            var transitions = GetForwardTransisions(lane.LaneID, lane.StartNode);
            if (transitions.Count() != 1)
                return false; // not single transition

            var transition = transitions.First();
            var targetBackwardTransitions = GetBackwardTransisions(transition.laneId, transition.startNode);
            return targetBackwardTransitions.Count() >= 2; // merge
        }
    }
}
