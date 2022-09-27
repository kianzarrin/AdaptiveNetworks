namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.UI;
    using ColossalFramework;
    using KianCommons;
    using PrefabMetadata.Helpers;
    using System;
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
        public static void SwitchFlags<T>(ref this T flags, T flag1, T flag2) where T : struct, Enum, IConvertible {
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
            if (name4.EndsWith("-") || name4.EndsWith(" "))
                name4 = name4.Substring(0, name4.Length - 1);
            if (name4.StartsWith("-") || name4.StartsWith(" "))
                name4 = name4.Substring(1, name4.Length - 1);

            if (name2 == prop.name && name3 == prop.name) {
                prop2 = prop; // right and left is the same.
                return false;
            }
            if (name2 != prop.name && name3 != prop.name) {
                prop2 = null; //confusing.
                return false;
            }
            if (name3 != prop.name)
                name2 = name3;
            else if (name4 != prop.name)
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
            if (prop.GetMetaData() is NetInfoExtionsion.LaneProp metaData) {
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
            if (unidirectional) {
                prop.m_position.x = -prop.m_position.x;
            } else {
                prop.m_segmentOffset = -prop.m_segmentOffset;
                prop.m_angle = (prop.m_angle + 180) % 360;
                prop.m_position.z = -prop.m_position.z;
            }

            prop.ChangeInvertedFlag();
            if (!unidirectional) {
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
                if (!unidirectional) {
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
            if (TryMirrorMesh(prop.m_prop, out var propInfoInverted))
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
            if (pos.x == 0)
                return;
            else if (pos.x < 0)
                pos.x -= x;
            else
                pos.x += x;
            prop.m_position = pos;
        }

        public static void CopyPropsToOtherElevations(bool clear) =>
            CopyPropsToOtherElevationsMain(clear);

        public static void CopyPropsToOtherElevations(
            bool clear, int laneIndex) =>
            CopyPropsToOtherElevationsMain(clear: clear, laneIndex: laneIndex);

        public static void CopyPropToOtherElevations(NetLaneProps.Prop prop) =>
            CopyPropsToOtherElevationsMain(clear: false, prop: prop);

        /// <summary>
        /// copy props from ground to other elevations.
        /// if prop is provided,
        ///     - clear is ignored.
        ///     - if laneIndex=-1, it will be the lane that contains the prop.
        ///     - if laneIndex>=0 , prop will be copied to all corresponding lanes of other elevations
        /// </summary>
        /// <param name="laneIndex">copy only this lane index (-1 for all lanes)</param>
        /// <param name="clear">clears target lane[s] before copying</param>
        /// <param name="prop">copy only this prop. set null to copy all props</param>
        static void CopyPropsToOtherElevationsMain(
        bool clear,
        int laneIndex = -1,
        NetLaneProps.Prop prop = null) {
            Log.Called(clear, laneIndex, Str(prop));
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
                                    CopyProp(prop, targetLane, overwrite: clear);
                                }
                            } else if (ii == laneIndex) {
                                CopyProp(prop, targetLane, overwrite: clear);
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

            foreach (var info in NetInfoExtionsion.EditedNetInfos) info.GetMetaData()?.Recalculate(info);
        }

        public static NetLaneProps.Prop CopyProp(NetLaneProps.Prop prop, NetInfo.Lane targetLane, bool overwrite) {
            if (Log.VERBOSE) Log.Called(Str(prop), Str(targetLane), overwrite);
            var prop2 = prop.Clone();
            EnumerationExtensions.AppendElement(ref targetLane.m_laneProps.m_props, prop2);
            CopyCustomFlagNames(prop, prop2, overwrite);
            return prop2;
        }

        public static void CopyProps(NetInfo.Lane srcLane, NetInfo.Lane targetLane, bool clear) {
            if (Log.VERBOSE) Log.Called($"{Str(srcLane)} -> {Str(targetLane)}", clear);
            var srcProps = srcLane.m_laneProps.m_props;
            if (clear) {
                targetLane.m_laneProps.m_props = Clone(srcProps);
            } else {
                var clonedProps = Clone(srcProps);
                var targetProps =
                    targetLane.m_laneProps.m_props
                    .Concat(clonedProps);
                targetLane.m_laneProps.m_props = targetProps.ToArray();

                CustomFlags customFlags = default;
                foreach (var prop in srcProps) customFlags |= prop.GetMetaData().UsedCustomFlags;
                NetInfo srcInfo = srcLane.GetParent(out int srcLanIndex);
                NetInfo targetInfo = targetLane.GetParent(out int targetLaneIndex);
                CopyCustomFlagNames(
                    customFlags, clear,
                    srcInfo, srcLanIndex,
                    targetInfo, targetLaneIndex);
            }
        }

        static string Str(NetLaneProps.Prop prop) {
            if (prop == null)  return "<null prop>";
            NetInfo parent = prop.GetParent(out int li, out int pi);
            return $"{parent}'.lanes[{li}].props[{pi}]";
        }
        static string Str(NetInfo.Lane lane) {
            if (lane == null) return "<null lane>";
            NetInfo parent = lane.GetParent(out int li);
            return $"{parent}'.m_lanes[{li}]";
        }

        public static void CopyCustomFlagNames(
            NetLaneProps.Prop srcProp, NetLaneProps.Prop targetProp, bool overwrite) {
            if (ModSettings.ARMode) {
                NetInfo srcInfo = srcProp.GetParent(laneIndex: out int srcLaneIndex, out _);
                NetInfo targetInfo = targetProp.GetParent(laneIndex: out int targetLaneIndex, out _);
                CopyCustomFlagNames(
                    srcProp.GetMetaData().UsedCustomFlags, overwrite,
                    srcInfo, srcLaneIndex,
                    targetInfo, targetLaneIndex);
            }
        }

        public static void CopyCustomFlagNames(
            CustomFlags customFlags, bool overwrite,
            NetInfo sourceNetnfo, int sourceLaneIndex,
            NetInfo targetNetInfo, int targetLaneIndex) {
            if(Log.VERBOSE) Log.Called(customFlags, "overwrite:" + overwrite,
                "sourceNetnfo:" + sourceNetnfo, "sourceLaneIndex:" + sourceLaneIndex,
                 "targetNetInfo:" + targetNetInfo, "targetLaneIndex:" + targetLaneIndex);
            foreach (Enum flag in customFlags.Iterate()) {
                if (flag is NetLaneExt.Flags laneFlag) {
                    if (sourceLaneIndex >= 0 && targetLaneIndex >= 0) {
                        string srcName = sourceNetnfo.GetMetaData().GetCustomLaneFlagName(laneFlag, sourceLaneIndex);
                        string targetName = targetNetInfo.GetMetaData().GetCustomLaneFlagName(laneFlag, targetLaneIndex);
                        if (!srcName.IsNullorEmpty() && (targetName.IsNullorEmpty() || overwrite)) {
                            targetNetInfo.GetMetaData().RenameCustomFlag(targetLaneIndex, laneFlag, srcName);
                        }
                    }
                } else {
                    string srcName = sourceNetnfo.GetMetaData().CustomFlagNames?.GetorDefault(flag);
                    string targetName = targetNetInfo.GetMetaData().CustomFlagNames?.GetorDefault(flag);
                    if (!srcName.IsNullorEmpty() && (targetName.IsNullorEmpty() || overwrite)) {
                        targetNetInfo.GetMetaData().RenameCustomFlag(flag, srcName);
                    }
                }
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

        public static bool LocateEditProp(this NetLaneProps.Prop prop, out NetInfo info, out NetInfo.Lane lane) {
            foreach (var info2 in NetInfoExtionsion.EditedNetInfos) {
                foreach (var lane2 in info2.m_lanes) {
                    var props = lane2?.m_laneProps?.m_props;
                    if (props == null)
                        continue;
                    if (props.Any(prop2 => prop2 == prop)) {
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
        public static bool LocateEditProp(this NetInfoExtionsion.TransitionProp prop, out NetInfo info, out NetInfoExtionsion.Track track) {
            foreach (var info2 in NetInfoExtionsion.EditedNetInfos) {
                foreach (var track2 in info2.GetMetaData().Tracks) {
                    var props = track2?.Props;
                    if (props == null)
                        continue;
                    if (props.Any(prop2 => prop2 == prop)) {
                        track = track2;
                        info = info2;
                        return true;
                    }
                }
            }
            track = null;
            info = null;
            return false;
        }
    }
}
