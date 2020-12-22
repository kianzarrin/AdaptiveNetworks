using ColossalFramework;
using KianCommons;
using PrefabMetadata.API;
using PrefabMetadata.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static AdaptiveRoads.Manager.NetInfoExtionsion;
using static AdaptiveRoads.UI.ModSettings;
using KianCommons.Math;


namespace AdaptiveRoads.Manager {
    using static HintExtension;

    [AttributeUsage(AttributeTargets.Struct)]
    public class FlagPairAttribute : Attribute { }

    /// <summary>
    /// Field visibility in asset is controlled by settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionalAttribute : Attribute {
        public string Option;
        public OptionalAttribute(string option) => Option = option;
    }


    public static class Extensions {
        internal static NetInfo GetInfo(ushort index) =>
            PrefabCollection<NetInfo>.GetPrefab(index);

        internal static ushort GetIndex(this NetInfo info) =>
            MathUtil.Clamp2U16(info.m_prefabDataIndex);

        public static Segment GetMetaData(this NetInfo.Segment segment) =>
            (segment as IInfoExtended)?.GetMetaData<Segment>();

        public static Node GetMetaData(this NetInfo.Node node) =>
            (node as IInfoExtended)?.GetMetaData<Node>();

        public static LaneProp GetMetaData(this NetLaneProps.Prop prop) =>
            (prop as IInfoExtended)?.GetMetaData<LaneProp>();

        public static Net GetMetaData(this NetInfo netInfo) =>
            Net.GetAt(netInfo.GetIndex());
        
        public static Net GetOrCreateMetaData(this NetInfo netInfo) {
            Assertion.Assert(netInfo);
            var index = netInfo.GetIndex();
            return Net.GetAt(index) ?? Net.SetAt(index, new Net(netInfo))
                ?? throw new Exception("failed to create meta data for NetInfo " + netInfo);
        }

        public static void SetMeteData(this NetInfo netInfo, Net value) {
            Assertion.Assert(netInfo);
            var index = netInfo.GetIndex();
            Net.SetAt(index, value);
        }

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

        public static bool CheckRange(this Range range, float value) => range?.InRange(value) ?? true;

        public static bool CheckFlags(this NetInfo.Segment segmentInfo, NetSegment.Flags flags, bool turnAround) {
            if (!turnAround)
                return flags.CheckFlags(segmentInfo.m_forwardRequired, segmentInfo.m_forwardForbidden);
            else
                return flags.CheckFlags(segmentInfo.m_backwardRequired, segmentInfo.m_backwardForbidden);
        }
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
        public struct VanillaNodeInfoFlags {
            [BitMask]
            public NetNode.Flags Required, Forbidden;
            public bool CheckFlags(NetNode.Flags flags) => flags.CheckFlags(Required, Forbidden);
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
        [Hint("segment specific node flags")]
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
        public class Net :ICloneable{
            object ICloneable.Clone() => Clone();
            public Net() { }
            public Net(NetInfo template) : this() {
                PavementWidthRight = template.m_pavementWidth;
            }

            public Net Clone() => this.ShalowClone();

            [CustomizableProperty("Pavement Width Right", "Properties")]
            public float PavementWidthRight;

            /****************************************/
            public static Net[] Buffer;
            public static void EnsureBuffer() {
                int count = PrefabCollection<NetInfo>.PrefabCount();
                if(Buffer == null) {
                    Buffer = new Net[count];
                    Log.Debug($"Net.Buffer[{Buffer.Length}] created");
                } else if(Buffer.Length < count) {
                    var old = Buffer;
                    Buffer = new Net[count];
                    for (int i = 0; i < old.Length; ++i)
                        Buffer[i] = old[i];
                    Log.Debug($"Net.Buffer expanded from {old.Length} to {Buffer.Length}");
                }

            }
            public static Net SetAt(int index, Net net) {
                EnsureBuffer();
                return Buffer[index] = net;
            }
            public static Net GetAt(int index) {
                if (Buffer == null || index >= Buffer.Length)
                    return null;
                return Buffer[index];
            }
        }

        [Serializable]
        public class Segment : ICloneable {
            object ICloneable.Clone() => Clone();

            [Serializable]
            public class FlagsT {
                [CustomizableProperty("Extension")]
                public SegmentInfoFlags Flags;

                [CustomizableProperty("Start Node")]
                [Optional(SEGMENT_VANILLA_NODE)]
                public VanillaNodeInfoFlags VanillaStartNode;

                [CustomizableProperty("End Node")]
                [Optional(SEGMENT_VANILLA_NODE)]
                public VanillaNodeInfoFlags VanillaEndNode;

                [CustomizableProperty("Segment Start")]
                [Optional(SEGMENT_SEGMENT_END)]
                public SegmentEndInfoFlags Start;

                [CustomizableProperty("Segment End")]
                [Optional(SEGMENT_SEGMENT_END)]
                public SegmentEndInfoFlags End;

                public bool CheckFlags(
                    NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags,
                    NetNode.Flags startNodeFlags,
                    NetNode.Flags endNodeFlags) {
                    return
                        Flags.CheckFlags(flags) &
                        Start.CheckFlags(startFlags) &
                        End.CheckFlags(endFlags) &
                        VanillaStartNode.CheckFlags(startNodeFlags) &
                        VanillaEndNode.CheckFlags(endNodeFlags);
                }


                public FlagsT Clone() => this.ShalowClone();
            }

            public FlagsT ForwardFlags = new FlagsT();
            public FlagsT BackwardFlags = new FlagsT();

            public bool CheckFlags(NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags,
                    NetNode.Flags startNodeFlags,
                    NetNode.Flags endNodeFlags,
                    bool turnAround) {
                if (!turnAround)
                    return ForwardFlags.CheckFlags(flags, startFlags, endFlags, startNodeFlags, endNodeFlags);
                else
                    return BackwardFlags.CheckFlags(flags, startFlags, endFlags, startNodeFlags, endNodeFlags);
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
            [Optional(NODE_SEGMENT)]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Extension")]
            [Optional(NODE_SEGMENT)]
            public SegmentInfoFlags SegmentFlags;

            public bool CheckFlags(
                NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags,
                NetSegmentExt.Flags segmentFlags, NetSegment.Flags vanillaSegmentFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags);

            public Node() { }
            public Node(NetInfo.Node template) { }

            /// <summary>clone</summary>
            public Node Clone()  => this.ShalowClone();
        }

        [Serializable]
        public class LaneProp : ICloneable {
            object ICloneable.Clone() => Clone();
            [CustomizableProperty("Lane")]
            [Hint("lane extension flags")]
            public LaneInfoFlags LaneFlags = new LaneInfoFlags();

            [CustomizableProperty("Segment Extionsion")]
            [Optional(LANE_SEGMENT)]
            public SegmentInfoFlags SegmentFlags = new SegmentInfoFlags();

            [CustomizableProperty("Vanilla Segment")]
            [Optional(LANE_SEGMENT)]
            public VanillaSegmentInfoFlags VanillaSegmentFlags = new VanillaSegmentInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("Segment Tail")]
            [Optional(LANE_SEGMENT_END)]
            public SegmentEndInfoFlags SegmentStartFlags = new SegmentEndInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("Segment Head")]
            [Optional(LANE_SEGMENT_END)]
            public SegmentEndInfoFlags SegmentEndFlags = new SegmentEndInfoFlags();

            //[CustomizableProperty("Start Node Extension")]
            [Hint(LANE_HEAD_TAIL)]
            [Optional(LANE_NODE)]
            public NodeInfoFlags StartNodeFlags = new NodeInfoFlags();

            //[CustomizableProperty("End Node Extension")]
            [Hint(LANE_HEAD_TAIL)]
            [Optional(LANE_NODE)]
            public NodeInfoFlags EndNodeFlags = new NodeInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("Start Node")]
            [Optional(LANE_NODE)]
            public VanillaNodeInfoFlags VanillaStartNodeFlags = new VanillaNodeInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("End Node")]
            [Optional(LANE_NODE)]
            public VanillaNodeInfoFlags VanillaEndNodeFlags = new VanillaNodeInfoFlags();

            [CustomizableProperty("Lane Speed Limit Range")]
            public Range SpeedLimit; // null => N/A

            [CustomizableProperty("Average Speed Limit Range")]
            public Range AverageSpeedLimit; // null => N/A

            //[CustomizableProperty("Lane Curve")]
            //public Range LaneCurve; // minimum |curve| with same sign

            //[CustomizableProperty("Segment Curve")]
            //public Range SegmentCurve;

            /// <param name="laneSpeed">game speed</param>
            /// <param name="averageSpeed">game speed</param>
            public bool Check(
                NetLaneExt.Flags laneFlags,
                NetSegmentExt.Flags segmentFlags,
                NetSegment.Flags vanillaSegmentFlags,
                NetNodeExt.Flags startNodeFlags, NetNodeExt.Flags endNodeFlags,
                NetNode.Flags vanillaStartNodeFlags, NetNode.Flags vanillaEndNodeFlags,
                NetSegmentEnd.Flags segmentStartFlags, NetSegmentEnd.Flags segmentEndFlags,
                float laneSpeed, float averageSpeed) =>
                LaneFlags.CheckFlags(laneFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) &&
                VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags) &&
                SegmentStartFlags.CheckFlags(segmentStartFlags) &&
                SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                StartNodeFlags.CheckFlags(startNodeFlags) &&
                EndNodeFlags.CheckFlags(endNodeFlags) &&
                VanillaStartNodeFlags.CheckFlags(vanillaStartNodeFlags) &&
                VanillaEndNodeFlags.CheckFlags(vanillaEndNodeFlags) &&
                SpeedLimit.CheckRange(laneSpeed) &&
                AverageSpeedLimit.CheckRange(averageSpeed);

            public LaneProp(NetLaneProps.Prop template) { }
            public LaneProp() { }
            public LaneProp Clone() => this.ShalowClone();
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

        public static bool IsAdaptive(this NetInfo info) {
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

        public static IEnumerable<NetInfo> AllElevations(this NetInfo ground) {
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


        public static void ApplyVanillaForbidden(this NetInfo info) {
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

        public static void UndoVanillaForbidden(this NetInfo info) {
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
                netInfo.GetOrCreateMetaData();

                Log.Debug($"EnsureExtended({netInfo}): successful");
            } catch (Exception e) {
                Log.Exception(e);
            }
        }

        public static void UndoExtend(this NetInfo netInfo) {
            try {
                Log.Debug($"UndoExtend({netInfo}): called");
                for (int i = 0; i < netInfo.m_nodes.Length; ++i) {
                    if (netInfo.m_nodes[i] is IInfoExtended<NetInfo.Node> ext)
                        netInfo.m_nodes[i] = ext.UndoExtend();
                }
                for (int i = 0; i < netInfo.m_segments.Length; ++i) {
                    if (netInfo.m_segments[i] is IInfoExtended<NetInfo.Segment> ext)
                        netInfo.m_segments[i] = ext.UndoExtend();
                }
                foreach (var lane in netInfo.m_lanes) {
                    var props = lane.m_laneProps.m_props;
                    int n = props?.Length ?? 0;
                    for (int i = 0; i < n; ++i) {
                        if (props[i] is IInfoExtended<NetLaneProps.Prop> ext)
                            props[i] = ext.UndoExtend();
                    }
                }
                netInfo.SetMeteData(null);
            } catch (Exception e) {
                Log.Exception(e);
            }
        }

        public static void EnsureExtended_EditedNetInfos() {
            Log.Debug($"EnsureExtended_EditedNetInfos() was called");
            foreach (var info in EditedNetInfos)
                EnsureExtended(info);
            Log.Debug($"EnsureExtended_EditedNetInfos() was successful");
        }

        public static void UndoExtend_EditedNetInfos() {
            Log.Debug($"UndoExtend_EditedNetInfos() was called");
            foreach (var info in EditedNetInfos)
                UndoExtend(info);
            Log.Debug($"UndoExtend_EditedNetInfos() was successful");
        }
        #endregion
    }
}
