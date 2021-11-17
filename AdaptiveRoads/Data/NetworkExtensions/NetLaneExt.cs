namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using ColossalFramework.Math;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using TrafficManager;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Enums;
    using TrafficManager.Manager.Impl;
    using UnityEngine;
    using Log = KianCommons.Log;

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
        //public CornerTripleData A, D;
        public Bezier3 Left, Right; // left and right is WRT the direction of the bezier
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
            CornerTripleData A = default, D = default;
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
            ref var segmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[LaneData.SegmentID];
            var infoExt = segmentExt.NetInfoExt;
            var netManager = Singleton<NetManager>.instance;
            ref var segment = ref LaneData.Segment;
            foreach(var track in infoExt.Tracks) {
                if((track.LaneIndeces & laneBit) != 0 && track.CheckSegmentFlags(segmentExt.m_flags, segment.m_flags)) {
                    Vector4 dataVector3 = renderData.m_dataVector3;
                    if(track.m_requireWindSpeed)
                        dataVector3.w = renderData.m_dataFloat0;

                    Vector4 dataVector0 = renderData.m_dataVector0;
                    bool turnAround = LaneData.LaneInfo.IsGoingBackward(); // TODO is this logic sufficient? is this line even necessary?
                    if(turnAround) {
                        dataVector0.x = -dataVector0.x;
                        dataVector0.y = -dataVector0.y;
                    }
                    if(cameraInfo.CheckRenderDistance(renderData.m_position, track.m_lodRenderDistance)) {
                        netManager.m_materialBlock.Clear();
                        netManager.m_materialBlock.SetMatrix(netManager.ID_LeftMatrix, renderData.m_dataMatrix0);
                        netManager.m_materialBlock.SetMatrix(netManager.ID_RightMatrix, renderData.m_dataMatrix1);
                        netManager.m_materialBlock.SetVector(netManager.ID_MeshScale, dataVector0);
                        netManager.m_materialBlock.SetVector(netManager.ID_ObjectIndex, dataVector3);
                        netManager.m_materialBlock.SetColor(netManager.ID_Color, renderData.m_dataColor0);
                        NetManager.instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(track.m_trackMesh, renderData.m_position, renderData.m_rotation, track.m_trackMaterial, track.m_layer, null, 0, netManager.m_materialBlock);
                    } else {
                        NetInfo.LodValue combinedLod = track.m_combinedLod;
                        if(combinedLod == null) continue;

                        combinedLod.m_leftMatrices[combinedLod.m_lodCount] = renderData.m_dataMatrix0;
                        combinedLod.m_rightMatrices[combinedLod.m_lodCount] = renderData.m_dataMatrix1;
                        combinedLod.m_meshScales[combinedLod.m_lodCount] = dataVector0;
                        combinedLod.m_objectIndices[combinedLod.m_lodCount] = dataVector3;
                        combinedLod.m_meshLocations[combinedLod.m_lodCount] = renderData.m_position;
                        combinedLod.m_lodMin = new Vector3(100000f, 100000f, 100000f);
                        combinedLod.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
                        //combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, renderData.m_position);
                        //combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, renderData.m_position);
                        if(++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
                            NetSegment.RenderLod(cameraInfo, combinedLod);
                        }
                    }
                }
            }
        }

        public void RefreshRenderData(ref RenderManager.Instance renderData) {
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

        public bool CalculateGroupData(int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            var info = LaneData.Segment.Info;
            if((layer & info.m_netLayers) == 0)
                return false;
            bool result = false;
            ref var segmentExt = ref LaneData.SegmentID.ToSegmentExt();
            var infoExt = segmentExt.NetInfoExt;
            if(infoExt.TrackLaneCount == 0)
                return false;
            ref var segment = ref LaneData.Segment;

            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.HasTrackLane(LaneData.LaneIndex) && trackInfo.CheckSegmentFlags(segmentExt.m_flags, segment.m_flags)) {
                    if(trackInfo.m_combinedLod != null) {
                        var tempSegmentInfo = NetInfoExtionsion.Net.TempSegmentInfo(trackInfo);
                        NetSegment.CalculateGroupData(tempSegmentInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                        result = true;
                    }
                }
            }

            return result;
        }
        public static bool CalculateGroupData(RenderGroup.MeshData meshData, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            vertexCount += meshData.m_vertices.Length;
            triangleCount += meshData.m_triangles.Length;
            objectCount++;
            vertexArrays |= meshData.VertexArrayMask() | RenderGroup.VertexArrays.Colors | RenderGroup.VertexArrays.Uvs2 | RenderGroup.VertexArrays.Uvs4;
            return true;
        }

        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData meshData) {
            var info = LaneData.Segment.Info;
            if((layer & info.m_netLayers) == 0)
                return;
            ref var segmentExt = ref LaneData.SegmentID.ToSegmentExt();
            var infoExt = segmentExt.NetInfoExt;
            if(infoExt.TrackLaneCount == 0)
                return;
            ref var segment = ref LaneData.Segment;

            RenderManager.Instance renderData = default;
            RefreshRenderData(ref renderData);

            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.HasTrackLane(LaneData.LaneIndex) && trackInfo.CheckSegmentFlags(segmentExt.m_flags, segment.m_flags)) {
                    if(trackInfo.m_combinedLod != null) {
                        var tempSegmentInfo = NetInfoExtionsion.Net.TempSegmentInfo(trackInfo);
                        bool _ = false;
                        NetSegment.PopulateGroupData(
                            info, tempSegmentInfo,
                            leftMatrix: renderData.m_dataMatrix0, rightMatrix: renderData.m_dataMatrix1,
                            meshScale: renderData.m_dataVector0, objectIndex: renderData.m_dataVector3,
                            ref vertexIndex, ref triangleIndex, groupPosition, meshData, ref _);
                    }
                }
            }
        }
        #endregion
    }
}

