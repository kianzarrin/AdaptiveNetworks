namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data.NetworkExtensions;
    using ColossalFramework;
    using KianCommons;
    using KianCommons.Math;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static AdaptiveRoads.UI.ModSettings;
    using static KianCommons.ReflectionHelpers;

    public static partial class NetInfoExtionsion {
        #region static
        private static IEnumerable<NetLaneProps.Prop> IterateProps(NetInfo.Lane lane)
        => lane?.m_laneProps?.m_props ?? Enumerable.Empty<NetLaneProps.Prop>();

        private static IEnumerable<NetLaneProps.Prop> IterateProps(NetInfo info) {
            foreach(var lane in info?.m_lanes ?? Enumerable.Empty<NetInfo.Lane>()) {
                foreach(var prop in IterateProps(lane))
                    yield return prop;
            }
        }

        public static bool IsAdaptive(this NetInfo info) {
            return info?.GetMetaData() != null;
            //Assertion.AssertNotNull(info);
            //if(info.GetMetaData() != null)
            //    return true;
            //foreach(var item in info.m_nodes) {
            //    if(item.GetMetaData() != null)
            //        return true;
            //}
            //foreach(var item in info.m_segments) {
            //    if(item.GetMetaData() != null)
            //        return true;
            //}
            //foreach(var item in IterateProps(info)) {
            //    if(item.GetMetaData() != null)
            //        return true;
            //}
            //return false;
        }

        public static NetInfo EditedNetInfo =>
            ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;

        public static IEnumerable<NetInfo> EditedNetInfos =>
            AllElevations(EditedNetInfo);

        public static IEnumerable<NetInfo> AllElevations(this NetInfo ground) {
            if(ground == null) yield break;

            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

            yield return ground;
            if(elevated != null) yield return elevated;
            if(bridge != null) yield return bridge;
            if(slope != null) yield return slope;
            if(tunnel != null) yield return tunnel;
        }

        public static void InvokeEditPrefabChanged() {
            // invoke eventEditPrefabChanged
            SimulationManager.instance.m_ThreadingWrapper.QueueMainThread(delegate () {
                var tc = ToolsModifierControl.toolController;
                ReflectionHelpers.EventToDelegate<ToolController.EditPrefabChanged>(
                    tc, nameof(tc.eventEditPrefabChanged))
                    ?.Invoke(tc.m_editPrefabInfo);
            });
        }


        const NetNode.Flags vanillaNode = NetNode.Flags.Sewage | NetNode.Flags.Deleted;
        const NetSegment.Flags vanillaSegment = NetSegment.Flags.AccessFailed | NetSegment.Flags.Deleted;
        const NetLane.Flags vanillaLane = NetLane.Flags.Created | NetLane.Flags.Deleted;

        public static void ApplyVanillaForbidden(this NetInfo info) {
            foreach(var node in info.m_nodes) {
                if(node.GetMetaData() is Node metadata) {
                    bool vanillaForbidden = metadata.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.NodeFlags.Forbidden.IsFlagSet(NetNodeExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.SegmentEndFlags.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    if(vanillaForbidden)
                        node.m_flagsRequired |= vanillaNode;
                }
            }
            foreach(var segment in info.m_segments) {
                if(segment.GetMetaData() is Segment metadata) {
                    bool forwardVanillaForbidden = metadata.Forward.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    bool backwardVanillaForbidden = metadata.Backward.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    bool vanillaForbidden = metadata.Head.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    vanillaForbidden |= metadata.Tail.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    forwardVanillaForbidden |= vanillaForbidden;
                    backwardVanillaForbidden |= vanillaForbidden;

                    if(forwardVanillaForbidden)
                        segment.m_forwardRequired |= vanillaSegment;
                    if(backwardVanillaForbidden)
                        segment.m_backwardRequired |= vanillaSegment;
                }
            }
            foreach(var prop in IterateProps(info)) {
                if(prop.GetMetaData() is LaneProp metadata) {
                    bool vanillaForbidden = metadata.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.LaneFlags.Forbidden.IsFlagSet(NetLaneExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.StartNodeFlags.Forbidden.IsFlagSet(NetNodeExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.EndNodeFlags.Forbidden.IsFlagSet(NetNodeExt.Flags.Vanilla);
                    vanillaForbidden |= metadata.SegmentStartFlags.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);
                    vanillaForbidden |= metadata.SegmentEndFlags.Forbidden.IsFlagSet(NetSegmentEnd.Flags.Vanilla);

                    if(vanillaForbidden)
                        prop.m_flagsRequired |= vanillaLane;
                }
            }
        }

        public static void UndoVanillaForbidden(this NetInfo info) {
            foreach(var node in info.m_nodes) {
                if(node.m_flagsRequired.CheckFlags(vanillaNode))
                    node.m_flagsRequired &= ~vanillaNode;
            }
            foreach(var segment in info.m_segments) {
                if(segment.m_forwardRequired.CheckFlags(vanillaSegment))
                    segment.m_forwardRequired &= ~vanillaSegment;
                if(segment.m_backwardRequired.CheckFlags(vanillaSegment))
                    segment.m_backwardRequired &= ~vanillaSegment;
            }
            foreach(var prop in IterateProps(info)) {
                if(prop.m_flagsRequired.CheckFlags(vanillaLane))
                    prop.m_flagsRequired &= ~vanillaLane;
            }
        }

        public static void EnsureExtended(this NetInfo netInfo) {
            try {
                Assertion.Assert(netInfo);
                Log.Debug($"EnsureExtended({netInfo}): called "/* + Environment.StackTrace*/);
                for(int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if(!(netInfo.m_nodes[i] is IInfoExtended))
                        netInfo.m_nodes[i] = netInfo.m_nodes[i].Extend() as NetInfo.Node;
                }
                for(int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if(!(netInfo.m_segments[i] is IInfoExtended))
                        netInfo.m_segments[i] = netInfo.m_segments[i].Extend() as NetInfo.Segment;
                }
                foreach(var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    int n = props?.Length ?? 0;
                    for(int i = 0; i < n; ++i) {
                        if(!(props[i] is IInfoExtended))
                            props[i] = props[i].Extend() as NetLaneProps.Prop;
                    }
                }
                netInfo.GetOrCreateMetaData();

                Log.Debug($"EnsureExtended({netInfo}): successful");
            } catch(Exception e) {
                Log.Exception(e);
            }
        }

        public static void UndoExtend(this NetInfo netInfo) {
            try {
                Log.Debug($"UndoExtend({netInfo}): called");
                for(int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if(netInfo.m_nodes[i] is IInfoExtended<NetInfo.Node> ext)
                        netInfo.m_nodes[i] = ext.UndoExtend();
                }
                for(int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if(netInfo.m_segments[i] is IInfoExtended<NetInfo.Segment> ext)
                        netInfo.m_segments[i] = ext.UndoExtend();
                }
                foreach(var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps.m_props;
                    int n = props?.Length ?? 0;
                    for(int i = 0; i < n; ++i) {
                        if(props[i] is IInfoExtended<NetLaneProps.Prop> ext)
                            props[i] = ext.UndoExtend();
                    }
                }
                netInfo.RemoveMetadataContainer();

            } catch(Exception e) {
                Log.Exception(e);
            }
        }

        public static void Ensure_EditedNetInfos() {
            LogCalled();
            if(VanillaMode) {
                UndoExtend_EditedNetInfos();
            } else {
                EnsureExtended_EditedNetInfos();
            }
        }

        public static void EnsureExtended_EditedNetInfos() {
            if(VanillaMode) {
                Log.Debug($"EnsureExtended_EditedNetInfos() because we are in vanilla mode");
                return;
            }
            Log.Debug($"EnsureExtended_EditedNetInfos() was called");
            foreach(var info in EditedNetInfos)
                EnsureExtended(info);
            Log.Debug($"EnsureExtended_EditedNetInfos() was successful");
        }

        public static void UndoExtend_EditedNetInfos() {
            Log.Debug($"UndoExtend_EditedNetInfos() was called");
            foreach(var info in EditedNetInfos)
                UndoExtend(info);
            Verify_UndoExtend();
            Log.Debug($"UndoExtend_EditedNetInfos() was successful");
        }

        public static void Verify_UndoExtend() {
            //test if UndoExtend_EditedNetInfos was successful.
            foreach(var netInfo in NetInfoExtionsion.EditedNetInfos) {
                for(int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if(!(netInfo.m_nodes[i].GetType() == typeof(NetInfo.Node)))
                        throw new Exception($"reversal unsuccessful. nodes[{i}]={netInfo.m_nodes[i]}");
                }
                for(int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if(!(netInfo.m_segments[i].GetType() == typeof(NetInfo.Segment)))
                        throw new Exception($"reversal unsuccessful. segments[{i}]={netInfo.m_segments[i]}");
                }
                foreach(var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    int n = props?.Length ?? 0;
                    for(int i = 0; i < n; ++i) {
                        if(!(props[i].GetType() == typeof(NetLaneProps.Prop)))
                            throw new Exception($"reversal unsuccessful. props[{i}]={props[i]}");
                    }
                }
            }
        }
        #endregion
    }
}
