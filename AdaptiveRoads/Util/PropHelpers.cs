namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static KianCommons.EnumerationExtensions;

    internal static class PropHelpers {
        public static void InvertStartEnd(ref this NetLane.Flags flags) {
            var copy = flags;
            flags = flags.SetFlags(
                NetLane.Flags.YieldStart,
                copy.IsFlagSet(NetLane.Flags.YieldEnd));
            flags = flags.SetFlags(
                NetLane.Flags.YieldEnd,
                copy.IsFlagSet(NetLane.Flags.YieldStart));
            flags = flags.SetFlags(
                NetLane.Flags.StartOneWayRight,
                copy.IsFlagSet(NetLane.Flags.StartOneWayLeft));
            flags = flags.SetFlags(
                NetLane.Flags.StartOneWayLeft,
                copy.IsFlagSet(NetLane.Flags.StartOneWayRight));
            flags = flags.SetFlags(
                NetLane.Flags.EndOneWayRight,
                copy.IsFlagSet(NetLane.Flags.EndOneWayLeft));
            flags = flags.SetFlags(
                NetLane.Flags.EndOneWayLeft,
                copy.IsFlagSet(NetLane.Flags.EndOneWayRight));
        }
        public static void InvertLeftRight(ref this NetLane.Flags flags) {
            var copy = flags;
            flags = flags.SetFlags(
                NetLane.Flags.StartOneWayRight,
                copy.IsFlagSet(NetLane.Flags.EndOneWayRight));
            flags = flags.SetFlags(
                NetLane.Flags.EndOneWayRight,
                copy.IsFlagSet(NetLane.Flags.StartOneWayRight));
            flags = flags.SetFlags(
                NetLane.Flags.StartOneWayLeft,
                copy.IsFlagSet(NetLane.Flags.EndOneWayLeft));
            flags = flags.SetFlags(
                NetLane.Flags.EndOneWayLeft,
                copy.IsFlagSet(NetLane.Flags.StartOneWayLeft));
        }
        public static void ChangeInvertedFlag(this NetLaneProps.Prop prop) {
            bool InvertRequired = prop.m_flagsRequired.IsFlagSet(NetLane.Flags.Inverted);
            bool InvertForbidden = prop.m_flagsForbidden.IsFlagSet(NetLane.Flags.Inverted);
            prop.m_flagsRequired = prop.m_flagsRequired.SetFlags(
                NetLane.Flags.Inverted,
                InvertForbidden);
            prop.m_flagsForbidden = prop.m_flagsForbidden.SetFlags(
                NetLane.Flags.Inverted,
                InvertRequired);
        }

        public static void ToggleRHT_LHT(this NetLaneProps.Prop prop) {
            Log.Debug("ToggleRHT_LHT() called for " + prop.m_prop.name);
            prop.m_segmentOffset = -prop.m_segmentOffset;
            prop.m_angle = (prop.m_angle + 180) % 360;
            prop.m_position.z = -prop.m_position.z;

            prop.ChangeInvertedFlag();
            prop.m_flagsRequired.InvertStartEnd();
            prop.m_flagsForbidden.InvertStartEnd();
            prop.m_flagsRequired.InvertLeftRight();
            prop.m_flagsForbidden.InvertLeftRight();

            Helpers.Swap(ref prop.m_startFlagsRequired, ref prop.m_endFlagsRequired);
            Helpers.Swap(ref prop.m_startFlagsForbidden, ref prop.m_endFlagsForbidden);

            var propExt = prop.GetMetaData();
            if (propExt != null) {
                Helpers.Swap(ref propExt.StartNodeFlags, ref propExt.EndNodeFlags);
                Helpers.Swap(ref propExt.SegmentStartFlags, ref propExt.SegmentEndFlags);
            }
        }

        public static void ToggleForwardBackward(this NetLaneProps.Prop prop) {
            Log.Debug("ToggleForwardBackward() called for " + prop.m_prop.name);

            prop.m_segmentOffset = -prop.m_segmentOffset;
            prop.m_angle = (prop.m_angle + 180) % 360;
            prop.m_position.z = -prop.m_position.z;
            prop.m_position.x = -prop.m_position.x;

            prop.m_flagsRequired.InvertStartEnd();
            prop.m_flagsForbidden.InvertStartEnd();

            Helpers.Swap(ref prop.m_startFlagsRequired, ref prop.m_endFlagsRequired);
            Helpers.Swap(ref prop.m_startFlagsForbidden, ref prop.m_endFlagsForbidden);

            var propExt = prop.GetMetaData();
            if (propExt != null) {
                Helpers.Swap(ref propExt.StartNodeFlags, ref propExt.EndNodeFlags);
                Helpers.Swap(ref propExt.SegmentStartFlags, ref propExt.SegmentEndFlags);
            }
        }

        public static bool CanInvert(this NetLaneProps.Prop prop) =>
            (prop.m_flagsRequired ^ prop.m_flagsForbidden)
            .IsFlagSet(NetLane.Flags.Inverted);

        public static void CopyPropsToOtherElevations(bool clear = true) =>
            CopyPropsToOtherElevationsMain(clear);

        public static void CopyPropsToOtherElevations(
            bool clear, int laneIndex) =>
            CopyPropsToOtherElevationsMain(clear: clear, laneIndex: laneIndex);

        public static void CopyPropsToOtherElevations(NetLaneProps.Prop prop) =>
            CopyPropsToOtherElevationsMain(clear: false, prop: prop);

        public static void Displace(this NetLaneProps.Prop prop, int x) {
            var pos = prop.m_position;
            if (pos.x == 0)
                return;
            else if (pos.x < 0)
                pos.x -= x;
            else
                pos.x += x;
            prop.m_position = pos;
        }

        /// <summary>
        /// copy props from ground to other elevations.
        /// if prop is provided,
        ///     - clear is ignored.
        ///     - if laneIndex=-1, it will be the lane that contains the prop.
        ///     - if laneIndex>=0 , prop will be copied to all coresponding lanes of other elevations
        /// </summary>
        /// <param name="laneIndex">copy only this lane index (-1 for all lanes)</param>
        /// <param name="clear">clears target lane[s] before copying</param>
        /// <param name="prop">copy only this props. set null to copy all props</param>
        static void CopyPropsToOtherElevationsMain(
        bool clear = true,
        int laneIndex = -1,
        NetLaneProps.Prop prop = null) {
            var srcInfo = NetInfoExtionsion.EditedNetInfo;
            var srcLanes = srcInfo.SortedLanes().ToList();
            foreach (var targetInfo in NetInfoExtionsion.EditedNetInfos.Skip(1)) {
                var targetLanes = targetInfo.SortedLanes().ToList();
                for (int i = 0, j = 0; i < srcLanes.Count && j < targetLanes.Count;) {
                    var srcLane = srcLanes[i];
                    var targetLane = targetLanes[j];
                    if (srcLane.m_laneType == targetLane.m_laneType) {
                        if (prop != null) {
                            if (laneIndex < 0) {
                                if (srcLane.m_laneProps.m_props.ContainsRef(prop)) {
                                    AddProp(prop, targetLane);
                                }
                            } else {
                                if (i == laneIndex) AddProp(prop, targetLane);
                            }
                        } else if (i == laneIndex || laneIndex < 0) {
                            CopyProps(srcLane, targetLane, clear);
                        }
                        i++; j++;
                    } else {
                        // assuming that ground elevation has all the lanes of the other elevations.
                        i++;
                    }
                }
            }
        }

        public static void AddProp(NetLaneProps.Prop prop, NetInfo.Lane targetLane) {
            EnumerationExtensions.AppendElement(ref targetLane.m_laneProps.m_props, prop);
        }

        public static void CopyProps(NetInfo.Lane srcLane, NetInfo.Lane targetLane, bool clear) {
            var srcProps = srcLane.m_laneProps.m_props;
            if (clear) {
                targetLane.m_laneProps.m_props = Clone(srcProps);
            } else {
                var targetProps =
                    targetLane.m_laneProps.m_props
                    .Concat(Clone(srcProps));
                targetLane.m_laneProps.m_props = targetProps.ToArray();
            }
        }

        public static NetLaneProps.Prop[] Clone(this NetLaneProps.Prop[] src) =>
            src.Select(prop => prop.Clone()).ToArray();


        public static NetLaneProps.Prop Clone(this NetLaneProps.Prop prop) {
            if (prop is ICloneable cloneable)
                return cloneable.Clone() as NetLaneProps.Prop;
            else
                return prop.ShalowClone();
        }

        public static bool HasParkingLane(NetInfo info) {
            return info.m_lanes.Any(l =>
                l.m_laneType.IsFlagSet(NetInfo.LaneType.Parking));
        }

        public static bool IsParkingProp(NetLaneProps.Prop prop) {
            var propExt = prop.GetMetaData();
            var flags = propExt.SegmentFlags.Required | propExt.SegmentFlags.Forbidden;
            var parkingFlags = NetSegmentExt.Flags.ParkingAllowedBoth;
            return (flags & parkingFlags) != 0;
        }

        public static string DisplayName(this NetLaneProps.Prop prop) {
            if (prop.m_prop != null) {
                return prop.m_prop.name;
            } else if (prop.m_tree != null) {
                return prop.m_tree.name;
            } else {
                return "New prop";
            }
        }
#pragma warning disable
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
                    tStart = " Start:" + tStart;
                var tEnd = MergeFlagText(
                    prop.m_endFlagsForbidden,
                    propExt?.EndNodeFlags.Forbidden,
                    propExt?.SegmentEndFlags.Forbidden);
                if (!string.IsNullOrEmpty(tEnd))
                    tEnd = " End:" + tEnd;
                text2 = t + tStart + tEnd;
            }

            if (!string.IsNullOrEmpty(text1))
                ret += "\n  Required:" + text1;
            if (!string.IsNullOrEmpty(text2))
                ret += "\n  Forbidden:" + text2;
            return ret;
        }
#pragma warning restore

        public static string MergeFlagText(params object[] flags) {
            string ret = "";
            foreach (object item in flags) {
                try {
                    if (item is null || (int)item == 0)
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

        public static string Summary(this IEnumerable<NetLaneProps.Prop> props) {
            return props.Select(p => p.Summary()).JoinLines();
        }

    }
}
