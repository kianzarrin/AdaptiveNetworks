namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.Math;
    using KianCommons;
    using UnityEngine;
    using Log = KianCommons.Log;

    public struct LaneTransition {
        public ushort NodeID;
        public uint LaneIDSource; // dominant
        public uint LaneIDTarget;
        public int AntiFlickerIndex;

        public void Init(uint laneID1, uint laneID2, int antiFlickerIndex) {
            AntiFlickerIndex = antiFlickerIndex;
            ushort segmentID1 = laneID1.ToLane().m_segment;
            ushort segmentID2 = laneID2.ToLane().m_segment;
            var info1 = segmentID1.ToSegment().Info;
            var info2 = segmentID2.ToSegment().Info;
            float prio1 = info1.m_netAI.GetNodeInfoPriority(segmentID1, ref segmentID1.ToSegment());
            float prio2 = info2.m_netAI.GetNodeInfoPriority(segmentID2, ref segmentID2.ToSegment());

            var infoExt = info1.GetMetaData();
            var infoExt2 = info2.GetMetaData();
            bool hasTrackLane = infoExt != null && infoExt.HasTrackLane(laneID1.ToLaneExt().LaneData.LaneIndex);
            bool hasTrackLane2 = infoExt2 != null && infoExt2.HasTrackLane(laneID2.ToLaneExt().LaneData.LaneIndex);
            if(!(hasTrackLane || hasTrackLane2)) {
                Log.Warning("neither has track for this lane index");
                return; //empty
            }
            if( (prio1 >= prio2 && hasTrackLane) || !hasTrackLane2) {
                LaneIDSource = laneID1;
                LaneIDTarget = laneID2;
            } else {
                LaneIDSource = laneID2;
                LaneIDTarget = laneID1;
            }

            NodeID = segmentID1.ToSegment().GetSharedNode(segmentID2);

            Calculate();
            if(Log.VERBOSE) Log.Debug($"LaneTransition.Init() succeeded for: {this}", false);
        }

        public OutlineData OutLine;
        public TrackRenderData RenderData;
        public OutlineData WireOutLine;
        public TrackRenderData WireRenderData;

        public override string ToString() => $"LaneTransition[node:{NodeID} lane:{LaneIDSource}->lane:{LaneIDTarget}]";
        #region shortcuts
        ref NetLane LaneA => ref LaneIDSource.ToLane();
        ref NetLane LaneD => ref LaneIDTarget.ToLane();
        ref NetLaneExt LaneExtA => ref LaneIDSource.ToLaneExt();
        ref NetLaneExt LaneExtD => ref LaneIDTarget.ToLaneExt();
        ref NetSegment SegmentA => ref LaneA.m_segment.ToSegment();
        ref NetSegment SegmentD => ref LaneD.m_segment.ToSegment();
        ref NetSegmentExt SegmentExtA => ref LaneA.m_segment.ToSegmentExt();
        ref NetSegmentExt SegmentExtD => ref LaneD.m_segment.ToSegmentExt();
        ushort segmentID_A => LaneA.m_segment;
        ushort segmentID_D => LaneD.m_segment;
        ref NetNode Node => ref NodeID.ToNode();
        ref NetNodeExt NodeExt => ref NodeID.ToNodeExt();
        NetInfo Info => Node.Info;
        NetInfo InfoA => SegmentA.Info;
        NetInfoExtionsion.Net InfoExtA => segmentID_A.ToSegment().Info?.GetMetaData();
        NetInfo.Lane laneInfoA => LaneExtA.LaneData.LaneInfo;
        int laneIndexA => LaneExtA.LaneData.LaneIndex;

        float Width => laneInfoA.m_width;
        #endregion

        public bool Nodeless => OutLine.Empty;

        public void Calculate() {
            Vector3 a, dirA;
            float angleA;
            if(SegmentA.IsStartNode(NodeID)) {
                a = LaneA.m_bezier.a;
                dirA = LaneExtA.OutLine.DirA;

                // the dir is already going away from the node which is against the direction of the bezier at start. so we need - :
                angleA = -segmentID_A.ToSegmentExt().Start.TotalAngle;
            } else {
                a = LaneA.m_bezier.d;
                dirA = LaneExtA.OutLine.DirD;

                // the dir is already going away from the node which is against the direction of the bezier at start. so we need - :
                angleA = -segmentID_A.ToSegmentExt().End.TotalAngle; 
            }

            Vector3 d, dirD;
            float angleD;
            if(SegmentD.IsStartNode(NodeID)) {
                d = LaneD.m_bezier.a;
                dirD = LaneExtD.OutLine.DirA;

                // the dir is already going away from the node which is in the same direction as lane end. so we need + :
                angleD = +segmentID_D.ToSegmentExt().Start.TotalAngle;
            } else {
                d = LaneD.m_bezier.d;
                dirD = LaneExtD.OutLine.DirD;

                // the dir is already going away from the node which is in the same direction as lane end. so we need + :
                angleD = +segmentID_D.ToSegmentExt().End.TotalAngle; 
            }

            OutLine = new OutlineData(a, d, -dirA, -dirD, Width, true, true, angleA, angleD, wire:false);
            if(OutLine.Empty) return;
            RenderData = GenerateRenderData(ref OutLine);

            WireOutLine = new OutlineData(a, d, -dirA, -dirD, Width, true, true, angleA, angleD, wire: true);
            WireRenderData = GenerateRenderData(ref WireOutLine);

        }
        public TrackRenderData GenerateRenderData(ref OutlineData outline, Vector3? pos = null) {
            TrackRenderData ret = default;
            ret.Position = pos ?? (outline.Center.a + outline.Center.d) * 0.5f;

            ret.MeshScale = new Vector4(1f / Width, 1f / InfoA.m_segmentLength, 1f, 1f);
            ret.TurnAround = laneInfoA.m_finalDirection.IsGoingBackward(); // TODO is this logic sufficient? is this line even necessary?
            ret.TurnAround ^= SegmentA.IsInvert();
            if(ret.TurnAround) {
                ret.MeshScale.x *= -1;
                ret.MeshScale.y *= -1;
            }

            float vScale = InfoA.m_netAI.GetVScale();
            ret.LeftMatrix = NetSegment.CalculateControlMatrix(
                outline.Left.a, outline.Left.b, outline.Left.c, outline.Left.d,
                outline.Right.a, outline.Right.b, outline.Right.c, outline.Right.d,
                ret.Position, vScale);
            ret.RightMatrix = NetSegment.CalculateControlMatrix(
                outline.Right.a, outline.Right.b, outline.Right.c, outline.Right.d,
                outline.Left.a, outline.Left.b, outline.Left.c, outline.Left.d,
                ret.Position, vScale);


            ret.WindSpeed = Singleton<WeatherManager>.instance.GetWindSpeed(ret.Position);
            ret.Color = Info.m_color;
            ret.Color.a = 0;
            Vector4 colorLocationA;
            Vector4 colorLocationD;
            if(NetNode.BlendJunction(NodeID)) {
                colorLocationD = colorLocationA = RenderManager.GetColorLocation(TrackManager.NODE_HOLDER + NodeID);
            } else {
                colorLocationA = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segmentID_A);
                colorLocationD = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segmentID_D);
            }
            ret.ObjectIndex = new Vector4(colorLocationA.x, colorLocationA.y, colorLocationD.x, colorLocationD.y);
            return ret;
        }

        private bool Check(NetInfoExtionsion.Track trackInfo) =>
            trackInfo.HasTrackLane(laneIndexA) && trackInfo.CheckNodeFlags(NodeExt.m_flags, Node.m_flags, SegmentExtA.m_flags, SegmentA.m_flags);


        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo) {
            if(Nodeless) return;
            var infoExtA = InfoExtA;
            if(infoExtA == null || InfoExtA.TrackLaneCount == 0) return;

            foreach(var trackInfo in infoExtA.Tracks) {
                if(Check(trackInfo)) {
                    TrackRenderData renderData;
                    if(trackInfo.m_requireWindSpeed) {
                        renderData = WireRenderData.GetDataFor(trackInfo, AntiFlickerIndex);
                    } else {
                        renderData = RenderData.GetDataFor(trackInfo, AntiFlickerIndex);
                    }
                    renderData.RenderInstance(trackInfo, cameraInfo);
                    TrackManager.instance.EnqueuOverlay(trackInfo, ref OutLine, turnAround: renderData.TurnAround, DC: true);
                }
            }
        }


        public bool CalculateGroupData(ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            if(!Nodeless || InfoExtA == null || InfoExtA.TrackLaneCount == 0)
                return false;

            bool result = false;
            foreach(var trackInfo in InfoExtA.Tracks) {
                if(Check(trackInfo)) {
                    result |= RenderData.CalculateGroupData(trackInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                }
            }
            return result;
        }

        public void PopulateGroupData(int groupX, int groupZ, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData meshData) {
            if(!Nodeless || InfoExtA == null || InfoExtA.TrackLaneCount == 0)
                return;

            var renderData0 = GenerateRenderData(ref OutLine, groupPosition);
            var wireRenderData0 = GenerateRenderData(ref WireOutLine, groupPosition);
            foreach(var trackInfo in InfoExtA.Tracks) {
                if(Check(trackInfo)) {
                    TrackRenderData renderData;
                    if(trackInfo.m_requireWindSpeed) {
                        renderData = wireRenderData0.GetDataFor(trackInfo, AntiFlickerIndex);
                    } else {
                        renderData = renderData0.GetDataFor(trackInfo, AntiFlickerIndex);
                    }
                    renderData = renderData.GetDataFor(trackInfo, AntiFlickerIndex);
                    renderData.PopulateGroupData(trackInfo, groupX, groupZ, ref vertexIndex, ref triangleIndex, meshData);
                }
            }
        }
    }
}
