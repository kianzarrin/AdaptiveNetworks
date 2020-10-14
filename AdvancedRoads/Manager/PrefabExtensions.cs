using KianCommons;
using PrefabIndeces;
using System;
using System.Collections.Generic;
using System.Linq;
using static AdaptiveRoads.Manager.NetInfoExt;

namespace AdaptiveRoads.Manager {
    [AttributeUsage(AttributeTargets.Struct)]
    public class FlagPairAttribute : Attribute {
        public string Name;
        public FlagPairAttribute(string name) => Name = name;
        public FlagPairAttribute() { }
    }

    public static class Extensions {
        public static Segment GetExt(this NetInfoExtension.Segment IndexExt)
            => Segment.Get(IndexExt);

        public static Segment GetExt(this NetInfo.Segment segment) =>
            (segment as NetInfoExtension.Segment)?.GetExt();

        public static Node GetExt(this NetInfoExtension.Node IndexExt)
            => Node.Get(IndexExt);

        public static Node GetExt(this NetInfo.Node node) =>
            (node as NetInfoExtension.Node)?.GetExt();

        public static Lane GetExt(this NetInfoExtension.Lane IndexExt)
            => Lane.Get(IndexExt);

        public static Lane GetExt(this NetInfo.Lane lane) =>
            (lane as NetInfoExtension.Lane)?.GetExt();

        public static LaneProp GetExt(this NetInfoExtension.Lane.Prop IndexExt)
            => LaneProp.Get(IndexExt);

        public static LaneProp GetExt(this NetLaneProps.Prop prop) =>
            (prop as NetInfoExtension.Lane.Prop)?.GetExt();

        public static NetInfoExt GetExt(this NetInfo info) => NetInfoExt.Buffer[info.GetIndex()];

        public static void SetExt(this NetInfo info, NetInfoExt netInfoExt)
            => NetInfoExt.SetNetInfoExt(info, netInfoExt);

        public static IEnumerable<NetInfo> AllElevations(this NetInfo ground) =>
            NetInfoExt.AllElevations(ground);

        public static bool CheckRange(this Range range, float value) => range?.InRange(value) ?? true;
    }

    [Serializable]
    public class NetInfoExt {
        #region value types
        [Serializable]
        public class Range {
            public float Lower, Upper;
            public bool InRange(float value) => Lower <= value && value < Upper;
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
        public class Segment {
            [Serializable]
            public struct FlagsT {
                public SegmentInfoFlags Flags;
                public SegmentEndInfoFlags Start, End;
                public bool CheckFlags(
                    NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags) {
                    return
                        Flags.CheckFlags(flags) &
                        Start.CheckFlags(startFlags) &
                        End.CheckFlags(endFlags);
                }
            }

            public FlagsT ForwardFlags, BackwardFlags;

            public bool CheckFlags(NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags startFlags,
                    NetSegmentEnd.Flags endFlags,
                    bool turnAround) {
                if (!turnAround)
                    return ForwardFlags.CheckFlags(flags, startFlags, endFlags);
                else
                    return BackwardFlags.CheckFlags(flags, startFlags, endFlags);
            }

            public static bool CheckFlags(NetInfo.Segment segmentInfo, NetSegment.Flags flags, bool turnAround) {
                if (!turnAround)
                    return flags.CheckFlags(segmentInfo.m_forwardRequired, segmentInfo.m_forwardForbidden);
                else
                    return flags.CheckFlags(segmentInfo.m_backwardRequired, segmentInfo.m_backwardForbidden);
            }

            private Segment() { }
            public Segment(NetInfo.Segment template) { }

            /// <summary>clone</summary>
            public Segment Clone() {
                var clone = new Segment();
                clone.ForwardFlags = this.ForwardFlags;
                clone.BackwardFlags = this.BackwardFlags;
                return clone;
            }

            public static Segment Get(NetInfoExtension.Segment IndexExt) {
                if (IndexExt == null || Buffer[IndexExt.PrefabIndex] == null)
                    return null;
                return Buffer[IndexExt.PrefabIndex].SegmentInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class Node {
            public NodeInfoFlags NodeFlags;

            [CustomizableProperty("Segment End")]
            public SegmentEndInfoFlags SegmentEndFlags;

            public bool CheckFlags(NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags) =>
                NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags);

            private Node() { }
            public Node(NetInfo.Node template) { }

            /// <summary>clone</summary>
            public Node Clone() {
                var clone = new Node();
                clone.NodeFlags = this.NodeFlags;
                clone.SegmentEndFlags = this.SegmentEndFlags;
                return clone;
            }

            public static Node Get(NetInfoExtension.Node IndexExt) {
                if (IndexExt == null || Buffer[IndexExt.PrefabIndex] == null)
                    return null;
                return Buffer[IndexExt.PrefabIndex].NodeInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class Lane {
            //[CustomizableProperty("Lane")]
            //public LaneInfoFlags LaneFlags;

            public LaneProp[] PropInfoExts;

            public Lane(NetInfo.Lane template) {
                Assertion.AssertNotNull(template, "template");
                PropInfoExts = new LaneProp[template.m_laneProps?.m_props?.Length ?? 0];
                for (int i = 0; i < PropInfoExts.Length; ++i) {
                    PropInfoExts[i] = new LaneProp(template.m_laneProps.m_props[i]);
                }
            }
            private Lane() { }
            public Lane Clone() {
                var clone = new Lane();
                clone.PropInfoExts = new LaneProp[PropInfoExts.Length];
                for (int i = 0; i < PropInfoExts.Length; ++i) {
                    clone.PropInfoExts[i] = PropInfoExts[i].Clone();
                }
                return clone;
            }


            public static Lane Get(NetInfoExtension.Lane IndexExt) {
                if (IndexExt == null || Buffer[IndexExt.PrefabIndex] == null)
                    return null;
                return Buffer[IndexExt.PrefabIndex].LaneInfoExts[IndexExt.Index];
            }
        }

        [Serializable]
        public class LaneProp {
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

            public static LaneProp Get(NetInfoExtension.Lane.Prop IndexExt) {
                if (IndexExt == null || Buffer[IndexExt.PrefabIndex] == null)
                    return null;
                return Buffer[IndexExt.PrefabIndex]
                    .LaneInfoExts[IndexExt.LaneIndex]
                    .PropInfoExts[IndexExt.Index];
            }
        }

        #endregion

        #region NetInfoExt instance
        public Version Version;

        public Node[] NodeInfoExts;

        public Segment[] SegmentInfoExts;

        public Lane[] LaneInfoExts;

        public NetInfoExt(NetInfo template) {
            Version = this.VersionOf();
            SegmentInfoExts = new Segment[template.m_segments?.Length ?? 0];
            for (int i = 0; i < SegmentInfoExts.Length; ++i) {
                SegmentInfoExts[i] = new Segment(template.m_segments[i]);
            }

            NodeInfoExts = new Node[template.m_nodes?.Length ?? 0];
            for (int i = 0; i < NodeInfoExts.Length; ++i) {
                NodeInfoExts[i] = new Node(template.m_nodes[i]);
            }

            LaneInfoExts = new Lane[template.m_lanes?.Length ?? 0];
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                LaneInfoExts[i] = new Lane(template.m_lanes[i]);
            }
        }

        private NetInfoExt() { }
        public NetInfoExt Clone() {
            var clone = new NetInfoExt();
            clone.Version = Version;

            clone.SegmentInfoExts = new Segment[this.SegmentInfoExts.Length];
            for (int i = 0; i < SegmentInfoExts.Length; ++i) {
                clone.SegmentInfoExts[i] = this.SegmentInfoExts[i].Clone();
            }

            clone.NodeInfoExts = new Node[this.NodeInfoExts.Length];
            for (int i = 0; i < NodeInfoExts.Length; ++i) {
                clone.NodeInfoExts[i] = this.NodeInfoExts[i].Clone();
            }

            clone.LaneInfoExts = new Lane[this.LaneInfoExts.Length];
            for (int i = 0; i < LaneInfoExts.Length; ++i) {
                clone.LaneInfoExts[i] = this.LaneInfoExts[i].Clone();
            }

            Log.Debug("NetInfoExt.Clone()->" + clone);
            return clone;
        }
        #endregion NetInfoExt instance

        #region static 
        public static NetInfoExt[] Buffer;
        public static Dictionary<NetInfo, NetInfoExt> DataDict;

        public static void ApplyDataDict() {
            if (DataDict == null) return;
            foreach (var pair in DataDict) {
                SetNetInfoExt(pair.Key.GetIndex(), pair.Value);
            }
            DataDict = null;
        }


        public static NetInfo EditNetInfo =>
            ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;

        public static IEnumerable<NetInfo> EditNetInfos =>
            AllElevations(EditNetInfo);

        public static NetInfoExt EditNetInfoExt {
            get {
                EnsureBuffer();
                return EditNetInfo?.GetExt();
            }
        }

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

        public static void ReExtendEditedPrefabIndeces() {
            try {
                Log.Debug("ReExtendedEditedPrefabIndeces called");
                foreach (var info in NetInfoExt.EditNetInfos)
                    info.ExtendPrefab();
            }
            catch (Exception e) {
                Log.Exception(e);
            }
        }


        public static void EnsureEditNetInfoExt() {
            EnsureNetInfoExt(EditNetInfo);
        }

        public static void EnsureNetInfoExt(NetInfo info) {
            EnsureBuffer();
            if (info != null && info.GetExt() == null) {
                CreateAllNetInfoExt(info);
            }
        }

        public static void CreateAllNetInfoExt(NetInfo ground) {
            CreateNewNetInfoExt(ground);
            CreateNewNetInfoExt(AssetEditorRoadUtils.TryGetElevated(ground));
            CreateNewNetInfoExt(AssetEditorRoadUtils.TryGetBridge(ground));
            CreateNewNetInfoExt(AssetEditorRoadUtils.TryGetSlope(ground));
            CreateNewNetInfoExt(AssetEditorRoadUtils.TryGetTunnel(ground));
        }

        public static void CreateNewNetInfoExt(NetInfo info) {
            if (info == null) return;
            SetNetInfoExt(info, new NetInfoExt(info));
        }

        public static void SetNetInfoExt(NetInfo info, NetInfoExt netInfoExt) {
            Assertion.AssertNotNull(info, "DataDict");
            if (info.GetIndex() != 0) {
                SetNetInfoExt(info.GetIndex(), netInfoExt);
            } else {
                // if level is not loaded, prefab indeces are zero.
                // put it inside dict so that I can move them to buffer later.
                Assertion.AssertNotNull(DataDict, "DataDict");
                DataDict[info] = netInfoExt;
            }
        }

        public static void SetNetInfoExt(int index, NetInfoExt netInfoExt) {
            Log.Debug($"SetNetInfoExt({index},{netInfoExt?.ToString() ?? "null"})");
            EnsureBuffer();
            ExtendPrefabIndeces(index);
            Buffer[index] = netInfoExt;
            Log.Debug($"SetNetInfoExt({index},{netInfoExt}) Result: Buffer[{index}] = {netInfoExt};");
        }

        public static void ExtendPrefabIndeces(int netInfoIndex) {
            NetInfo netInfo = PrefabCollection<NetInfo>.GetPrefab((uint)netInfoIndex);
            PrefabIndeces.NetInfoExtension.ExtendPrefab(netInfo);
        }

        public static void Init() {
            Log.Debug($"NetInfoExt.Init() : prefab count={NetInfoCount}\n" /* + Environment.StackTrace*/);
            Buffer = new NetInfoExt[NetInfoCount];
        }
        public static void Unload() {
            Buffer = null;
        }

        public static void EnsureBuffer() {
            if (Buffer == null || Buffer.Length != NetInfoCount)
                ExpandBuffer();
        }
        public static void ExpandBuffer() {
            Log.Debug("ExpandBuffer() called\n" /*+ Environment.StackTrace*/);
            if (Buffer == null) {
                Init();
                return;
            }
            var old = Buffer;
            Init();
            for (int i = 0; i < old.Count(); ++i)
                Buffer[i] = old[i];
        }

        /// <param name="forceCreate">if true creates new NetInfoExt for target if source does not have NetInfoExt</param>
        public static void CopyAll(NetInfo source, NetInfo target, bool forceCreate) {
            Log.Debug($"CopyAll(source={source.name}:{source.m_prefabDataIndex} , " +
                $"target={target.name}:{target.m_prefabDataIndex}, forceCreate:{forceCreate}) called");
            NetInfo elevated0 = AssetEditorRoadUtils.TryGetElevated(source);
            NetInfo bridge0 = AssetEditorRoadUtils.TryGetBridge(source);
            NetInfo slope0 = AssetEditorRoadUtils.TryGetSlope(source);
            NetInfo tunnel0 = AssetEditorRoadUtils.TryGetTunnel(source);

            NetInfo elevated1 = AssetEditorRoadUtils.TryGetElevated(target);
            NetInfo bridge1 = AssetEditorRoadUtils.TryGetBridge(target);
            NetInfo slope1 = AssetEditorRoadUtils.TryGetSlope(target);
            NetInfo tunnel1 = AssetEditorRoadUtils.TryGetTunnel(target);

            Copy(source.GetIndex(), target.GetIndex(), forceCreate);
            Copy(elevated0.GetIndex(), elevated1.GetIndex(), forceCreate);
            Copy(bridge0.GetIndex(), bridge1.GetIndex(), forceCreate);
            Copy(slope0.GetIndex(), slope1.GetIndex(), forceCreate);
            Copy(tunnel0.GetIndex(), tunnel1.GetIndex(), forceCreate);
        }

        /// <param name="forceCreate">if true creates new NetInfoExt for target if source does not have NetInfoExt</param>
        public static void Copy(ushort sourceIndex, ushort targetIndex, bool forceCreate) {
            Assertion.AssertNeq(sourceIndex, 0, "sourceIndex, cannot copy before level loaded");
            Assertion.AssertNeq(targetIndex, 0, "targetIndex, cannot copy before level loaded");
            Log.Debug($"NetInfoExt.Copy(source:{sourceIndex}, target:{targetIndex}, forceCreate:{forceCreate} called)");
            NetInfoExt sourceNetInfoExt =
                Buffer.Length > sourceIndex ? NetInfoExt.Buffer[sourceIndex] : null;
            if (sourceNetInfoExt != null)
                NetInfoExt.SetNetInfoExt(targetIndex, sourceNetInfoExt.Clone());
            else if (forceCreate) {
                Log.Debug($"NetInfoExt.Copy: forceCreating ...");
                var netInfo = PrefabCollection<NetInfo>.GetPrefab(targetIndex);
                CreateNewNetInfoExt(netInfo);
                Log.Debug($"NetInfoExt.Copy: forceCreate ->  Buffer[{targetIndex}] = {Buffer[targetIndex]}");
            } else {
                Log.Debug($"NetInfoExt.Copy: skipped forceCreating. sourceNetInfoExt={sourceNetInfoExt} forceCreate={forceCreate}");
            }
        }
        #endregion
    }
}
