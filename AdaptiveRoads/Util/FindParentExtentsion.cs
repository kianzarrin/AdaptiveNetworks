namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
    using PrefabMetadata.API;

    public static class FindParentExtentsion {
        public static NetInfo GetParent(this NetInfo.Node node, out int nodeIndex) {
            foreach (var netInfo in NetInfoExtionsion.EditedNetInfos) {
                for(int i = 0; i< netInfo.m_nodes.Length; ++i) {
                    if (netInfo.m_nodes[i] == node) {
                        nodeIndex = i;
                        return netInfo;
                    }
                }
            }
            nodeIndex = -1;
            return null;
        }
        public static NetInfo GetParent(this NetInfo.Segment segment, out int segmentIndex) {
            foreach (var netInfo in NetInfoExtionsion.EditedNetInfos) {
                for (int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if (netInfo.m_segments[i] == segment) {
                        segmentIndex = i;
                        return netInfo;
                    }
                }
            }
            segmentIndex = -1;
            return null;
        }
        public static NetInfo GetParent(this NetInfo.Lane lane, out int laneIndex) {
            foreach (var netInfo in NetInfoExtionsion.EditedNetInfos) {
                for (int i = 0; i < netInfo.m_lanes.Length; ++i) {
                    if (netInfo.m_lanes[i] == lane) {
                        laneIndex = i;
                        return netInfo;
                    }
                }
            }
            laneIndex = -1;
            return null;
        }

        public static NetInfo GetParent(this NetLaneProps.Prop prop, out int laneIndex, out int propIndex) {
            foreach (var netInfo in NetInfoExtionsion.EditedNetInfos) {
                for (int i = 0; i < netInfo.m_lanes.Length; ++i) {
                    var props = netInfo.m_lanes[i].m_laneProps?.m_props;
                    if (props == null) continue;
                    for (int j = 0; j < props.Length; ++j) {
                        if (props[j] == prop) {
                            laneIndex = i;
                            propIndex = j;
                            return netInfo;
                        }
                    }
                }
            }
            propIndex = -1;
            laneIndex = -1;
            return null;
        }

        public static NetInfo GetParent(this IInfoExtended child) {
            if(child is NetInfo.Segment segment) {
                return GetParent(segment, out _);
            } else if (child is NetInfo.Node node) {
                return GetParent(node, out _);
            } else if (child is NetLaneProps.Prop prop) {
                return GetParent(prop, out _, out _);
            }
            return null;
        }

    }
}
