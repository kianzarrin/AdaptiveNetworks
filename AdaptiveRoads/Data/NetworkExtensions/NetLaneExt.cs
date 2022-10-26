namespace AdaptiveRoads.Manager{
    using AdaptiveRoads.Data.NetworkExtensions;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using TrafficManager.API.Traffic.Enums;
    using UnityEngine;
    using Log = KianCommons.Log;
    using AdaptiveRoads.CustomScript;
    using static AdaptiveRoads.Util.Shortcuts;
    using KianCommons.Math;
    using UnityEngine.Networking.Types;
    using ColossalFramework.Math;

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

            [Hint("lane has lane connections (car/track, outgoing/incoming/dead-end)")]
            LaneConnections = 1 << 18,

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
            this = default;
            LaneData.LaneID = laneID;
        }


        // pass in segmentID for the sake of MOM lane problem.
        public void UpdateLane(LaneData lane, ushort segmentID) {
            Assertion.AssertEqual(LaneData.LaneID, lane.LaneID, "lane id");
            if(lane.Lane.m_segment != 0 && lane.Lane.m_segment != segmentID)
                Log.Error($"lane segment mismatch: {LaneData} parentSegment:{segmentID}");
            lane.Lane.m_segment = segmentID; // fix MOM lane issue

            try {
                LaneData = lane;

                bool parkingAllowed = LaneData.LaneInfo.m_laneType == NetInfo.LaneType.Parking;
                if(ParkingMan != null)
                    parkingAllowed &= ParkingMan.IsParkingAllowed(LaneData.SegmentID, LaneData.LaneInfo.m_finalDirection);
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
                m_flags = m_flags.SetFlags(Flags.LaneConnections, lane.LaneID.HasAnyConnections());
                SpeedLimit = lane.GetLaneSpeedLimit();

                UpdateCorners();
                if(Log.VERBOSE) Log.Succeeded(ToString());
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
            // TODO: only update corners when NetSegment.UpdateLanes is called. not when traffic rules change.
            // TODO: only update corners for AN networks
            ushort segmentId = LaneData.SegmentID;
            ref NetSegment segment = ref segmentId.ToSegment();
            ref var segmentExt = ref segmentId.ToSegmentExt();
            float width = LaneData.LaneInfo.m_width;
            ref Bezier3 bezier = ref LaneData.Lane.m_bezier;

            TiltData tiltData = new TiltData(
                startAngle: segmentExt.Start.TotalAngle,
                startVelocity: segmentExt.Start.GetAngleVelocity(),
                endAngle: -segmentExt.End.TotalAngle,
                endVelocity: -segmentExt.End.GetAngleVelocity());

            OutLine = new OutlineData(bezier, width, tiltData);
            RenderData = GenerateRenderData(ref OutLine);

            tiltData.wireHeight = segmentExt.NetInfoExt?.CatenaryHeight ?? 0;
            WireOutLine = new OutlineData(bezier, width: width, tiltData);
            WireRenderData = GenerateRenderData(ref WireOutLine);
        }

        public TrackRenderData GenerateRenderData(ref OutlineData outline, Vector3? pos = null) {
            TrackRenderData ret = default;
            ref var segment = ref LaneData.Segment;
            NetInfo netInfo = segment.Info;
            ref NetNode startNode = ref segment.m_startNode.ToNode();
            ref NetNode endNode = ref segment.m_endNode.ToNode();
            ref var bezier = ref LaneData.Lane.m_bezier;
            ref var lane = ref LaneData.Lane;
            var laneInfo = LaneData.LaneInfo;
            Vector3 startPos = bezier.a;
            Vector3 endPos = bezier.d;

            ret.Position = pos ?? (startPos + endPos) * 0.5f;
            ret.Color = netInfo.m_color;
            ret.Color.a = 0f;
            ret.WindSpeed = Singleton<WeatherManager>.instance.GetWindSpeed(ret.Position);
            ret.MeshScale = new Vector4(1f / laneInfo.m_width, 1f / netInfo.m_segmentLength, 1f, 1f);

            Vector4 colorLocationStart = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + LaneData.SegmentID);
            Vector4 colorLocationEnd = colorLocationStart;
            if(NetNode.BlendJunction(segment.m_startNode)) {
                colorLocationStart = RenderManager.GetColorLocation(TrackManager.NODE_HOLDER + segment.m_startNode);
            }
            if(NetNode.BlendJunction(segment.m_endNode)) {
                colorLocationEnd = RenderManager.GetColorLocation(TrackManager.NODE_HOLDER + segment.m_endNode);
            }
            ret.ObjectIndex = new Vector4(colorLocationStart.x, colorLocationStart.y, colorLocationEnd.x, colorLocationEnd.y); // object index
            float vScale = netInfo.m_netAI.GetVScale();
            ret.TurnAround = LaneData.LaneInfo.IsGoingBackward(); // TODO is this logic sufficient?
            ret.TurnAround ^= LaneData.Segment.IsInvert();
            ret.CalculateControlMatrix(outline, vScale);


            ret.CalculateMapping(netInfo);
            return ret;
        }

        private bool Check(NetInfoExtionsion.Track trackInfo) {
            ref var segmentExt = ref LaneData.SegmentID.ToSegmentExt();
            ref var segment = ref LaneData.Segment;
            ref var lane = ref LaneData.Lane;
            return
                trackInfo.HasTrackLane(LaneData.LaneIndex) &&
                trackInfo.CheckSegmentFlags(segmentExt.m_flags,
                segment.m_flags, this.m_flags, lane.Flags(),
                segmentExt.UserData);
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

        public bool CalculateGroupData(int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            var infoExt = LaneData.SegmentID.ToSegmentExt().NetInfoExt;
            if(infoExt == null || infoExt.TrackLaneCount == 0)
                return false;

            bool result = false;
            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.m_layer == layer && Check(trackInfo)) {
                    result |= RenderData.CalculateGroupData(trackInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                }
            }
            return result;
        }

        public void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData meshData) {
            var infoExt = LaneData.SegmentID.ToSegmentExt().NetInfoExt;
            if(infoExt == null || infoExt.TrackLaneCount == 0)
                return;

            var renderData0 = GenerateRenderData(ref OutLine, groupPosition);
            var wireRenderData0 = GenerateRenderData(ref WireOutLine, groupPosition);
            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.m_layer == layer && Check(trackInfo)) {
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

        public static void Render(NetInfo info, int laneIndex, NetSegment.Flags flags, in OutlineData segmentOutline, bool smoothStart, bool smoothEnd) {
            var infoExt = info?.GetMetaData();
            if(infoExt == null || infoExt.TrackLaneCount == 0)
                return;
            var laneInfo = info.m_lanes[laneIndex];

            var posStartLeft = segmentOutline.Left.a;
            var posStartRight = segmentOutline.Right.a;
            var posEndLeft = segmentOutline.Left.d;
            var posEndRight = segmentOutline.Right.d;

            float posNormalized = laneInfo.m_position / (info.m_halfWidth * 2f) + 0.5f;
            if(flags.IsFlagSet(NetSegment.Flags.Invert)) posNormalized = 1f - posNormalized;
            
            Vector3 a = posStartLeft + (posStartRight - posStartLeft) * posNormalized;
            Vector3 d = posEndLeft + (posEndRight - posEndLeft) * posNormalized;
            Vector3 startDir = segmentOutline.Center.DirA().normalized;
            Vector3 endDir = segmentOutline.Center.DirA().normalized;
            a.y += laneInfo.m_verticalOffset;
            d.y += laneInfo.m_verticalOffset;
            //NetSegment.CalculateMiddlePoints(a, startDir, d, endDir, smoothStart, smoothEnd, out var b, out var c);
            //var bezier = new Bezier3(a, b, c, d);

            var laneOutline = new OutlineData(a, d, startDir, endDir, laneInfo.m_width, smoothStart, smoothEnd);

            TrackRenderData renderData = default;
            renderData.Position = (laneOutline.Center.a + laneOutline.Center.d) * 0.5f;
            renderData.Color = info.m_color;
            renderData.Color.a = 0f;
            renderData.WindSpeed = Singleton<WeatherManager>.instance.GetWindSpeed(renderData.Position); 
            renderData.MeshScale = new Vector4(1f / laneInfo.m_width, 1f / info.m_segmentLength, 1f, 1f);

            renderData.ObjectIndex = RenderManager.DefaultColorLocation;
            float vScale = info.m_netAI.GetVScale();
            renderData.TurnAround = laneInfo.IsGoingBackward();
            renderData.CalculateControlMatrix(laneOutline, vScale);


            renderData.CalculateMapping(info);

            foreach(var trackInfo in infoExt.Tracks) {
                if(trackInfo.HasTrackLane(laneIndex) && trackInfo.CheckSegmentFlags(default, flags, default, default, default)) {
                    var renderData2 = renderData.GetDataFor(trackInfo);
                    renderData2.RenderInstance(trackInfo, null);
                }
            }
        }
        #endregion
    }
}

