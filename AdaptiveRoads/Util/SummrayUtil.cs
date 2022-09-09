namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SummrayUtil {
        public static string MergeFlagText(params IConvertible[] flags) {
            string ret = "";
            foreach (IConvertible item in flags) {
                try {
                    if (item is null || item.ToInt64() == 0)
                        continue;
                    if (ret != "") ret += ", ";
                    ret += item.ToString();
                } catch (Exception ex) {
                    throw new Exception(
                        $"Bad argument type: {(item?.GetType()).ToSTR()}",
                        ex);
                }
            }
            return ret;
        }

        public static string MergeText(params string[] texts) {
            return texts.Where(s => !s.IsNullorEmpty()).Join(", ") + "";
        }


        #region prop
        public static string DisplayName(this NetLaneProps.Prop prop) {
            if (prop.m_prop != null) {
                return prop.m_prop.name;
            } else if (prop.m_tree != null) {
                return prop.m_tree.name;
            } else {
                return "New prop";
            }
        }

        public static string Summary(this IEnumerable<NetLaneProps.Prop> props) {
            return props.Select(p => p.Summary()).JoinLines();
        }

        public static string Summary(this NetLaneProps.Prop prop) {
            return Summary(prop, prop.GetMetaData(), prop.DisplayName());
        }

        public static string Summary(
            NetLaneProps.Prop prop,
            NetInfoExtionsion.LaneProp propExt) {
            return Summary(prop, propExt, prop.DisplayName());
        }

        public static string Summary(
            NetLaneProps.Prop prop,
            NetInfoExtionsion.LaneProp propExt,
            string name) {
            string ret = name ?? "New prop";

            string text1;
            {
                var t = MergeFlagText(
                    prop.m_flagsRequired,
                    propExt?.LaneFlags.Required,
                    propExt?.VanillaSegmentFlags.Required,
                    propExt?.SegmentFlags.Required);
                var tStart = MergeFlagText(
                    prop.m_startFlagsRequired,
                    propExt?.StartNodeFlags.Required,
                    propExt?.SegmentStartFlags.Required);
                if (!string.IsNullOrEmpty(tStart))
                    tStart = " Tail:" + tStart;
                var tEnd = MergeFlagText(
                    prop.m_endFlagsRequired,
                    propExt?.EndNodeFlags.Required,
                    propExt?.SegmentEndFlags.Required);
                if (!string.IsNullOrEmpty(tEnd))
                    tEnd = " Head:" + tEnd;
                text1 = t + tStart + tEnd;
            }
            string text2;
            {
                var t = MergeFlagText(
                    prop.m_flagsForbidden,
                    propExt?.LaneFlags.Forbidden,
                    propExt?.VanillaSegmentFlags.Forbidden,
                    propExt?.SegmentFlags.Forbidden);
                var tStart = MergeFlagText(
                    prop.m_startFlagsForbidden,
                    propExt?.StartNodeFlags.Forbidden,
                    propExt?.SegmentStartFlags.Forbidden);
                if (!string.IsNullOrEmpty(tStart))
                    tStart = " Tail:" + tStart;
                var tEnd = MergeFlagText(
                    prop.m_endFlagsForbidden,
                    propExt?.EndNodeFlags.Forbidden,
                    propExt?.SegmentEndFlags.Forbidden);
                if (!string.IsNullOrEmpty(tEnd))
                    tEnd = " Head:" + tEnd;
                text2 = t + tStart + tEnd;
            }

            if (!string.IsNullOrEmpty(text1))
                ret += "\n  Required:" + text1;
            if (!string.IsNullOrEmpty(text2))
                ret += "\n  Forbidden:" + text2;
            return ret;
        }
        #endregion

        #region node
        public static string DisplayName(this NetInfo.Node node) {
            string ret = node?.GetMetaData()?.Title;
            if (ret.IsNullorEmpty())
                ret = node?.m_mesh?.name;
            return ret;
        }

        public static string Summary(this IEnumerable<NetInfo.Node> nodes) {
            return nodes.Select(_item => _item.Summary()).JoinLines();
        }

        public static string Summary(this NetInfo.Node node) {
            return Summary(node, node.GetMetaData(), node.DisplayName());
        }

        public static string Summary(
            NetInfo.Node node,
            NetInfoExtionsion.Node propExt) {
            return Summary(node, propExt, node.DisplayName());
        }

        public static string Summary(
            NetInfo.Node node,
            NetInfoExtionsion.Node nodeExt,
            string name) {
            string ret = name ?? "New Node";

            string text1 = MergeFlagText(
                node.m_flagsRequired,
                nodeExt?.SegmentEndFlags.Required,
                nodeExt?.VanillaSegmentFlags.Required,
                nodeExt?.SegmentFlags.Required);

            string text2 = MergeFlagText(
                node.m_flagsForbidden,
                nodeExt?.SegmentEndFlags.Forbidden,
                nodeExt?.VanillaSegmentFlags.Forbidden,
                nodeExt?.SegmentFlags.Forbidden);

            string text3 = "";
            if (node.m_directConnect) {
                var tCG1 = MergeFlagText(node.m_connectGroup);
                var tCG2 = nodeExt?.CustomConnectGroups.Selected?.Join(", ");
                text3 = MergeText(tCG1, tCG2);
            }

            if (!string.IsNullOrEmpty(text1))
                ret += "\n  Required:" + text1;
            if (!string.IsNullOrEmpty(text2))
                ret += "\n  Forbidden:" + text2;
            if (!string.IsNullOrEmpty(text3))
                ret += "\n  Connect Groups:" + text3;
            return ret;
        }
        #endregion

        #region segment
        public static string DisplayName(this NetInfo.Segment segment) {
            string ret = segment?.GetMetaData()?.Title;
            if (ret.IsNullorEmpty())
                ret = segment?.m_mesh?.name;
            return ret;
        }

        public static string Summary(this IEnumerable<NetInfo.Segment> segments) {
            return segments.Select(_item => _item.Summary()).JoinLines();
        }

        public static string Summary(this NetInfo.Segment segment) {
            return Summary(segment, segment.GetMetaData(), segment.DisplayName());
        }

        public static string Summary(
            NetInfo.Segment segment,
            NetInfoExtionsion.Segment propExt) {
            return Summary(segment, propExt, segment.DisplayName());
        }

        public static string Summary(
            NetInfo.Segment segment,
            NetInfoExtionsion.Segment segmentExt,
            string name) {
            string ret = name ?? "New Segment";

            string forwardRequired = MergeFlagText(
                segment.m_forwardRequired, segmentExt?.Forward.Required);

            string forwardForbidden = MergeFlagText(
                segment.m_forwardForbidden, segmentExt?.Forward.Forbidden);

            string backwardRequired = MergeFlagText(
                segment.m_backwardRequired, segmentExt?.Backward.Required);

            string backwardForbidden = MergeFlagText(
                segment.m_backwardForbidden, segmentExt?.Backward.Forbidden);

            string headRequired = MergeFlagText(
                segmentExt?.Head.Required, segmentExt?.VanillaHeadNode.Required, segmentExt?.HeadNode.Required);

            string headForbidden = MergeFlagText(
                segmentExt?.Head.Forbidden, segmentExt?.VanillaHeadNode.Forbidden, segmentExt?.HeadNode.Forbidden);

            string tailRequired = MergeFlagText(
                segmentExt?.Tail.Required, segmentExt?.VanillaTailNode.Required, segmentExt?.TailtNode.Required);

            string tailForbidden = MergeFlagText(
                segmentExt?.Tail.Forbidden, segmentExt?.VanillaTailNode.Forbidden, segmentExt?.TailtNode.Forbidden);

            if (!string.IsNullOrEmpty(forwardRequired))
                ret += "\n  Forward Required:" + forwardRequired;
            if (!string.IsNullOrEmpty(forwardForbidden))
                ret += "\n  Forward Forbidden:" + forwardForbidden;
            if (!string.IsNullOrEmpty(backwardRequired))
                ret += "\n  Backward Required:" + backwardRequired;
            if (!string.IsNullOrEmpty(backwardForbidden))
                ret += "\n  Backward Forbidden:" + backwardForbidden;
            if (!string.IsNullOrEmpty(headRequired))
                ret += "\n  Head Required:" + headRequired;
            if (!string.IsNullOrEmpty(headForbidden))
                ret += "\n  Head Forbidden:" + headForbidden;
            if (!string.IsNullOrEmpty(tailRequired))
                ret += "\n  Tail Required:" + tailRequired;
            if (!string.IsNullOrEmpty(tailForbidden))
                ret += "\n  Tail Forbidden:" + tailForbidden;

            return ret;
        }
        #endregion
    }
}
