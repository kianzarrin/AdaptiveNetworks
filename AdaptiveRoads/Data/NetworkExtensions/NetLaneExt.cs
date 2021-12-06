namespace AdaptiveRoads.Manager{
    using AdaptiveRoads.Data.NetworkExtensions;
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
    using AdaptiveRoads.CustomScript;

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

            RightSlight = 1L << 35,
            RightModerate = 1L << 36,
            RightSharp = 1L << 37,

            UTurn = 1L << 38,
            AllDirections = LeftSlight | LeftModerate | LeftSharp | RightSlight | RightModerate | RightSharp | UTurn,

            [ExpressionFlag] Expression0 = 1L << 39,
            [ExpressionFlag] Expression1 = 1L << 40,
            [ExpressionFlag] Expression2 = 1L << 41,
            [ExpressionFlag] Expression3 = 1L << 42,
            [ExpressionFlag] Expression4 = 1L << 43,
            [ExpressionFlag] Expression5 = 1L << 44,
            [ExpressionFlag] Expression6 = 1L << 45,
            [ExpressionFlag] Expression7 = 1L << 46,
            ExpressionMask = Expression0 | Expression1 | Expression2 | Expression3 | Expression4 | Expression5 | Expression6 | Expression7,

        }

        public LaneData LaneData;
        public Flags m_flags;
        public float SpeedLimit; // game speed limit 1=50kph 20=unlimited

        //public object OuterMarking;
        //public object InnerMarking;


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
        public void UpdateScriptedFlags() {
            try {
                var net = LaneData.Segment.Info?.GetMetaData();
                if (net == null) return;
                foreach (var scriptedFlag in Flags.ExpressionMask.ExtractPow2Flags()) {
                    bool condition = false;
                    if (net.ScriptedFlags.TryGetValue(scriptedFlag, out var expression)) {
                        condition = expression.Condition(segmentID: LaneData.SegmentID, nodeID: 0, laneIndex: LaneData.LaneIndex);
                    }
                    m_flags = m_flags.SetFlags(scriptedFlag, condition);
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }
        public override string ToString() {
            return $"NetLaneExt({LaneData} flags={m_flags} speed={SpeedLimit})";
        }

        #region track
        public OutlineData OutLine;
        public TrackRenderData RenderData;
        public OutlineData WireOutLine;
        public TrackRenderData WireRenderData;

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

            OutLine = new OutlineData(
                a, d, startDir, endDir, laneInfo.m_width,
                smoothStart, smoothEnd,
                segmentExt.Start.TotalAngle, -segmentExt.End.TotalAngle, wireHeight: 0);

            RenderData = GenerateRenderData(ref OutLine);

            WireOutLine = new OutlineData(
                a, d, startDir, endDir, laneInfo.m_width,
                smoothStart, smoothEnd,
                segmentExt.Start.TotalAngle, -segmentExt.End.TotalAngle, wireHeight: segmentExt.NetInfoExt?.CatenaryHeight ?? 0);

            WireRenderData = GenerateRenderData(ref WireOutLine);
        }

        public TrackRenderData GenerateRenderData(ref OutlineData outline, Vector3? pos = null) {
            TrackRenderData ret = default;
            ref var segment = ref LaneData.Segment;
            var info = segment.Info;
            ref NetNode startNode = ref segment.m_startNode.ToNode();
            ref NetNode endNode = ref segment.m_endNode.ToNode();
            ref var bezier = ref LaneData.Lane.m_bezier;
            ref var lane = ref LaneData.Lane;
            var laneInfo = LaneData.LaneInfo;
            Vector3 startPos = bezier.a;
            Vector3 endPos = bezier.d;

            ret.Position = pos ?? (startPos + endPos) * 0.5f;
            ret.Color = info.m_color;
            ret.Color.a = 0f;
            ret.WindSpeed = Singleton<WeatherManager>.instance.GetWindSpeed(ret.Position);
            ret.MeshScale = new Vector4(1f / laneInfo.m_width, 1f / info.m_segmentLength, 1f, 1f);
            ret.TurnAround = LaneData.LaneInfo.IsGoingBackward(); // TODO is this logic sufficient?
            ret.TurnAround ^= LaneData.Segment.IsInvert();
            if(ret.TurnAround) {
                ret.MeshScale.x *= -1;
                ret.MeshScale.y *= -1;
            }
            Vector4 colorLocationStart = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + LaneData.SegmentID);
            Vector4 colorLocationEnd = colorLocationStart;
            if(NetNode.BlendJunction(segment.m_startNode)) {
                colorLocationStart = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segment.m_startNode);
            }
            if(NetNode.BlendJunction(segment.m_endNode)) {
                colorLocationEnd = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segment.m_endNode);
            }
            ret.ObjectIndex = new Vector4(colorLocationStart.x, colorLocationStart.y, colorLocationEnd.x, colorLocationEnd.y); // object index
            float vScale = info.m_netAI.GetVScale();
            ret.LeftMatrix = NetSegment.CalculateControlMatrix(
                outline.Left.a, outline.Left.b, outline.Left.c, outline.Left.d,
                outline.Right.a, outline.Right.b, outline.Right.c, outline.Right.d,
                ret.Position, vScale);
            ret.RightMatrix = NetSegment.CalculateControlMatrix(
                outline.Right.a, outline.Right.b, outline.Right.c, outline.Right.d,
                outline.Left.a, outline.Left.b, outline.Left.c, outline.Left.d,
                ret.Position, vScale);
            return ret;
        }

        private bool Check(NetInfoExtionsion.Track trackInfo) {
            ref var segmentExt = ref LaneData.SegmentID.ToSegmentExt();
            ref var segment = ref LaneData.Segment;
            return trackInfo.HasTrackLane(LaneData.LaneIndex) &&
                trackInfo.CheckSegmentFlags(segmentExt.m_flags, segment.m_flags, this.m_flags);
        }
        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo) {
            var tracks = LaneData.SegmentID.ToSegmentExt().NetInfoExt.Tracks;
            foreach(var trackInfo in tracks) {
                if(Check(trackInfo)) {
                    TrackRenderData renderData;
                    if(trackInfo.m_requireWindSpeed) {
                        renderData = WireRenderData.GetDataFor(trackInfo);
                    } else {
                        renderData = RenderData.GetDataFor(trackInfo);
                    }
                    renderData.RenderInstance(trackInfo, cameraInfo);
                    TrackManager.instance.EnqueuOverlay(trackInfo, ref OutLine, turnAround: renderData.TurnAround, DC: false);
                }
            }
        }

        public bool CalculateGroupData(ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            var infoExt = LaneData.SegmentID.ToSegmentExt().NetInfoExt;
            if(infoExt == null || infoExt.TrackLaneCount == 0)
                return false;

            bool result = false;
            foreach(var trackInfo in infoExt.Tracks) {
                if(Check(trackInfo)) {
                    result |= RenderData.CalculateGroupData(trackInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                }
            }
            return result;
        }

        public void PopulateGroupData(int groupX, int groupZ, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData meshData) {
            var infoExt = LaneData.SegmentID.ToSegmentExt().NetInfoExt;
            if(infoExt == null || infoExt.TrackLaneCount == 0)
                return;

            var renderData0 = GenerateRenderData(ref OutLine, groupPosition);
            var wireRenderData0 = GenerateRenderData(ref WireOutLine, groupPosition);
            foreach(var trackInfo in infoExt.Tracks) {
                if(Check(trackInfo)) {
                    TrackRenderData renderData;
                    if(trackInfo.m_requireWindSpeed) {
                        renderData = wireRenderData0.GetDataFor(trackInfo);
                    } else {
                        renderData = renderData0.GetDataFor(trackInfo);
                    }
                    renderData.PopulateGroupData(trackInfo, groupX, groupZ, ref vertexIndex, ref triangleIndex, meshData);
                }
            }
        }

        public static void Render(NetInfo info, int laneIndex, NetSegment.Flags flags, OutlineData segmentOutline) {
            var infoExt = info?.GetMetaData();
            if(infoExt == null || infoExt.TrackLaneCount == 0)
                return;
            var laneInfo = info.m_lanes[laneIndex];

            var posStartLeft = segmentOutline.Left.a;
            var posStartRight = segmentOutline.Right.a;
            var posEndLeft = segmentOutline.Left.d;
            var posEndRight = segmentOutline.Right.d;

            var smoothStart = segmentOutline.SmoothA;
            var smoothEnd = segmentOutline.SmoothD;

            float posNormalized = laneInfo.m_position / (info.m_halfWidth * 2f) + 0.5f;
            if(flags.IsFlagSet(NetSegment.Flags.Invert)) posNormalized = 1f - posNormalized;
            
            Vector3 a = posStartLeft + (posStartRight - posStartLeft) * posNormalized;
            Vector3 startDir = segmentOutline.DirA;
            Vector3 d = posEndLeft + (posEndRight- posEndLeft) * posNormalized;
            Vector3 endDir = segmentOutline.DirD;
            a.y += laneInfo.m_verticalOffset;
            d.y += laneInfo.m_verticalOffset;
            //NetSegment.CalculateMiddlePoints(a, startDir, d, endDir, smoothStart, smoothEnd, out var b, out var c);
            //var bezier = new Bezier3(a, b, c, d);

            var laneOutline = new OutlineData(a, d, startDir, endDir, laneInfo.m_width, smoothStart, smoothEnd, 0, 0, 0);

            TrackRenderData renderData = default;
            renderData.Position = (laneOutline.Center.a + laneOutline.Center.d) * 0.5f;
            renderData.Color = info.m_color;
            renderData.Color.a = 0f;
            renderData.WindSpeed = Singleton<WeatherManager>.instance.GetWindSpeed(renderData.Position); 
            renderData.MeshScale = new Vector4(1f / laneInfo.m_width, 1f / info.m_segmentLength, 1f, 1f); 
            bool turnAround = laneInfo.IsGoingBackward();
            if(turnAround) {
                renderData.MeshScale.x *= -1;
                renderData.MeshScale.y *= -1;
            }
            renderData.ObjectIndex = RenderManager.DefaultColorLocation;
            float vScale = info.m_netAI.GetVScale();
            renderData.LeftMatrix = NetSegment.CalculateControlMatrix(
                laneOutline.Left.a, laneOutline.Left.b, laneOutline.Left.c, laneOutline.Left.d,
                laneOutline.Right.a, laneOutline.Right.b, laneOutline.Right.c, laneOutline.Right.d,
                renderData.Position, vScale);
            renderData.RightMatrix = NetSegment.CalculateControlMatrix(
                laneOutline.Right.a, laneOutline.Right.b, laneOutline.Right.c, laneOutline.Right.d,
                laneOutline.Left.a, laneOutline.Left.b, laneOutline.Left.c, laneOutline.Left.d,
                renderData.Position, vScale);

            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.HasTrackLane(laneIndex) && trackInfo.CheckSegmentFlags(default, flags, default)) {
                    var renderData2 = renderData.GetDataFor(trackInfo);
                    renderData2.RenderInstance(trackInfo, null);
                }
            }
        }
        #endregion
    }
}

