namespace AdaptiveRoads.Manager {
    using ColossalFramework;
    using ColossalFramework.IO;
    using ColossalFramework.Math;
    using CSUtil.Commons;
    using KianCommons;
    using System;
    using TrafficManager;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using UnityEngine;
    using Log = KianCommons.Log;
    using System.Linq;
    using AdaptiveRoads.Util;
    using KianCommons.Serialization;
    using KianCommons.Plugins;
    using System.Reflection;
    using AdaptiveRoads.Data;

    public static class AdvanedFlagsExtensions {
        public static bool CheckFlags(this NetLaneExt.Flags value, NetLaneExt.Flags required, NetLaneExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegmentEnd.Flags value, NetSegmentEnd.Flags required, NetSegmentEnd.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetSegmentExt.Flags value, NetSegmentExt.Flags required, NetSegmentExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;
        public static bool CheckFlags(this NetNodeExt.Flags value, NetNodeExt.Flags required, NetNodeExt.Flags forbidden) =>
            (value & (required | forbidden)) == required;

        public static NetLaneExt.Flags SetMaskedFlags(this NetLaneExt.Flags flags, NetLaneExt.Flags value, NetLaneExt.Flags mask) =>
            (flags & ~mask) | (value & mask);
        public static NetSegmentEnd.Flags SetMaskedFlags(this NetSegmentEnd.Flags flags, NetSegmentEnd.Flags value, NetSegmentEnd.Flags mask) =>
            (flags & ~mask) | (value & mask);
        public static NetSegmentExt.Flags SetMaskedFlags(this NetSegmentExt.Flags flags, NetSegmentExt.Flags value, NetSegmentExt.Flags mask) =>
            (flags & ~mask) | (value & mask);
        public static NetNodeExt.Flags SetMaskedFlags(this NetNodeExt.Flags flags, NetNodeExt.Flags value, NetNodeExt.Flags mask) =>
            (flags & ~mask) | (value & mask);
    }

    public struct CustomFlags {
        public NetNodeExt.Flags Node;
        public NetSegmentExt.Flags Segment;
        public NetSegmentEnd.Flags SegmentEnd;
        public NetLaneExt.Flags Lane;

        public static CustomFlags None = default;

        public static CustomFlags operator |(CustomFlags lhs, CustomFlags rhs) {
            return new CustomFlags {
                Node = lhs.Node | rhs.Node,
                Segment = lhs.Segment | rhs.Segment,
                SegmentEnd = lhs.SegmentEnd | rhs.SegmentEnd,
                Lane = lhs.Lane | rhs.Lane,
            };
        }

        public bool IsDefault() {
            return
                Node == default &&
                Segment == default &&
                SegmentEnd == default &&
                Lane == default;
        }
    }

    public class CustomFlagAttribute : Attribute {
        public static string GetName(Enum flag, NetInfo netInfo) {
            var cfn = netInfo?.GetMetaData()?.CustomFlagNames;
            if (cfn != null && cfn.TryGetValue(flag, out string ret))
                return ret;
            return null;
        }
    }

    public struct NetLaneExt {
        [Flags]
        public enum Flags : Int64 {
            None = 0,

            [Hide]
            [Hint("[Obsolete] " + HintExtension.VANILLA)]
            Vanilla = 1 << 0,

            ParkingAllowed = 1 << 4,

            // Vehicle restrictions
            [Hint("private cars and motorbikes")]
            Car = 1 << 5,

            [Hint("Emergencies (active sirens)\n" +
                "when siren is off, they act as Service vehicles.")]
            SOS = 1 << 6,

            Taxi = 1 << 7,

            [Hint("passenger, sightseeing and evacuation buses")]
            Bus = 1 << 8,

            [Hint("all types of industry trucks and vans")]
            CargoTruck = 1 << 9,

            [Hint("all services including emergency services without active siren")]
            Service = 1 << 10,

            CargoTrain = 1 << 14,
            PassengerTrain = 1 << 15,

            [Hint("this lane has a single merging transition\n" +
                  "use this in conjunction with TwoSegment node flag to put merge arrow road marking")]
            MergeUnique = 1 << 16,

            [Hint("cars can go to multiple lanes from this lane at least one of which is non-merging transition.\n" +
                  "use this in conjunction with TwoSegment node flag to put split arrow road marking")]
            SplitUnique = 1 << 17,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1L << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,

            LeftSlight = 1L << 32,
            LeftModerate = 1L << 33,
            LeftSharp = 1L << 34,
            UTurn = 1L << 38,

            RightSlight = 1L << 35,
            RightModerate = 1L << 36,
            RightSharp = 1L << 37,
            AllDirections = LeftSlight | LeftModerate | LeftSharp | RightSlight | RightModerate | RightSharp | UTurn,
        }

        public Flags m_flags;

        public float SpeedLimit; // game speed limit 1=50kph 20=unlimited

        //public object OuterMarking;
        //public object InnerMarking;

        public LaneData LaneData;

        const int CUSTOM_FLAG_SHIFT = 24;

        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;
        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(uint laneID) {
            LaneData.LaneID = laneID;
            LaneData.LaneIndex = 0;
            LaneData.LaneInfo = null;
            m_flags = Flags.None;
        }

        static IManagerFactory TMPE => Constants.ManagerFactory;
        static IParkingRestrictionsManager PMan => TMPE?.ParkingRestrictionsManager;
        static IVehicleRestrictionsManager VRMan => TMPE?.VehicleRestrictionsManager;
        static ISpeedLimitManager SLMan => TMPE?.SpeedLimitManager as SpeedLimitManager;
        static IRoutingManager RMan => TMPE?.RoutingManager as RoutingManager;

        // pass in segmentID for the sake of MOM lane problem.
        public void UpdateLane(LaneData lane, ushort segmentID) {
            Assertion.AssertEqual(LaneData.LaneID, lane.LaneID, "lane id");
            if(lane.Lane.m_segment != 0 && lane.Lane.m_segment != segmentID)
                Log.Error($"lane segment mismatch: {LaneData} parentSegment:{segmentID}");
            lane.Lane.m_segment = segmentID; // fix MOM lane issue

            try {
                LaneData = lane;

                bool parkingAllowed = LaneData.LaneInfo.m_laneType == NetInfo.LaneType.Parking;
                if(PMan != null)
                    parkingAllowed &= PMan.IsParkingAllowed(LaneData.SegmentID, LaneData.LaneInfo.m_finalDirection);
                m_flags = m_flags.SetFlags(Flags.ParkingAllowed, parkingAllowed);

                ExtVehicleType mask = 0;
                if(VRMan != null) {
                    mask = VRMan.GetAllowedVehicleTypes(
                        segmentId: segmentID,
                        segmentInfo: segmentID.ToSegment().Info,
                        laneIndex: (uint)LaneData.LaneIndex,
                        laneInfo: LaneData.LaneInfo,
                        busLaneMode: VehicleRestrictionsMode.Configured);
                }

                m_flags = m_flags.SetFlags(Flags.Car, VRMan.IsPassengerCarAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.SOS, VRMan.IsEmergencyAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.Bus, VRMan.IsBusAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.CargoTruck, VRMan.IsCargoTruckAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.Taxi, VRMan.IsTaxiAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.Service, VRMan.IsServiceAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.CargoTrain, VRMan.IsCargoTrainAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.PassengerTrain, VRMan.IsPassengerTrainAllowed(mask));
                m_flags = m_flags.SetFlags(Flags.SplitUnique, lane.IsSplitsUnique());
                m_flags = m_flags.SetFlags(Flags.MergeUnique, lane.IsMergesUnique());
                m_flags = (m_flags & ~Flags.AllDirections) | lane.GetArrowsExt();

                SpeedLimit = lane.GetLaneSpeedLimit();

                UpdateCorners();
                //Log.Debug("NetLaneExt.UpdateLane() result: " + this);
            } catch(Exception ex) {
                Log.Exception(ex, this.ToString(), false);
                throw ex;
            }
        }

        public override string ToString() {
            return $"NetLaneExt({LaneData} flags={m_flags} speed={SpeedLimit})";
        }

        #region corner
        // left and right is WRT the direction of the bezier
        public CornerTripleData A, D;
        public Bezier3 Left, Right; 
        public void UpdateCorners() {
            ushort segmentID = LaneData.SegmentID;
            ref var segment = ref LaneData.Segment;
            ref var segmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];

            var posStartLeft = segmentExt.Start.Corner.Left.Position;
            var posStartRight = segmentExt.Start.Corner.Right.Position;
            var posEndLeft = segmentExt.End.Corner.Left.Position;
            var posEndRight = segmentExt.End.Corner.Right.Position;

            var DirectionStartLeft = segmentExt.Start.Corner.Left.Direction;
            var DirectionStartRight = segmentExt.Start.Corner.Right.Direction;
            var DirectionEndLeft = segmentExt.End.Corner.Left.Direction;
            var DirectionEndRight = segmentExt.End.Corner.Right.Direction;

            var smoothStart = segmentExt.Start.Corner.smooth;
            var smoothEnd = segmentExt.End.Corner.smooth;

            var laneInfo = LaneData.LaneInfo;
            float posNormalized = laneInfo.m_position / (segment.Info.m_halfWidth * 2f) + 0.5f;
            if(segment.IsInvert()) {
                posNormalized = 1f - posNormalized;
            }
            Vector3 a = posStartLeft + (posStartRight - posStartLeft) * posNormalized;
            Vector3 startDir = Vector3.Lerp(DirectionStartLeft, DirectionStartRight, posNormalized);
            Vector3 d = posEndRight + (posEndLeft - posEndRight) * posNormalized;
            Vector3 endDir = Vector3.Lerp(DirectionEndRight, DirectionEndLeft, posNormalized);
            a.y += laneInfo.m_verticalOffset;
            d.y += laneInfo.m_verticalOffset;
            NetSegment.CalculateMiddlePoints(a, startDir, d, endDir, smoothStart, smoothEnd, out var b, out var c);
            //var bezier = new Bezier3(a, b, c, d);

            float laneWidth = laneInfo.m_width;
            A.Set(a, startDir, laneWidth, start: true);
            D.Set(d, endDir, laneWidth, start: false);

            Left.a = A.Left;
            Left.d = D.Left; 
            NetSegment.CalculateMiddlePoints(A.Left, A.Direction, D.Left, D.Direction, smoothStart, smoothEnd, out Left.b, out Left.c);

            Right.a = A.Right;
            Right.d = D.Right; 
            NetSegment.CalculateMiddlePoints(A.Right, A.Direction, D.Right, D.Direction, smoothStart, smoothEnd, out Right.b, out Right.c);
        }
        #endregion

        #region track
        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo, int layerMask, ref RenderManager.Instance renderData) {
            var info = LaneData.Segment.Info;
            if((layerMask & info.m_netLayers) == 0)
                return;
            UInt64 laneBit = 1ul << LaneData.LaneIndex;
            var infoExt = info.GetMetaData();
            var netManager = Singleton<NetManager>.instance;
            ref var segment = ref LaneData.Segment;
            ref var segmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[LaneData.SegmentID];
            foreach(var track in infoExt.Tracks) {
                if((track.LaneIndeces & laneBit) != 0 && track.CheckSegmentFlags(segmentExt.m_flags, segment.m_flags)) {
                    Vector4 dataVector3 = renderData.m_dataVector3;
                    if(track.m_requireWindSpeed) 
                        dataVector3.w = renderData.m_dataFloat0;

                    Vector4 dataVector0 = renderData.m_dataVector0;
                    bool turnAround = LaneData.LaneInfo.IsGoingBackward(); // TODO is this logic sufficient? is this line even necessary?
                    if(turnAround) {
                        dataVector0.x = - dataVector0.x;
                        dataVector0.y = - dataVector0.y;
                    }
                    if(cameraInfo.CheckRenderDistance(renderData.m_position, track.m_lodRenderDistance)) {
                        netManager.m_materialBlock.Clear();
                        netManager.m_materialBlock.SetMatrix(netManager.ID_LeftMatrix, renderData.m_dataMatrix0);
                        netManager.m_materialBlock.SetMatrix(netManager.ID_RightMatrix, renderData.m_dataMatrix1);
                        netManager.m_materialBlock.SetVector(netManager.ID_MeshScale, dataVector0);
                        netManager.m_materialBlock.SetVector(netManager.ID_ObjectIndex, dataVector3);
                        netManager.m_materialBlock.SetColor(netManager.ID_Color, renderData.m_dataColor0);
                        netManager.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(track.m_trackMesh, renderData.m_position, renderData.m_rotation, track.m_trackMaterial, track.m_layer, null, 0, netManager.m_materialBlock);
                    } else {
                        NetInfo.LodValue combinedLod = track.m_combinedLod;
                        if(combinedLod == null) continue;

                        ref Matrix4x4 reference = ref combinedLod.m_leftMatrices[combinedLod.m_lodCount];
                        reference = renderData.m_dataMatrix0;
                        ref Matrix4x4 reference2 = ref combinedLod.m_rightMatrices[combinedLod.m_lodCount];
                        reference2 = renderData.m_dataMatrix1;
                        combinedLod.m_meshScales[combinedLod.m_lodCount] = dataVector0;
                        combinedLod.m_objectIndices[combinedLod.m_lodCount] = dataVector3;
                        ref Vector4 reference3 = ref combinedLod.m_meshLocations[combinedLod.m_lodCount];
                        reference3 = renderData.m_position;
                        combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, renderData.m_position);
                        combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, renderData.m_position);
                        if(++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
                            NetInfoExtionsion.Track.RenderLod(cameraInfo, combinedLod);
                        }
                    }
                }
            }
        }

#if true //DUMMY_CODE
        private void RenderSegmentInstance(ref NetSegment This, RenderManager.CameraInfo cameraInfo, ushort segmentID, int layerMask, NetInfo info, ref RenderManager.Instance renderData) {
            var netManager = Singleton<NetManager>.instance;
            if(info.m_segments != null && (layerMask & info.m_netLayers) != 0) {
                for(int j = 0; j < info.m_segments.Length; j++) {
                    NetInfo.Segment segment = info.m_segments[j];
                    if(!segment.CheckFlags(This.m_flags, out var turnAround)) {
                        continue;
                    }
                    Vector4 dataVector3 = renderData.m_dataVector3;
                    Vector4 dataVector0 = renderData.m_dataVector0;
                    if(segment.m_requireWindSpeed) {
                        dataVector3.w = renderData.m_dataFloat0;
                    }
                    if(turnAround) {
                        dataVector0.x = 0f - dataVector0.x;
                        dataVector0.y = 0f - dataVector0.y;
                    }
                    if(cameraInfo.CheckRenderDistance(renderData.m_position, segment.m_lodRenderDistance)) {
                        netManager.m_materialBlock.Clear();
                        netManager.m_materialBlock.SetMatrix(netManager.ID_LeftMatrix, renderData.m_dataMatrix0);
                        netManager.m_materialBlock.SetMatrix(netManager.ID_RightMatrix, renderData.m_dataMatrix1);
                        netManager.m_materialBlock.SetVector(netManager.ID_MeshScale, dataVector0);
                        netManager.m_materialBlock.SetVector(netManager.ID_ObjectIndex, dataVector3);
                        netManager.m_materialBlock.SetColor(netManager.ID_Color, renderData.m_dataColor0);
                        if(segment.m_requireSurfaceMaps && renderData.m_dataTexture0 != null) {
                            netManager.m_materialBlock.SetTexture(netManager.ID_SurfaceTexA, renderData.m_dataTexture0);
                            netManager.m_materialBlock.SetTexture(netManager.ID_SurfaceTexB, renderData.m_dataTexture1);
                            netManager.m_materialBlock.SetVector(netManager.ID_SurfaceMapping, renderData.m_dataVector1);
                        } else if(segment.m_requireHeightMap && renderData.m_dataTexture0 != null) {
                            netManager.m_materialBlock.SetTexture(netManager.ID_HeightMap, renderData.m_dataTexture0);
                            netManager.m_materialBlock.SetVector(netManager.ID_HeightMapping, renderData.m_dataVector1);
                            netManager.m_materialBlock.SetVector(netManager.ID_SurfaceMapping, renderData.m_dataVector2);
                        }
                        netManager.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(segment.m_segmentMesh, renderData.m_position, renderData.m_rotation, segment.m_segmentMaterial, segment.m_layer, null, 0, netManager.m_materialBlock);

                    } else {
                        NetInfo.LodValue combinedLod = segment.m_combinedLod;
                        if(combinedLod == null) continue;

                        ref Matrix4x4 reference = ref combinedLod.m_leftMatrices[combinedLod.m_lodCount];
                        reference = renderData.m_dataMatrix0;
                        ref Matrix4x4 reference2 = ref combinedLod.m_rightMatrices[combinedLod.m_lodCount];
                        reference2 = renderData.m_dataMatrix1;
                        combinedLod.m_meshScales[combinedLod.m_lodCount] = dataVector0;
                        combinedLod.m_objectIndices[combinedLod.m_lodCount] = dataVector3;
                        ref Vector4 reference3 = ref combinedLod.m_meshLocations[combinedLod.m_lodCount];
                        reference3 = renderData.m_position;
                        combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, renderData.m_position);
                        combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, renderData.m_position);
                        if(++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
                            NetSegment.RenderLod(cameraInfo, combinedLod);
                        }
                    }
                }
            }
        }
#endif

        public  void RefreshRenderData(ref RenderManager.Instance renderData) {
            ref var segment = ref LaneData.Segment;
            var info = segment.Info;
            ref NetNode startNode = ref segment.m_startNode.ToNode();
            ref NetNode endNode = ref segment.m_endNode.ToNode();
            ref var bezier = ref LaneData.Lane.m_bezier;
            ref var lane = ref LaneData.Lane;
            var laneInfo = LaneData.LaneInfo;
            Vector3 startPos = bezier.a;
            Vector3 endPos = bezier.d;

            renderData.m_dataInt0 = LaneData.LaneIndex;
            renderData.m_position = (startPos + endPos) * 0.5f;
            renderData.m_rotation = Quaternion.identity;
            renderData.m_dataColor0 = info.m_color;
            renderData.m_dataColor0.a = 0f;
            renderData.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(renderData.m_position);
            renderData.m_dataVector0 = new Vector4(1f / laneInfo.m_width, 1f / info.m_segmentLength, 1f, 1f);
            Vector4 colorLocationStart = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + LaneData.SegmentID);
            Vector4 colorLocationEnd = colorLocationStart;
            if(NetNode.BlendJunction(segment.m_startNode)) {
                colorLocationStart = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segment.m_startNode);
            }
            if(NetNode.BlendJunction(segment.m_endNode)) {
                colorLocationEnd = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segment.m_endNode);
            }
            renderData.m_dataVector3 = new Vector4(colorLocationStart.x, colorLocationStart.y, colorLocationEnd.x, colorLocationEnd.y);
            {
                float vScale = info.m_netAI.GetVScale();
                renderData.m_dataMatrix0 = NetSegment.CalculateControlMatrix(
                    Left.a, Left.b, Left.c, Left.d,
                    Right.a, Right.b, Right.c, Right.d,
                    renderData.m_position, vScale);
                renderData.m_dataMatrix1 = NetSegment.CalculateControlMatrix(
                    Right.a, Right.b, Right.c, Right.d,
                    Left.a, Left.b, Left.c, Left.d,
                    renderData.m_position, vScale);
            }
        }
        #endregion
    }

    public struct NetNodeExt {
        public ushort NodeID;
        public Flags m_flags;

        const int CUSTOM_FLAG_SHIFT = 24;
        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;
        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(ushort nodeID) => NodeID = nodeID;

        [Flags]
        public enum Flags {
            None = 0,
            [Hint(HintExtension.VANILLA)]
            Vanilla = 1 << 0,

            [Hide]
            [Hint("Hide Crossings mod is active")]
            HC_Mod = 1 << 1,

            [Hint("Direct Connect Roads mod is active")]
            DCR_Mod = 1 << 2,

            [Hint("Hide Unconnected Tracks mod is active")]
            HUT_Mod = 1 << 3,

            [Hint("all entering segment ends keep clear of the junction." +
                "useful for drawing pattern on the junction.")]
            KeepClearAll = 1 << 10,

            [Hint("the junction only has two segments.")]
            TwoSegments = 1 << 11,

            [Hint("the junction has segments with different speed limits.")]
            SpeedChange = 1 << 12,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1 << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,
        }

        public static IJunctionRestrictionsManager JRMan =>
            TrafficManager.Constants.ManagerFactory.JunctionRestrictionsManager;

        public void UpdateFlags() {
            m_flags = m_flags.SetFlags(Flags.HC_Mod, NetworkExtensionManager.Instance.HTC);
            m_flags = m_flags.SetFlags(Flags.DCR_Mod, NetworkExtensionManager.Instance.DCR);
            m_flags = m_flags.SetFlags(Flags.HUT_Mod, NetworkExtensionManager.Instance.HUT);

            if (JRMan != null) {
                bool keepClearAll = true;
                foreach(var segmentID in NetUtil.IterateNodeSegments(NodeID)) {
                    bool startNode = NetUtil.IsStartNode(segmentId: segmentID, nodeId: NodeID);
                    bool keppClear = JRMan.IsEnteringBlockedJunctionAllowed(segmentID, startNode);
                    keepClearAll &= keppClear;

                }
                m_flags = m_flags.SetFlags(Flags.KeepClearAll, keepClearAll);


                bool speedChange = TMPEHelpers.SpeedChanges(NodeID);
                bool twoSegments = NodeID.ToNode().CountSegments() == 2;

                m_flags = m_flags.SetFlags(Flags.SpeedChange, speedChange);
                m_flags = m_flags.SetFlags(Flags.TwoSegments, twoSegments);
            }
        }

        public override string ToString() {
            return $"NetNodeExt({NodeID} flags={m_flags})";
        }
    }

    public struct NetSegmentExt {
        public ushort SegmentID;
        public float Curve;
        public float ForwardSpeedLimit; // max
        public float BackwardSpeedLimit; // max
        public float MaxSpeedLimit => Mathf.Max(ForwardSpeedLimit, BackwardSpeedLimit);
        public Flags m_flags;

        const int CUSTOM_FLAG_SHIFT = 24;
        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;
        public ref NetSegment Segment => ref SegmentID.ToSegment();

        #region cache
        public uint[] LaneIDs;
        public NetInfoExtionsion.Net NetInfoExt;
        private uint renderCount_;
        #endregion

        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(ushort segmentID) => SegmentID = segmentID;

        [Flags]
        public enum Flags {
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
            [CustomFlag] Custom7 = 1 << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,
        }

        public ref NetSegmentEnd Start => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, true);
        public ref NetSegmentEnd End => ref NetworkExtensionManager.Instance.GetSegmentEnd(SegmentID, false);

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

        public void UpdateAllFlags() {
            try {
                NetInfoExt = null;
                LaneIDs = new uint[0];
                renderCount_ = 0;
                if(!NetUtil.IsSegmentValid(SegmentID)) {
                    if(SegmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Created))
                        Log.Debug("Skip updating invalid segment:" + SegmentID);
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
                    if(lane.LaneInfo.m_laneType.IsFlagSet(SpeedLimitManager.LANE_TYPES) &&
                       lane.LaneInfo.m_vehicleType.IsFlagSet(SpeedLimitManager.VEHICLE_TYPES)) {
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

                renderCount_ = CalculateTrackRenderCount();

                if(Log.VERBOSE) Log.Debug($"NetSegmentExt.UpdateAllFlags() succeeded for {this}" /*Environment.StackTrace*/, false);
            } catch(Exception ex) {
                Log.Exception(
                    ex,
                    $"failed to update segment:{SegmentID} info:{SegmentID.ToSegment().Info} " +
                    $"startNode:{Start.NodeID} endNode:{End.NodeID}",
                    showErrorOnce_);
                showErrorOnce_ = false;
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
            if(!SegmentID.ToSegment().IsValid()) {
                return;
            }
            NetInfo info = SegmentID.ToSegment().Info;
            if(!cameraInfo.Intersect(Segment.m_bounds)) {
                return;
            }
            if((layerMask & (info.m_netLayers | info.m_propLayers)) == 0) {
                return;
            }
            if(GetOrCalculateTrackInstance(out var renderInstanceIndex)) {
                var renderData = RenderManager.instance.m_instances[renderInstanceIndex];
                if(renderData.m_dirty) {
                    renderData.m_dirty = false;
                    RefreshRenderData(renderInstanceIndex);
                }
                RenderTrackInstance(cameraInfo, layerMask, renderInstanceIndex);
            }
        }

        public bool GetOrCalculateTrackInstance(out uint renderInstanceIndex) {
            renderInstanceIndex = TrackManager.INVALID_RENDER_INDEX;
            var count = renderCount_;
            if(count == 0)
                return false;
            if(!RequireTrackInstance(count, out renderInstanceIndex))
                return false;
            
            var renderData = RenderManager.instance.m_instances[renderInstanceIndex];
            if(renderData.m_dirty) {
                renderData.m_dirty = false;
                RefreshRenderData(renderInstanceIndex);
            }
            return true;
        }
    
        public uint CalculateTrackRenderCount() => (uint)(NetInfoExt?.TrackLaneCount ?? 0);

        public bool RequireTrackInstance(uint count, out uint instanceIndex) =>
            Singleton<RenderManager>.instance.RequireInstance(TrackManager.TRACK_HOLDER_SEGMNET + SegmentID, count, out instanceIndex);

#if DUMMY_CODE
        // copied from NetSegmnet.RenderInstance where render data is dirty
        // used as if I want to refresh segment data here.
        // will be modified for track code.
        private void RefreshSegmentData(uint renderInstanceIndex) {
            ref var renderData = ref RenderManager.instance.m_instances[renderInstanceIndex];
            var info = Segment.Info;
            ref NetNode startNode = ref Segment.m_startNode.ToNode();
            ref NetNode endNode = ref Segment.m_endNode.ToNode();
            Vector3 startPos = startNode.m_position;
            Vector3 endPos = endNode.m_position;
            renderData.m_position = (startPos + endPos) * 0.5f;
            renderData.m_rotation = Quaternion.identity;
            renderData.m_dataColor0 = info.m_color;
            renderData.m_dataColor0.a = 0f;
            renderData.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(renderData.m_position);
            renderData.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
            Vector4 colorLocationStart = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + SegmentID);
            Vector4 colorLocationEnd = colorLocationStart;
            if(NetNode.BlendJunction(Segment.m_startNode)) {
                colorLocationStart = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + Segment.m_startNode);
            }
            if(NetNode.BlendJunction(Segment.m_endNode)) {
                colorLocationEnd = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + Segment.m_endNode);
            }
            renderData.m_dataVector3 = new Vector4(colorLocationStart.x, colorLocationStart.y, colorLocationEnd.x, colorLocationEnd.y);
            if(info.m_segments == null || info.m_segments.Length == 0) {
                if(info.m_lanes != null) {
                    bool invert = Segment.IsInvert();
                    float startAngle = Segment.m_cornerAngleStart * (Mathf.PI / 128f);
                    float endAngle = Segment.m_cornerAngleEnd * (Mathf.PI / 128f);
                    int propIndex = 0;
                    uint laneID = Segment.m_lanes;
                    for(int laneIndex = 0; laneIndex < info.m_lanes.Length; laneIndex++) {
                        if(laneID == 0) 
                            break;
                        laneID.ToLane().RefreshInstance(laneID, info.m_lanes[laneIndex], startAngle, endAngle, invert, ref renderData, ref propIndex);
                        laneID = laneID.ToLane().m_nextLane;
                    }
                }
            } else {
                float vScale = SegmentID.ToSegment().Info.m_netAI.GetVScale();
                ref var StartLeft = ref Start.Corner.Left;
                ref var StartRight = ref Start.Corner.Right;
                ref var EndLeft = ref End.Corner.Left;
                ref var EndRight = ref End.Corner.Right;
                bool smoothStart = Start.Corner.smooth;
                bool smoothEnd = End.Corner.smooth;
                NetSegment.CalculateMiddlePoints(StartLeft.Position, StartLeft.Direction, EndRight.Position, EndRight.Direction, smoothStart, smoothEnd, out var b1, out var c1);
                NetSegment.CalculateMiddlePoints(StartRight.Position, StartRight.Direction, EndLeft.Position, EndLeft.Direction, smoothStart, smoothEnd, out var b2, out var c2);
                renderData.m_dataMatrix0 = NetSegment.CalculateControlMatrix(
                    StartLeft.Position, b1, c1, EndRight.Position,
                    StartRight.Position, b2, c2, EndLeft.Position, renderData.m_position, vScale);
                renderData.m_dataMatrix1 = NetSegment.CalculateControlMatrix(
                    StartRight.Position, b2, c2, EndLeft.Position,
                    StartLeft.Position, b1, c1, EndRight.Position, renderData.m_position, vScale);
            }
        }
#endif
        private void RefreshRenderData(uint renderInstanceIndex) {
            var renderInstances = RenderManager.instance.m_instances;
            var laneMask = NetInfoExt.TrackLanes;
            for(int laneIndex = 0; laneIndex < LaneIDs.Length; ++laneIndex) {
                var laneBit = 1ul << laneIndex;
                if((laneBit & laneMask) != 0) {
                    var laneID = LaneIDs[laneIndex];
                    ref var laneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
                    ref var renderData = ref renderInstances[renderInstanceIndex];
                    laneExt.RefreshRenderData(ref renderData);
                    renderInstanceIndex = renderData.m_nextInstance;
                }
            }
        }



        private void RenderTrackInstance(RenderManager.CameraInfo cameraInfo, int layerMask, uint renderInstanceIndex) {
            try {
                var renderInstances = RenderManager.instance.m_instances;
                do {
                    ref var renderData = ref renderInstances[renderInstanceIndex];

                    int laneIndex = renderData.m_dataInt0;
                    var laneID = LaneIDs[laneIndex];
                    ref var laneExt = ref NetworkExtensionManager.Instance.LaneBuffer[laneID];
                    laneExt.RenderTrackInstance(cameraInfo, layerMask, ref renderData);

                    renderInstanceIndex = renderData.m_nextInstance;
                } while(renderInstanceIndex != TrackManager.INVALID_RENDER_INDEX);
            } catch(Exception ex) {
                ex.Log($"failed to render {SegmentID}", false);
            }
        }
#endregion
    }

    public struct NetSegmentEnd {
        [Flags]
        public enum Flags : Int64{
            None = 0,

            [Hide]
            [Hint("[Obsolete] " + HintExtension.VANILLA)]
            Vanilla = 1 << 0,            // priority signs
            [Hint("checks if TMPE rules requires vehicles to yield to upcoming traffic\n" +
                "different than the vanilla YieldStart/YieldEnd (Stop) flag.")]
            Yield = 1 << 4,

            [Hint("checks if TMPE rules requires vehicles to Stop at junctions\n" +
                "different than the vanilla YieldStart/YieldEnd (Stop) flag.")]
            Stop = 1 << 5,

            [Hint("checks if TMPE rules gives priority to vehicles on this segment-end")]
            PriorityMain = 1 << 6,

            // junction restrictions.
            [Hint("TMPE allows pedestrian to cross.")]
            ZebraCrossing = 1 << 7,

            [Hint("TMPE bans vehicles from entering blocked junction (requires them to keep clear)")]
            KeepClear = 1 << 8,

            [Hint("vehicles can take near turn (right turn with Right Hand Traffic) even when traffic light is red")]
            NearTurnAtRed = 1 << 9,

            [Hint("in a one-way road vehicles can take far turn even when traffic light is red\n" +
                "far turn is left turn with right hand traffic")]
            FarTurnAtRed = 1 << 10,

            [Hint("vehicles can change lanes in the middle of the junction")]
            LaneChangingGoingStraight = 1 << 11,

            [Hint("cars can make a U-turn at this segment-end\n" +
                "(provided that there is a lane with left lane arrow)")]
            Uturn = 1 << 12,

            // directions
            [Hint("there is a segment to the right (regardless of lane arrows or segment's direction)")]
            HasRightSegment = 1 << 13,

            [Hint("TMPE lane arrow manager allows at least one lane to turn right")]
            CanTurnRight = 1 << 14,

            [Hint("there is a segment to the left (regardless of lane arrows or segment's direction)")]
            HasLeftSegment = 1 << 15,

            [Hint("TMPE lane arrow manager allows at least one lane to turn left")]
            CanTurnLeft = 1 << 16,

            [Hint("TMPE lane arrow manager allows at least one lane to go straight")]
            HasForwardSegment = 1 << 17,
            CanGoForward = 1 << 18,

            [Hint("the start node from which road is placed (does not take into account StartNode/LHT/Invert)")]
            IsStartNode = 1 << 19,

            [Hint("traffic drives from tail node to head node (takes into account StartNode/LHT/Invert)")]
            IsTailNode = 1 << 20,

            [Hide]
            [Hint("[Obsolete] the junction only has two segments.\n")]
            [Obsolete("moved to node")]
            TwoSegments = 1 << 21,

            [Hide]
            [Hint("[Obsolete] the junction has segments with different speed limits.\n")]
            [Obsolete("moved to node")]
            SpeedChange = 1 << 22,

            [Hint("next segment has more lanes (only valid when there are two segments)")]
            LanesIncrase = 1L << 32,

            [Hint("next segment has more lanes (only valid when there are two segments)")]
            LanesDecrease = 1L << 33,

            [CustomFlag] Custom0 = 1 << 24,
            [CustomFlag] Custom1 = 1 << 25,
            [CustomFlag] Custom2 = 1 << 26,
            [CustomFlag] Custom3 = 1 << 27,
            [CustomFlag] Custom4 = 1 << 28,
            [CustomFlag] Custom5 = 1 << 29,
            [CustomFlag] Custom6 = 1 << 30,
            [CustomFlag] Custom7 = 1L << 31,
            CustomsMask = Custom0 | Custom1 | Custom2 | Custom3 | Custom4 | Custom5 | Custom6 | Custom7,
        }

        public Flags m_flags;
        public ushort SegmentID;
        public bool StartNode;

        public ushort NodeID => SegmentID.ToSegment().GetNode(StartNode);
        public NetSegmentExt[] Segments => NodeID.ToNode().IterateSegments()
            .Select(_segmentId => NetworkExtensionManager.Instance.SegmentBuffer[_segmentId]).ToArray();

        const int CUSTOM_FLAG_SHIFT = 24;
        public bool IsEmpty => (m_flags & Flags.CustomsMask) == Flags.None;
        public void Serialize(SimpleDataSerializer s) => s.WriteInt32(
            ((int)(Flags.CustomsMask & m_flags)) >> CUSTOM_FLAG_SHIFT);
        public void Deserialize(SimpleDataSerializer s) => m_flags =
            m_flags.SetMaskedFlags((Flags)(s.ReadInt32() << CUSTOM_FLAG_SHIFT), Flags.CustomsMask);

        public void Init(ushort segmentID, bool startNode) {
            m_flags = Flags.None;
            SegmentID = segmentID;
            StartNode = startNode;
        }

        public static JunctionRestrictionsManager JRMan => JunctionRestrictionsManager.Instance;
        public static TrafficPriorityManager PMan => TrafficPriorityManager.Instance;

        public void UpdateFlags() {
            var flags = m_flags;

            if(PMan != null) {
                PriorityType p = PMan.GetPrioritySign(SegmentID, StartNode);
                flags = flags.SetFlags(Flags.Yield, p == PriorityType.Yield);
                flags = flags.SetFlags(Flags.Stop, p == PriorityType.Stop);
                flags = flags.SetFlags(Flags.PriorityMain, p == PriorityType.Main);
            }

            if(JRMan != null) {
                flags = flags.SetFlags(Flags.KeepClear, !JRMan.IsEnteringBlockedJunctionAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.ZebraCrossing, JRMan.IsPedestrianCrossingAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.NearTurnAtRed, JRMan.IsNearTurnOnRedAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.FarTurnAtRed, JRMan.IsFarTurnOnRedAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.Uturn, JRMan.IsUturnAllowed(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.LaneChangingGoingStraight, JRMan.IsLaneChangingAllowedWhenGoingStraight(SegmentID, StartNode));
                flags = flags.SetFlags(Flags.LaneChangingGoingStraight, JRMan.IsLaneChangingAllowedWhenGoingStraight(SegmentID, StartNode));
            }
            flags = flags.SetFlags(Flags.IsStartNode, StartNode);
            flags = flags.SetFlags(Flags.IsTailNode, NetUtil.GetTailNode(SegmentID) == NodeID);

            bool speedChange = TMPEHelpers.SpeedChanges(NodeID);
            bool twoSegments = NodeID.ToNode().CountSegments() == 2;

            flags = flags.SetFlags(Flags.SpeedChange, speedChange);
            flags = flags.SetFlags(Flags.TwoSegments, twoSegments);

            bool lanesIncrease = false, lanesDecrease = false;
            if (twoSegments) {
                var sourceLanes = new LaneDataIterator(
                    SegmentID,
                    StartNode, // going toward node:NodeID
                    LaneArrowManager.LANE_TYPES,
                    LaneArrowManager.VEHICLE_TYPES);
                var segmentID2 = NodeID.ToNode().GetAnotherSegment(SegmentID);
                bool startNode2 = segmentID2.ToSegment().IsStartNode(NodeID);
                var targetLanes = new LaneDataIterator(
                    segmentID2,
                    !startNode2, // lanes that are going away from node:NodeID
                    LaneArrowManager.LANE_TYPES,
                    LaneArrowManager.VEHICLE_TYPES);
                int nSource = sourceLanes.Count;
                int nTarget = targetLanes.Count;
                lanesIncrease = nTarget > nSource;
                lanesDecrease = nTarget < nSource;
            }
            flags = flags.SetFlags(Flags.LanesIncrase, lanesIncrease);
            flags = flags.SetFlags(Flags.LanesDecrease, lanesDecrease);

            m_flags = flags;
        }

        public override string ToString() {
            return $"NetSegmentEnd(segment:{SegmentID} node:{NodeID} StartNode:{StartNode} flags={m_flags})";
        }

        public void UpdateDirections() {
            CheckSegmentsInEachDirection(
                segmentId: SegmentID, nodeId: NodeID,
                right: out bool right, forward: out bool forward, left: out bool left);
            m_flags = m_flags.SetFlags(Flags.HasRightSegment, right);
            m_flags = m_flags.SetFlags(Flags.HasLeftSegment, left);
            m_flags = m_flags.SetFlags(Flags.HasForwardSegment, forward);

            LaneArrows arrows = AllArrows(SegmentID, StartNode);
            m_flags = m_flags.SetFlags(Flags.CanGoForward, arrows.IsFlagSet(LaneArrows.Forward));
            m_flags = m_flags.SetFlags(Flags.CanTurnRight, arrows.IsFlagSet(LaneArrows.Right));
            m_flags = m_flags.SetFlags(Flags.CanTurnLeft, arrows.IsFlagSet(LaneArrows.Left));
        }

        private static LaneArrows AllArrows(ushort segmentId, bool startNode) {
            LaneArrows ret = LaneArrows.None;
            foreach(var lane in NetUtil.IterateLanes(
                segmentId: segmentId, startNode: startNode,
                laneType: LaneArrowManager.LANE_TYPES, vehicleType: LaneArrowManager.VEHICLE_TYPES)) {
                LaneArrows arrows = LaneArrowManager.Instance.GetFinalLaneArrows(lane.LaneID);
                ret |= arrows;
            }
            return ret;
        }

        private static void CheckSegmentsInEachDirection(
            ushort segmentId, ushort nodeId,
            out bool right, out bool forward, out bool left) {
            bool startNode = segmentId.ToSegment().m_startNode == nodeId;
            IExtSegmentEndManager segEndMan = Constants.ManagerFactory.ExtSegmentEndManager;
            ExtSegmentEnd segEnd = segEndMan.ExtSegmentEnds[segEndMan.GetIndex(segmentId, startNode)];

            forward = left = right = false;

            for(int i = 0; i < 8; ++i) {
                ushort otherSegmentId = nodeId.ToNode().GetSegment(i);
                if(otherSegmentId == 0) continue;
                bool isRoad = otherSegmentId.ToSegment().Info.m_netAI is RoadBaseAI;
                if(!isRoad) continue;
                ArrowDirection dir = segEndMan.GetDirection(ref segEnd, otherSegmentId);
                switch(dir) {
                    case ArrowDirection.Forward:
                        forward = true;
                        break;
                    case ArrowDirection.Right:
                        right = true;
                        break;
                    case ArrowDirection.Left:
                        left = true;
                        break;
                    default:
                        break;
                        //throw new Exception("Unreachable Code. dir = " + dir);
                } //end switch
            } // end for
        } // end method

#region cache corners
        public CornerPairData Corner;
        public void UpdateCorners() {
            ref var segment = ref SegmentID.ToSegment();
            segment.CalculateCorner(SegmentID, heightOffset: true, start: StartNode, leftSide: true, out Corner.Left.Position, out Corner.Left.Direction, out Corner.smooth);
            segment.CalculateCorner(SegmentID, heightOffset: true, start: StartNode, leftSide: false, out Corner.Right.Position, out Corner.Right.Direction, out Corner.smooth);
        }

#endregion

    }
}

