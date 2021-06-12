namespace AdaptiveRoads.Util {
    using KianCommons;
    using TrafficManager.Manager.Impl;

    internal static class LaneHelpers {
        static RoutingManager rman = RoutingManager.Instance;
        internal static bool HasProps(this NetInfo.Lane lane) =>
            (lane?.m_laneProps?.m_props?.Length ?? 0) > 0;


        // splits into multiple lanes at least one of which has only one transion to it.
        internal static bool SplitsUnique(this LaneData lane) {
            uint index = rman.GetLaneEndRoutingIndex(lane.LaneID, lane.StartNode);
            var transitions = rman.LaneEndForwardRoutings[index].transitions;
            if (transitions.Length < 2)
                return false;

            // any target lane that has only one transition to it?
            foreach (var transition in transitions) {
                uint targetIndex = rman.GetLaneEndRoutingIndex(transition.laneId, transition.startNode);
                var targetBackwardTransitions = rman.LaneEndBackwardRoutings[targetIndex].transitions;
                if (targetBackwardTransitions.Length == 1)
                    return true;
            }

            return false;
        }

        // has single transition which merges into another lane.
        internal static bool MergesUnique(this LaneData lane) {
            uint index = rman.GetLaneEndRoutingIndex(lane.LaneID, lane.StartNode);
            var transitions = rman.LaneEndForwardRoutings[index].transitions;
            if (transitions.Length != 1)
                return false;

            var transition = transitions[0];
            uint targetIndex = rman.GetLaneEndRoutingIndex(transition.laneId, transition.startNode);
            var targetBackwardTransitions = rman.LaneEndBackwardRoutings[targetIndex].transitions;

            return targetBackwardTransitions.Length >= 2; // other lane[s] also transition into target lane
        }
    }
}
