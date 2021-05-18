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
            flags.SwitchFlags(NetLane.Flags.YieldStart, NetLane.Flags.YieldEnd);
            flags.SwitchFlags(NetLane.Flags.StartOneWayRight, NetLane.Flags.EndOneWayRight);
            flags.SwitchFlags(NetLane.Flags.StartOneWayLeft, NetLane.Flags.EndOneWayLeft);
        }

        public static void InvertLeftRight(ref this NetLane.Flags flags) {
            flags.SwitchFlags(NetLane.Flags.StartOneWayRight, NetLane.Flags.StartOneWayLeft);
            flags.SwitchFlags(NetLane.Flags.EndOneWayRight, NetLane.Flags.EndOneWayLeft);
        }
        public static void InvertLeftRight(ref this NetSegmentEnd.Flags flags) {
            flags.SwitchFlags(NetSegmentEnd.Flags.HasRightSegment, NetSegmentEnd.Flags.HasLeftSegment);
            flags.SwitchFlags(NetSegmentEnd.Flags.CanTurnRight, NetSegmentEnd.Flags.CanTurnLeft);
        }
        public static void InvertLeftRight(ref this NetSegmentExt.Flags flags) {
            flags.SwitchFlags(NetSegmentExt.Flags.ParkingAllowedLeft, NetSegmentExt.Flags.ParkingAllowedRight);
        }
        public static void InvertLeftRight(ref this NetSegment.Flags flags) {
            flags.SwitchFlags(NetSegment.Flags.StopLeft, NetSegment.Flags.StopRight);
            flags.SwitchFlags(NetSegment.Flags.StopLeft2, NetSegment.Flags.StopRight2);
        }
        public static NetLaneProps.ColorMode InvertStartEnd(this NetLaneProps.ColorMode colorMode) {
            return colorMode switch {
                NetLaneProps.ColorMode.StartState => NetLaneProps.ColorMode.EndState,
                NetLaneProps.ColorMode.EndState => NetLaneProps.ColorMode.StartState,
                _ => colorMode,
            };
        }
        public static void SwitchFlags<T>(ref this T flags, T flag1, T flag2) where T : struct, Enum, IConvertible{
            bool hasFlag1 = flags.IsFlagSet(flag1);
            bool hasFlag2 = flags.IsFlagSet(flag2);
            flags = flags.SetFlags(flag1, hasFlag2);
            flags = flags.SetFlags(flag2, hasFlag1);
        }
        public static bool TryMirrorMesh(PropInfo prop, out PropInfo prop2) {
            string name2 = prop.name.Replace("left", "right").Replace("Left", "Right")
                .Replace("LEFT", "RIGHT").Replace("LHT", "RHT").Replace("lht", "rht");
            string name3 = prop.name.Replace("right", "left").Replace("Right", "Left")
                .Replace("RIGHT", "LEFT").Replace("RHT", "LHT").Replace("rht", "lht");
            string name4 = prop.name.Remove("Mirror").Remove("mirror")
                .Remove("Mirrored").Remove("mirrored");
            if(name4.EndsWith("-") || name4.EndsWith(" "))
                name4 = name4.Substring(0, name4.Length - 1);
            if(name4.StartsWith("-") || name4.StartsWith(" "))
                name4 = name4.Substring(1, name4.Length - 1);

            if(name2 == prop.name && name3 == prop.name) {
                prop2 = prop; // right and left is the same.
                return false;
            }
            if(name2 != prop.name && name3 != prop.name) {
                prop2 = null; //confusing.
                return false;
            }
            if(name3 != prop.name)
                name2 = name3;
            else if(name4 != prop.name)
                name2 = name4;

            prop2 = PrefabCollection<PropInfo>.FindLoaded(name2);
            return prop2 != null;
        }


        public static void ChangeInvertedFlag(this NetLaneProps.Prop prop) {
            {
                bool InvertRequired = prop.m_flagsRequired.IsFlagSet(NetLane.Flags.Inverted);
                bool InvertForbidden = prop.m_flagsForbidden.IsFlagSet(NetLane.Flags.Inverted);
                prop.m_flagsRequired = prop.m_flagsRequired.SetFlags(
                    NetLane.Flags.Inverted,
                    InvertForbidden);
                prop.m_flagsForbidden = prop.m_flagsForbidden.SetFlags(
                    NetLane.Flags.Inverted,
                    InvertRequired);
            }
            if(prop.GetMetaData() is NetInfoExtionsion.LaneProp metaData)
            {
                bool InvertRequired = metaData.SegmentFlags.Required.IsFlagSet(NetSegmentExt.Flags.LeftHandTraffic);
                bool InvertForbidden = metaData.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.LeftHandTraffic);
                metaData.SegmentFlags.Required = metaData.SegmentFlags.Required.SetFlags(
                    NetSegmentExt.Flags.LeftHandTraffic,
                    InvertForbidden);
                metaData.SegmentFlags.Forbidden = metaData.SegmentFlags.Forbidden.SetFlags(
                    NetSegmentExt.Flags.LeftHandTraffic,
                    InvertRequired);
            }
        }

        public static void ToggleRHT_LHT(this NetLaneProps.Prop prop, bool unidirectional) {
            Log.Debug("ToggleRHT_LHT() called for " + prop.m_prop.name);
            if(unidirectional) {
                prop.m_position.x = -prop.m_position.x;
            } else {
                prop.m_segmentOffset = -prop.m_segmentOffset;
                prop.m_angle = (prop.m_angle + 180) % 360;
                prop.m_position.z = -prop.m_position.z;
            }

            prop.ChangeInvertedFlag();
            if(!unidirectional) {
                prop.m_colorMode = prop.m_colorMode.InvertStartEnd();
                prop.m_flagsRequired.InvertStartEnd();
                prop.m_flagsForbidden.InvertStartEnd();
                Helpers.Swap(ref prop.m_startFlagsRequired, ref prop.m_endFlagsRequired);
                Helpers.Swap(ref prop.m_startFlagsForbidden, ref prop.m_endFlagsForbidden);
            }
            prop.m_flagsRequired.InvertLeftRight();
            prop.m_flagsForbidden.InvertLeftRight();

            var propExt = prop.GetMetaData();
            if (propExt != null) {
                if(!unidirectional) {
                    Helpers.Swap(ref propExt.StartNodeFlags, ref propExt.EndNodeFlags);
                    Helpers.Swap(ref propExt.SegmentStartFlags, ref propExt.SegmentEndFlags);
                }
                propExt.SegmentStartFlags.Required.InvertLeftRight();
                propExt.SegmentStartFlags.Forbidden.InvertLeftRight();
                propExt.SegmentEndFlags.Required.InvertLeftRight();
                propExt.SegmentEndFlags.Forbidden.InvertLeftRight();
                propExt.SegmentFlags.Required.InvertLeftRight();
                propExt.SegmentFlags.Forbidden.InvertLeftRight();
                propExt.VanillaSegmentFlags.Required.InvertLeftRight();
                propExt.VanillaSegmentFlags.Forbidden.InvertLeftRight();

                Helpers.Swap(ref propExt.ForwardSpeedLimit, ref propExt.BackwardSpeedLimit);
            }
            if(TryMirrorMesh(prop.m_prop, out var propInfoInverted))
                prop.m_prop = prop.m_finalProp = propInfoInverted;
        }

        public static void ToggleForwardBackward(this NetLaneProps.Prop prop) {
            Log.Debug("ToggleForwardBackward() called for " + prop.m_prop.name);

            prop.m_segmentOffset = -prop.m_segmentOffset;
            prop.m_angle = (prop.m_angle + 180) % 360;
            prop.m_position.z = -prop.m_position.z;
            prop.m_position.x = -prop.m_position.x;

            prop.m_colorMode = prop.m_colorMode.InvertStartEnd();
            prop.m_flagsRequired.InvertStartEnd();
            prop.m_flagsForbidden.InvertStartEnd();

            Helpers.Swap(ref prop.m_startFlagsRequired, ref prop.m_endFlagsRequired);
            Helpers.Swap(ref prop.m_startFlagsForbidden, ref prop.m_endFlagsForbidden);

            var propExt = prop.GetMetaData();
            if (propExt != null) {
                Helpers.Swap(ref propExt.StartNodeFlags, ref propExt.EndNodeFlags);
                Helpers.Swap(ref propExt.SegmentStartFlags, ref propExt.SegmentEndFlags);

                // change parking/bus/tram left/right flags.
                propExt.SegmentFlags.Required.InvertLeftRight();
                propExt.SegmentFlags.Forbidden.InvertLeftRight();
                propExt.VanillaSegmentFlags.Required.InvertLeftRight();
                propExt.VanillaSegmentFlags.Forbidden.InvertLeftRight();

                Helpers.Swap(ref propExt.ForwardSpeedLimit, ref propExt.BackwardSpeedLimit);
            }
        }

        public static bool CanInvert(this NetLaneProps.Prop prop) =>
            (prop.m_flagsRequired ^ prop.m_flagsForbidden)
            .IsFlagSet(NetLane.Flags.Inverted);

        public static void Displace(this NetLaneProps.Prop prop, float x) {
            var pos = prop.m_position;
            if(pos.x == 0)
                return;
            else if(pos.x < 0)
                pos.x -= x;
            else
                pos.x += x;
            prop.m_position = pos;
        }

        public static void CopyPropsToOtherElevations(bool clear = true) =>
            CopyPropsToOtherElevationsMain(clear);

        public static void CopyPropsToOtherElevations(
            bool clear, int laneIndex) =>
            CopyPropsToOtherElevationsMain(clear: clear, laneIndex: laneIndex);

        public static void CopyPropsToOtherElevations(NetLaneProps.Prop prop) =>
            CopyPropsToOtherElevationsMain(clear: false, prop: prop);

        /// <summary>
        /// copy props from ground to other elevations.
        /// if prop is provided,
        ///     - clear is ignored.
        ///     - if laneIndex=-1, it will be the lane that contains the prop.
        ///     - if laneIndex>=0 , prop will be copied to all coresponding lanes of other elevations
        /// </summary>
        /// <param name="laneIndex">copy only this lane index (-1 for all lanes)</param>
        /// <param name="clear">clears target lane[s] before copying</param>
        /// <param name="prop">copy only this prop. set null to copy all props</param>
        static void CopyPropsToOtherElevationsMain(
        bool clear = true,
        int laneIndex = -1,
        NetLaneProps.Prop prop = null) {
            var srcInfo = NetInfoExtionsion.EditedNetInfo;
            var srcLanes = srcInfo.m_lanes;
            foreach (var targetInfo in NetInfoExtionsion.EditedNetInfos.Skip(1)) {
                var targetLanes = targetInfo.m_lanes;
                for (int i = 0, j = 0; i < srcLanes.Length && j < targetLanes.Length;) {
                    int ii = srcInfo.m_sortedLanes[i];
                    int jj = targetInfo.m_sortedLanes[j];
                    var srcLane = srcLanes[ii];
                    var targetLane = targetLanes[jj];

                    if (srcLane.m_laneType == targetLane.m_laneType) {
                        if (prop != null) {
                            if (laneIndex < 0) {
                                if (srcLane.m_laneProps.m_props.ContainsRef(prop)) {
                                    AddProp(prop, targetLane);
                                }
                            } else {
                                if (ii == laneIndex) AddProp(prop, targetLane);
                            }
                        } else if (ii == laneIndex || laneIndex < 0) {
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

        public static bool LocateEditProp(this NetLaneProps.Prop  prop, out NetInfo info, out NetInfo.Lane lane) {
            foreach(var info2 in NetInfoExtionsion.EditedNetInfos) {
                foreach(var lane2 in info2.m_lanes) {
                    var props = lane2?.m_laneProps?.m_props;
                    if(props == null)
                        continue;
                    if(props.Any(prop2 => prop2 == prop)) {
                        lane = lane2;
                        info = info2;
                        return true;
                    }
                }
            }
            lane = null;
            info = null;
            return false;
        }
    }
}
