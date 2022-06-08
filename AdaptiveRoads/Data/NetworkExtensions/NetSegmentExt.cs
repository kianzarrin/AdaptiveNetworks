namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.CustomScript;
    using AdaptiveRoads.Data.NetworkExtensions;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using ColossalFramework.Math;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Linq;
    using UnityEngine;
    using Log = KianCommons.Log;
    using static AdaptiveRoads.Util.Shortcuts;

    public struct NetSegmentExt {

        public ushort SegmentID;
        public float Curve;
        public float ForwardSpeedLimit; // max
        public float BackwardSpeedLimit; // max
        public float MaxSpeedLimit => Mathf.Max(ForwardSpeedLimit, BackwardSpeedLimit);
        public Flags m_flags;
        public Data.UserData UserData;


        const int CUSTOM_FLAG_SHIFT = 24;
        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;

        [Obsolete("use VanillaSegment instead")]
        public ref NetSegment Segment => ref SegmentID.ToSegment();
        public ref NetSegment VanillaSegment => ref SegmentID.ToSegment();

        #region cache
        public uint[] LaneIDs;
        public NetInfoExtionsion.Net NetInfoExt;
        #endregion

        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(ushort segmentID) {
            this = default;
            SegmentID = segmentID;
        }

        [Flags]
        public enum Flags : Int64 {
            None = 0,

            [Hint(HintExtension.VANILLA)]
            Vanilla = 1 << 0,

            [Hint("tests if all lanes have the same speed")]
            UniformSpeedLimit = 1 << 1,

            ParkingAllowedRight = 1 << 5,
            ParkingAllowedLeft = 1 << 6,
            ParkingAllowedBoth = ParkingAllowedRight | ParkingAllowedLeft,

            [Hint("similar to lane inverted flag but for segment. tests if traffic drives on left (right hand drive).")]
            LeftHandTraffic = 1 << 7,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1L << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,

            [ExpressionFlag] Expression0 = 1L << 32,
            [ExpressionFlag] Expression1 = 1L << 33,
            [ExpressionFlag] Expression2 = 1L << 34,
            [ExpressionFlag] Expression3 = 1L << 35,
            [ExpressionFlag] Expression4 = 1L << 36,
            [ExpressionFlag] Expression5 = 1L << 37,
            [ExpressionFlag] Expression6 = 1L << 38,
            [ExpressionFlag] Expression7 = 1L << 39,
            ExpressionMask = Expression0 | Expression1 | Expression2 | Expression3 | Expression4 | Expression5 | Expression6 | Expression7,
        }

        public ref NetSegmentEnd Start => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, startNode: true);
        public ref NetSegmentEnd End => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, startNode: false);
        public ref NetSegmentEnd Head => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, Segment.GetHeadNode());
        public ref NetSegmentEnd Tail => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, Segment.GetTailNode());

        public override string ToString() =>
            $"NetSegmentExt(SegmentID:{SegmentID} info={SegmentID.ToSegment().Info} flags:{m_flags}"
            + $"\n\tForwardSpeedLimit:{ForwardSpeedLimit} BackwardSpeedLimit:{BackwardSpeedLimit}"
            + $"\n\tStart:{Start})" + $"\n\tEnd  :{End}";

        public ref NetSegmentEnd GetEnd(ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(segmentId: SegmentID, nodeId: nodeID);
            if(startNode)
                return ref Start;
            else
                return ref End;
        }

        private void HandleInvalidSegment() {
            if (this.LaneIDs != null) {
                for (int laneIdnex = 0; laneIdnex < this.LaneIDs.Length; ++laneIdnex) {
                    uint laneId = this.LaneIDs[laneIdnex];
                    ref NetLaneExt laneExt = ref laneId.ToLaneExt();
                    laneExt.Init(laneId);
                }
            }

            this.Start.Init(SegmentID, true);
            this.End.Init(SegmentID, false);
            this.Init(SegmentID);
        }

        public void UpdateAllFlags() {
            try {
                if(!NetUtil.IsSegmentValid(SegmentID)) {
                    if (this.Has(NetSegment.Flags.Created))
                        Log.Debug("Skip updating invalid segment:" + SegmentID);
                    else
                        HandleInvalidSegment();
                    return;
                }

                NetInfoExt = Segment.Info?.GetMetaData();
                LaneIDs = new LaneIDIterator(SegmentID).ToArray();

                if(Log.VERBOSE) Log.Debug($"NetSegmentExt.UpdateAllFlags() called. SegmentID={SegmentID}" /*Environment.StackTrace*/, false);
                bool parkingLeft = false;
                bool parkingRight = false;
                float speed0 = -1;

                bool uniformSpeed = true;
                foreach(LaneData lane in NetUtil.IterateSegmentLanes(SegmentID)) {
                    ref NetLaneExt laneExt = ref NetworkExtensionManager.Instance.LaneBuffer[lane.LaneID];
                    laneExt.UpdateLane(lane, SegmentID);
                    if(laneExt.m_flags.IsFlagSet(NetLaneExt.Flags.ParkingAllowed)) {
                        if(lane.LeftSide)
                            parkingLeft = true;
                        else
                            parkingRight = true;
                    }
                    if(lane.LaneInfo.m_laneType.IsFlagSet(SLMan.LaneTypes) &&
                       lane.LaneInfo.m_vehicleType.IsFlagSet(SLMan.VehicleTypes)) {
                        if(speed0 == -1)
                            speed0 = laneExt.SpeedLimit;
                        else
                            uniformSpeed &= laneExt.SpeedLimit == speed0;
                    }
                }

                m_flags = m_flags.SetFlags(Flags.ParkingAllowedLeft, parkingLeft);
                m_flags = m_flags.SetFlags(Flags.ParkingAllowedRight, parkingRight);
                m_flags = m_flags.SetFlags(Flags.UniformSpeedLimit, uniformSpeed);
                m_flags = m_flags.SetFlags(Flags.LeftHandTraffic, NetUtil.LHT);


                TMPEHelpers.GetMaxSpeedLimit(SegmentID, out ForwardSpeedLimit, out BackwardSpeedLimit);

                Curve = CalculateCurve();

                Start.UpdateFlags();
                Start.UpdateDirections();
                Start.UpdateCorners();

                End.UpdateFlags();
                End.UpdateDirections();
                End.UpdateCorners();

                UserData.AllocateNames(NetInfoExt?.UserDataNamesSet?.Segment);

                if (Log.VERBOSE) Log.Debug($"NetSegmentExt.UpdateAllFlags() succeeded for {this}" /*Environment.StackTrace*/, false);
                if(Log.VERBOSE) Log.Debug($"NetSegmentExt:{NetInfoExt}, TrackLaneCount={NetInfoExt?.TrackLaneCount}");
            } catch(Exception ex) {
                Log.Exception(
                    ex,
                    $"failed to update segment:{SegmentID} info:{SegmentID.ToSegment().Info} " +
                    $"startNode:{Start.NodeID} endNode:{End.NodeID}",
                    showErrorOnce_);
                showErrorOnce_ = false;
            }
        }

        public void UpdateScriptedFlags() {
            try {
                if (NetInfoExt == null) return;

                foreach (var scriptedFlag in Flags.ExpressionMask.ExtractPow2Flags()) {
                    bool condition = false;
                    if (NetInfoExt.ScriptedFlags.TryGetValue(scriptedFlag, out var expression)) {
                        condition = expression.Condition(segmentID: SegmentID, nodeID: 0);
                    }
                    m_flags = m_flags.SetFlags(scriptedFlag, condition);
                }

                Start.UpdateScriptedFlags();
                End.UpdateScriptedFlags();

                foreach(var laneID in LaneIDs) {
                    laneID.ToLaneExt().UpdateScriptedFlags();
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }

        static bool showErrorOnce_ = true;

        /// <summary>
        /// Calculates Radius of a curved segment assuming it is part of a circle.
        /// </summary>
        public float CalculateRadius() {
            // TDOO: to calculate maximum curvature for elliptical road, cut the bezier in 10 portions
            // and then find the bezier with minimum radius.
            ref NetSegment segment = ref SegmentID.ToSegment();
            Vector2 startDir = VectorUtils.XZ(segment.m_startDirection);
            Vector2 endDir = VectorUtils.XZ(segment.m_endDirection);
            Vector2 startPos = VectorUtils.XZ(segment.m_startNode.ToNode().m_position);
            Vector2 endPos = VectorUtils.XZ(segment.m_endNode.ToNode().m_position);
            float dot = Vector2.Dot(startDir, -endDir);
            float len = (startPos - endPos).magnitude;
            return len / Mathf.Sqrt(2 - 2 * dot); // see https://github.com/CitiesSkylinesMods/TMPE/issues/793#issuecomment-616351792
        }

        public float CalculateCurve() {
            var bezier = SegmentID.ToSegment().CalculateSegmentBezier3();
            return bezier.CalculateCurve();
        }

        #region Track
        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo, int layerMask) {
            try {
                if(!SegmentID.ToSegment().IsValid())
                    return;
                //Log.DebugWait($"RenderTrackInstance() called for {this}");
                if(NetInfoExt == null || !Segment.Info.CheckNetLayers(layerMask))
                    return;
                if(!cameraInfo.Intersect(Segment.m_bounds))
                    return;
                Assertion.NotNull(LaneIDs, "LaneIDs");
                for(int laneIndex = 0; laneIndex < LaneIDs.Length; ++laneIndex) {
                    if(NetInfoExt.HasTrackLane(laneIndex)) {
                        var laneID = LaneIDs[laneIndex];
                        ref var laneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
                        laneExt.RenderTrackInstance(cameraInfo);
                    }
                }
            } catch(Exception ex) {
                ex.Log($"failed to render {SegmentID}", false);
            }
        }
        public bool CalculateGroupData(int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            if(NetInfoExt == null)
                return false;
            if(!Segment.Info.CheckNetLayers(1 << layer))
                return false;
            if(NetInfoExt.TrackLaneCount == 0)
                return false;
            if(Log.VERBOSE) Log.Called(SegmentID);
            bool result = false;
            for(int laneIndex = 0; laneIndex < LaneIDs.Length; ++laneIndex) {
                if(NetInfoExt.HasTrackLane(laneIndex)) {
                    var laneID = LaneIDs[laneIndex];
                    ref var laneExt = ref laneID.ToLaneExt();
                    result |= laneExt.CalculateGroupData(layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                }
            }
            return result;
        }
        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            if(NetInfoExt == null)
                return;
            if(!Segment.Info.CheckNetLayers(1 << layer))
                return;
            if(NetInfoExt.TrackLaneCount == 0)
                return;
            if(Log.VERBOSE) Log.Called(SegmentID);
            min = Vector3.Min(min, Segment.m_bounds.min);
            max = Vector3.Max(max, Segment.m_bounds.max);
            maxRenderDistance = Mathf.Max(maxRenderDistance, 30000f);
            maxInstanceDistance = Mathf.Max(maxInstanceDistance, 1000f);
            for(int laneIndex = 0; laneIndex < LaneIDs.Length; ++laneIndex) {
                if(NetInfoExt.HasTrackLane(laneIndex)) {
                    var laneID = LaneIDs[laneIndex];
                    ref var laneExt = ref laneID.ToLaneExt();
                    laneExt.PopulateGroupData(groupX, groupZ, layer: layer, ref vertexIndex, ref triangleIndex, groupPosition, data);
                }
            }
        }

        public static void Render(NetInfo info, NetSegment.Flags flags, OutlineData outline) {
            var infoExt = info?.GetMetaData();
            if(infoExt == null || infoExt.TrackLaneCount == 0)
                return;

            for(int laneIndex=0;laneIndex< info.m_lanes.Length; ++laneIndex) {
                if(infoExt.HasTrackLane(laneIndex)) {
                    NetLaneExt.Render(info, laneIndex, flags, outline);
                }
            }
        }

        #endregion
    }
}

