namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
    using System.Linq;

    internal static class LaneHelpers {
        internal static bool HasProps(this NetInfo.Lane lane) =>
            (lane?.m_laneProps?.m_props?.Length ?? 0) > 0;

    }
}
