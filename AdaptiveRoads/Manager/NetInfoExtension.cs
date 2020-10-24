using ColossalFramework;
using KianCommons;
using PrefabMetadata.API;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace AdaptiveRoads.Manager {
    [AttributeUsage(AttributeTargets.Struct)]
    public class FlagPairAttribute : Attribute {
        public string Name;
        public FlagPairAttribute(string name) => Name = name;
        public FlagPairAttribute() { }
    }

    public static class Extensions {
        public static Segment GetMetaData(this NetInfo.Segment segment) =>
            (segment as IInfoExtended)?.GetMetaData<Segment>();

        public static Node GetMetaData(this NetInfo.Node node) =>
            (node as IInfoExtended)?.GetMetaData<Node>();

        public static LaneProp GetMetaData(this NetLaneProps.Prop prop) =>
            (prop as IInfoExtended)?.GetMetaData<LaneProp>();

        public static Segment GetOrCreateMetaData(this NetInfo.Segment segment) {
            Assertion.Assert(segment is IInfoExtended);
            var segment2 = segment as IInfoExtended;
            var ret = segment2.GetMetaData<Segment>();
            if (ret == null) {
                ret = new Segment(segment);
                segment2.SetMetaData(ret);
            }
            return ret;
        }

        public static Node GetOrCreateMetaData(this NetInfo.Node node) {
            Assertion.Assert(node is IInfoExtended);
            var node2 = node as IInfoExtended;
            var ret = node2.GetMetaData<Node>();
            if (ret == null) {
                ret = new Node(node);
                node2.SetMetaData(ret);
            }
            return ret;
        }

        public static LaneProp GetOrCreateMetaData(this NetLaneProps.Prop prop) {
            Assertion.Assert(prop is IInfoExtended);
            var prop2 = prop as IInfoExtended;
            var ret = prop2.GetMetaData<LaneProp>();
            if (ret == null) {
                ret = new LaneProp(prop);
                prop2.SetMetaData(ret);
            }
            return ret;
        }

        public static bool IsAdaptive(this NetInfo info) => NetInfoExtionsion.IsAdaptive(info);

        public static IEnumerable<NetInfo> AllElevations(this NetInfo ground) =>
            NetInfoExtionsion.AllElevations(ground);

        public static bool CheckRange(this Range range, float value) => range?.InRange(value) ?? true;

        public static bool CheckFlags(this NetInfo.Segment segmentInfo, NetSegment.Flags flags, bool turnAround) {
            if (!turnAround)
                return flags.CheckFlags(segmentInfo.m_forwardRequired, segmentInfo.m_forwardForbidden);
            else
                return flags.CheckFlags(segmentInfo.m_backwardRequired, segmentInfo.m_backwardForbidden);
        }

        public static void ApplyVanillaForbidden(this NetInfo info) => NetInfoExtionsion.ApplyVanillaForbiddden(info);
        public static void RollBackVanillaForbidden(this NetInfo info) => NetInfoExtionsion.RollBackVanillaForbidden(info);
    }

    [Serializable]
    public static class NetInfoExtionsion {
        #region value types
        [Serializable]
        public class Range {
            public float Lower, Upper;
            public bool InRange(float value) => Lower <= value && value < Upper;
            public override string ToString() => $"[{Lower}:{Upper})";
        }

        [FlagPair]
        [Serializable]
        public struct VanillaSegmentInfoFlags {
            [BitMask]
            public NetSegment.Flags Required, Forbidden;
            public bool CheckFlags(NetSegment.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        public struct SegmentInfoFlags {
            [BitMask]
            public NetSegmentExt.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        public struct SegmentEndInfoFlags {
            [BitMask]
            public NetSegmentEnd.Flags Required, Forbidden;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        public struct NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        public struct LaneInfoFlags {
            [BitMask]
            public NetLaneExt.Flags Required, Forbidden;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }
        #endregion

        #region sub prefab extensions
        [Serializable]
        public class Segment : ICloneable {
            object ICloneable.Clone() => Clone();

            [Serializable]
            public class FlagsT {
                [CustomizableProperty("Extension")]
                public SegmentInfoFlags Flags;

                //[CustomizableProperty("Segment Start")]
                public SegmentEndInfoFlags Start;

                //[CustomizableProperty("Segment End")]
                public SegmentEndInfoFlags End;

                public bool CheckFlags(
                    NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags) {
                    return
                        Flags.CheckFlags(flags) &
                        Start.CheckFlags(startFlags) &
                        End.CheckFlags(endFlags);
                }


                public FlagsT Clone() => this.ShalowClone();
            }

            public FlagsT ForwardFlags = new FlagsT();
            public FlagsT BackwardFlags = new FlagsT();

            public bool CheckFlags(NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags,
                    bool turnAround) {
                if (!turnAround)
                    return ForwardFlags.CheckFlags(flags, startFlags, endFlags);
                else
                    return BackwardFlags.CheckFlags(flags, startFlags, endFlags);
            }

            private Segment() { }
            public Segment(NetInfo.Segment template) : this() { }

            public Segment Clone() {
                var clone = new Segment();
                clone.ForwardFlags = ForwardFlags.Clone();
                clone.BackwardFlags = BackwardFlags.Clone();
                return clone;
            }
        }

        [Serializable]
        public class Node : ICloneable {
            object ICloneable.Clone() => Clone();

            public NodeInfoFlags NodeFlags;

            [CustomizableProperty("Segment End")]
            public SegmentEndInfoFlags SegmentEndFlags;

            [CustomizableProperty("Segment")]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Extension")]
            public SegmentInfoFlags SegmentFlags;

            public bool CheckFlags(
                NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags,
                NetSegmentExt.Flags segmentFlags, NetSegment.Flags vanillaSegmentFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags);

            private Node() { }
            public Node(NetInfo.Node template) { }

            /// <summary>clone</summary>
            public Node Clone() {
                var clone = new Node();
                clone.NodeFlags = this.NodeFlags;
                clone.SegmentEndFlags = this.SegmentEndFlags;
                return clone;
            }
        }

        [Serializable]
        public class LaneProp : ICloneable {
            object ICloneable.Clone() => Clone();

            [CustomizableProperty("Lane")]
            public LaneInfoFlags LaneFlags = new LaneInfoFlags();

            //[CustomizableProperty("SegmentExt")]
            public SegmentInfoFlags SegmentFlags = new SegmentInfoFlags();

            [CustomizableProperty("Segment")]
            public VanillaSegmentInfoFlags VanillaSegmentFlags = new VanillaSegmentInfoFlags();

            [CustomizableProperty("Segment Start")]
            public SegmentEndInfoFlags SegmentStartFlags = new SegmentEndInfoFlags();

            [CustomizableProperty("Segment End")]
            public SegmentEndInfoFlags SegmentEndFlags = new SegmentEndInfoFlags();

            //[CustomizableProperty("Start Node")]
            public NodeInfoFlags StartNodeFlags = new NodeInfoFlags();

            //[CustomizableProperty("End Node")]
            public NodeInfoFlags EndNodeFlags = new NodeInfoFlags();

            [CustomizableProperty("Lane Speed Limit Range")]
            public Range SpeedLimit; // null => N/A

            [CustomizableProperty("Average Speed Limit Range")]
            public Range AverageSpeedLimit; // null => N/A

            /// <param name="laneSpeed">game speed</param>
            /// <param name="averageSpeed">game speed</param>
            public bool Check(
                NetLaneExt.Flags laneFlags,
                NetSegmentExt.Flags segmentFlags,
                NetSegment.Flags vanillaSegmentFlags,
                NetNodeExt.Flags startNodeFlags, NetNodeExt.Flags endNodeFlags,
                NetSegmentEnd.Flags segmentStartFlags, NetSegmentEnd.Flags segmentEndFlags,
                float laneSpeed, float averageSpeed) =>
                LaneFlags.CheckFlags(laneFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) &&
                VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags) &&
                SegmentStartFlags.CheckFlags(segmentStartFlags) &&
                SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                StartNodeFlags.CheckFlags(startNodeFlags) &&
                EndNodeFlags.CheckFlags(endNodeFlags) &&
                SpeedLimit.CheckRange(laneSpeed) &&
                AverageSpeedLimit.CheckRange(averageSpeed);

            public LaneProp(NetLaneProps.Prop template) { }
            private LaneProp() { }
            public LaneProp Clone() {
                var clone = new LaneProp();
                clone.LaneFlags = LaneFlags;
                clone.SegmentFlags = SegmentFlags;
                clone.SegmentStartFlags = SegmentStartFlags;
                clone.SegmentEndFlags = SegmentEndFlags;
                clone.StartNodeFlags = StartNodeFlags;
                clone.EndNodeFlags = EndNodeFlags;
                return clone;
            }
        }

        #endregion

        #region static
        private static IEnumerable<NetLaneProps.Prop> IterateProps(NetInfo.Lane lane)
            => lane?.m_laneProps?.m_props ?? Enumerable.Empty<NetLaneProps.Prop>();

        private static IEnumerable<NetLaneProps.Prop> IterateProps(NetInfo info) {
            foreach (var lane in info?.m_lanes ?? Enumerable.Empty<NetInfo.Lane>()) {
                foreach (var prop in IterateProps(lane))
                    yield return prop;
            }
        }

        public static bool IsAdaptive(NetInfo info) {
            Assertion.AssertNotNull(info);
            foreach (var item in info.m_nodes) {
                if (item.GetMetaData() != null)
                    return true;
            }
            foreach (var item in info.m_segments) {
                if (item.GetMetaData() != null)
                    return true;
            }
            foreach (var item in IterateProps(info)) {
                if (item.GetMetaData() != null)
                    return true;
            }
            return false;
        }

        public static NetInfo EditedNetInfo =>
            ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;

        public static IEnumerable<NetInfo> EditedNetInfos =>
            AllElevations(EditedNetInfo);

        /// <summary>
        /// consistent with NetInfo.GetInex()
        /// </summary>
        public static int NetInfoCount => PrefabCollection<NetInfo>.PrefabCount();

        public static IEnumerable<NetInfo> AllElevations(NetInfo ground) {
            if (ground == null) yield break;

            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

            yield return ground;
            if (elevated != null) yield return elevated;
            if (bridge != null) yield return bridge;
            if (slope != null) yield return slope;
            if (tunnel != null) yield return tunnel;
        }


        public static void ApplyVanillaForbiddden(NetInfo info) {
            foreach (var node in info.m_nodes) {
                if (node.GetMetaData() is Node metadata) {
                    bool vanillaForbidden = metadata.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    if (vanillaForbidden)
                        node.m_flagsRequired |= NetNode.Flags.Created & NetNode.Flags.Deleted;
                }
            }
            foreach (var segment in info.m_segments) {
                if (segment.GetMetaData() is Segment metadata) {
                    bool forwardVanillaForbidden = metadata.ForwardFlags.Flags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    bool backwardVanillaForbidden = metadata.BackwardFlags.Flags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    if (forwardVanillaForbidden)
                        segment.m_forwardRequired |= NetSegment.Flags.Created & NetSegment.Flags.Deleted;
                    if (backwardVanillaForbidden)
                        segment.m_backwardRequired |= NetSegment.Flags.Created & NetSegment.Flags.Deleted;
                }
            }
            foreach (var prop in IterateProps(info)) {
                if (prop.GetMetaData() is LaneProp metadata) {
                    bool vanillaForbidden = metadata.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.Vanilla);
                    if (vanillaForbidden)
                        prop.m_flagsRequired |= NetLane.Flags.Created & NetLane.Flags.Deleted;
                }
            }
        }

        public static void RollBackVanillaForbidden(NetInfo info) {
            foreach (var node in info.m_nodes) {
                node.m_flagsRequired &= ~(NetNode.Flags.Created & NetNode.Flags.Deleted);
            }
            foreach (var segment in info.m_segments) {
                segment.m_forwardRequired &= ~(NetSegment.Flags.Created & NetSegment.Flags.Deleted);
                segment.m_backwardRequired &= ~(NetSegment.Flags.Created & NetSegment.Flags.Deleted);
            }
            foreach (var prop in IterateProps(info)) {
                prop.m_flagsRequired &= ~(NetLane.Flags.Created & NetLane.Flags.Deleted);
            }

        }

        public static void EnsureExtended(this NetInfo netInfo) {
            try {
                Log.Debug($"EnsureExtended({netInfo}): called " + Environment.StackTrace);
                for (int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if (!(netInfo.m_nodes[i] is IInfoExtended))
                        netInfo.m_nodes[i] = netInfo.m_nodes[i].Extend() as NetInfo.Node;
                }
                for (int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if (!(netInfo.m_segments[i] is IInfoExtended))
                        netInfo.m_segments[i] = netInfo.m_segments[i].Extend() as NetInfo.Segment;

                }
                foreach (var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps?.m_props;
                    int n = props?.Length ?? 0; 
                    for (int i = 0; i < n; ++i) {
                        if (!(props[i] is IInfoExtended))
                            props[i] = props[i].Extend() as NetLaneProps.Prop;
                    }
                }
                Log.Debug($"EnsureExtended({netInfo}): successful");
            }
            catch (Exception e) {
                Log.Exception(e);
            }
        }

        public static void RollBack(this NetInfo netInfo) {
            try {
                Log.Debug($"RollBack({netInfo}): called");
                for (int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if (netInfo.m_nodes[i] is IInfoExtended<NetInfo.Node> ext)
                        netInfo.m_nodes[i] = ext.RolledBackClone();
                }
                for (int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if (netInfo.m_segments[i] is IInfoExtended<NetInfo.Segment> ext)
                        netInfo.m_segments[i] = ext.RolledBackClone();
                }
                foreach (var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps.m_props;
                    int n = props?.Length ?? 0;
                    for (int i = 0; i < n; ++i) {
                        if (props[i] is IInfoExtended<NetLaneProps.Prop> ext)
                            props[i] = ext.RolledBackClone();
                    }
                }
            }
            catch (Exception e) {
                Log.Exception(e);
            }
        }

        public static void EnsureEditedNetInfosExtended() {
            Log.Debug($"EnsureEditedNetInfosExtended() was called");
            foreach (var info in EditedNetInfos)
                EnsureExtended(info);
            Log.Debug($"EnsureEditedNetInfosExtended() was successful");
        }

        public static void RollBackEditedNetInfos() {
            Log.Debug($"EnsureEditedNetInfosExtended() was called");
            foreach (var info in EditedNetInfos)
                RollBack(info);
            Log.Debug($"RollBackEditedNetInfos() was successful");
        }
        #endregion
    }
}
