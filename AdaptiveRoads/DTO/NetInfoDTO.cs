namespace AdaptiveRoads.DTO {
    using AdaptiveRoads.Manager;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;

    public class NetInfoDTO : IDTO {
        public float m_halfWidth = 8f;
        public float m_pavementWidth = 3f;
        public float m_segmentLength = 64f;
        public float m_minHeight;
        public float m_maxHeight = 5f;
        public float m_maxSlope = 0.25f;
        public float m_maxBuildAngle = 180f;
        public float m_maxTurnAngle = 180f;
        public float m_minCornerOffset;
        public float m_maxCornerOffset;
        public float m_buildHeight;
        public float m_surfaceLevel;
        public float m_terrainStartOffset;
        public float m_terrainEndOffset;
        public bool m_createPavement;
        public bool m_createGravel;
        public bool m_createRuining;
        public bool m_flattenTerrain;
        public bool m_lowerTerrain;
        public bool m_clipTerrain;
        public bool m_followTerrain;
        public bool m_flatJunctions;
        public bool m_clipSegmentEnds;
        public bool m_twistSegmentEnds;
        public bool m_straightSegmentEnds;
        public bool m_enableBendingSegments;
        public bool m_enableBendingNodes;
        public bool m_enableMiddleNodes;
        public bool m_requireContinuous;
        public bool m_canCrossLanes = true;
        public bool m_canCollide = true;
        public bool m_blockWater;
        public bool m_autoRemove;
        public bool m_overlayVisible = true;
        public NetInfo.ConnectGroup m_connectGroup;
        public Vehicle.Flags m_setVehicleFlags;
        public string m_UICategory;

        public NetInfoExtionsion.Net MetaData;

        public void WriteToGame(NetInfo info) {
            if (info == null) return;
            DTOUtil.CopyAllMatchingFields<NetInfoDTO>(info, this);
            info.m_lanes = DTOUtil.CopyArray<NetInfo.Lane>(m_lanes);
            info.m_segments = DTOUtil.CopyArray<NetInfo.Segment>(m_segments);
            info.m_nodes = DTOUtil.CopyArray<NetInfo.Node>(m_nodes);
            info.SetMeteData(MetaData);
        }

        public void ReadFromGame(NetInfo info) {
            DTOUtil.CopyAllMatchingFields<NetInfoDTO>(this, info);
            MetaData = info.GetMetaData().Clone();
            m_lanes = DTOUtil.CopyArray<Lane>(info.m_lanes);
            m_segments = DTOUtil.CopyArray<Segment>(info.m_segments);
            m_nodes = DTOUtil.CopyArray<Node>(info.m_nodes);
        }

        public Lane[] m_lanes;
        public Segment[] m_segments;
        public Node[] m_nodes;

        public class Node {
            public NetNode.Flags m_flagsRequired;
            public NetNode.Flags m_flagsForbidden;
            public NetInfo.ConnectGroup m_connectGroup;
            public bool m_directConnect;

            public NetInfoExtionsion.Node MetaData;

            public static explicit operator NetInfo.Node(Node node) {
                var ret =  DTOUtil.FromDTO<NetInfo.Node, Node>(node);
                if (node.MetaData != null) {
                    var retExt = ret.Extend();
                    retExt.SetMetaData(node.MetaData.Clone());
                    return retExt.Base;
                }
                return ret;
            }
            public static explicit operator Node(NetInfo.Node node) {
                var ret = DTOUtil.ToDTO<Node>(node);
                ret.MetaData = node.GetMetaData().Clone();
                return ret;
            }
        }

        public class Segment {
            public NetSegment.Flags m_forwardRequired;
            public NetSegment.Flags m_forwardForbidden;
            public NetSegment.Flags m_backwardRequired;
            public NetSegment.Flags m_backwardForbidden;
            public bool m_emptyTransparent;
            public bool m_disableBendNodes;

            public NetInfoExtionsion.Segment MetaData;

            public static explicit operator NetInfo.Segment(Segment segment) {
                var ret = DTOUtil.FromDTO<NetInfo.Segment, Segment>(segment);
                if (segment.MetaData != null) {
                    var retExt = ret.Extend();
                    retExt.SetMetaData(segment.MetaData.Clone());
                    return retExt.Base;
                }
                return ret;
            }

            public static explicit operator Segment(NetInfo.Segment segment) {
                var ret = DTOUtil.ToDTO<Segment>(segment);
                ret.MetaData = segment.GetMetaData().Clone();
                return ret;
            }
        }

        public class Lane {
            public float m_position;
            public float m_width = 3f;
            public float m_verticalOffset;
            public float m_stopOffset;
            public float m_speedLimit = 1f;
            public NetInfo.Direction m_direction = NetInfo.Direction.Forward;
            public NetInfo.LaneType m_laneType;
            public VehicleInfo.VehicleType m_vehicleType;
            public VehicleInfo.VehicleType m_stopType;
            public bool m_allowConnect = true;
            public bool m_useTerrainHeight;
            public bool m_centerPlatform;
            public bool m_elevated;
            public Prop[] m_props;

            public static explicit operator NetInfo.Lane(Lane lane) {
                var ret = new NetInfo.Lane();
                DTOUtil.CopyAllMatchingFields<Lane>(ret, lane);

                ret.m_laneProps = new NetLaneProps {
                    m_props = DTOUtil.CopyArray<NetLaneProps.Prop>(lane.m_props)
                };

                return ret;
            }
            public static explicit operator Lane(NetInfo.Lane lane) {
                var dto = new Lane();
                DTOUtil.CopyAllMatchingFields<Lane>(dto, lane);
                dto.m_props = DTOUtil.CopyArray<Prop>(lane.m_laneProps.m_props);
                return dto;
            }
        }

        public class Prop {
            public NetLane.Flags m_flagsRequired;
            public NetLane.Flags m_flagsForbidden;
            public NetNode.Flags m_startFlagsForbidden;
            public NetNode.Flags m_startFlagsRequired;
            public NetNode.Flags m_endFlagsForbidden;
            public NetNode.Flags m_endFlagsRequired;

            public int m_probability;
            public float m_cornerAngle;
            public float m_minLength;
            public float m_repeatDistance;
            public float m_segmentOffset;
            public float m_angle;
            public NetLaneProps.ColorMode m_colorMode;

            public Vector3DTO m_position;
            public PrefabInfoDTO<PropInfo> m_prop;
            public PrefabInfoDTO<TreeInfo> m_tree;

            public NetInfoExtionsion.LaneProp MetaData;

            public static explicit operator NetLaneProps.Prop(Prop prop) {
                var ret = new NetLaneProps.Prop();
                DTOUtil.CopyAllMatchingFields<Prop>(ret, prop);
                //ret.m_finalProp = ret.m_prop;
                //ret.m_finalTree = ret.m_tree;
                if (prop.MetaData != null) {
                    var retExt = ret.Extend();
                    retExt.SetMetaData(prop.MetaData.Clone());
                    return retExt.Base;
                }
                return ret;
            }
            public static explicit operator Prop(NetLaneProps.Prop prop) {
                var ret = new Prop();
                DTOUtil.CopyAllMatchingFields<Prop>(ret, prop);
                ret.MetaData = prop.GetMetaData().Clone();
                return ret;
            }
        }
    }
}
