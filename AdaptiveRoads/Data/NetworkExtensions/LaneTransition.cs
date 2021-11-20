namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using KianCommons;
    using UnityEngine;
    using Log = KianCommons.Log;

    public struct LaneTransition {
        public ushort NodeID;
        public uint LaneIDSource; // dominant
        public uint LaneIDTarget;
        public int TransitionIndex;

        public void Init(uint laneID1, uint laneID2, int index) {
            TransitionIndex = index;
            ushort segmentID1 = laneID1.ToLane().m_segment;
            ushort segmentID2 = laneID2.ToLane().m_segment;
            float prio1 = segmentID1.ToSegment().Info.m_netAI.GetNodeInfoPriority(segmentID1, ref segmentID1.ToSegment());
            float prio2 = segmentID2.ToSegment().Info.m_netAI.GetNodeInfoPriority(segmentID2, ref segmentID2.ToSegment());

            if(prio1 >= prio2) {
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

        public override string ToString() => $"LaneTransition[node:{NodeID} lane:{LaneIDSource}->lane:{LaneIDTarget}]";
        #region shortcuts
        ref NetLane LaneA => ref LaneIDSource.ToLane();
        ref NetLane LaneD => ref LaneIDTarget.ToLane();
        ref NetLaneExt LaneExtA => ref LaneIDSource.ToLaneExt();
        ref NetLaneExt LaneExtD => ref LaneIDTarget.ToLaneExt();
        ref NetSegment SegmentA => ref LaneA.m_segment.ToSegment();
        ref NetSegment SegmentD => ref LaneD.m_segment.ToSegment();
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

        public void Calculate() {
            Vector3 a, dirA;
            if(SegmentA.IsStartNode(NodeID)) {
                a = LaneA.m_bezier.a;
                dirA = LaneExtA.OutLine.DirA;
            } else {
                a = LaneA.m_bezier.d;
                dirA = LaneExtA.OutLine.DirD;
            }

            Vector3 d, dirD;
            if(SegmentD.IsStartNode(NodeID)) {
                d = LaneD.m_bezier.a;
                dirD = LaneExtD.OutLine.DirA;
            } else {
                d = LaneD.m_bezier.d;
                dirD = LaneExtD.OutLine.DirD;
            }

            OutLine = new OutlineData(a, d, -dirA, -dirD, Width, true, true);
            RenderData = GenerateRenderData();
        }
        public TrackRenderData GenerateRenderData(Vector3? pos = null) {
            TrackRenderData ret = default;
            ret.Position = pos ?? (OutLine.Center.a + OutLine.Center.d) * 0.5f;

            ret.MeshScale = new Vector4(1f / Width, 1f / InfoA.m_segmentLength, 1f, 1f);
            bool turnAround = laneInfoA.IsGoingBackward(); // TODO is this logic sufficient? is this line even necessary?
            if(turnAround) {
                ret.MeshScale.x *= -1;
                ret.MeshScale.y *= -1;
            }

            float vScale = InfoA.m_netAI.GetVScale();
            ret.LeftMatrix = NetSegment.CalculateControlMatrix(
                OutLine.Left.a, OutLine.Left.b, OutLine.Left.c, OutLine.Left.d,
                OutLine.Right.a, OutLine.Right.b, OutLine.Right.c, OutLine.Right.d,
                ret.Position, vScale);
            ret.RightMatrix = NetSegment.CalculateControlMatrix(
                OutLine.Right.a, OutLine.Right.b, OutLine.Right.c, OutLine.Right.d,
                OutLine.Left.a, OutLine.Left.b, OutLine.Left.c, OutLine.Left.d,
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

        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo) {
            var infoExtA = InfoExtA;
            if(infoExtA == null)
                return;
            foreach(var trackInfo in infoExtA.Tracks) {
                if(trackInfo.HasTrackLane(laneIndexA) && trackInfo.CheckNodeFlags(NodeExt.m_flags, Node.m_flags)) {
                    var renderData = RenderData.GetDataFor(trackInfo, TransitionIndex);
                    renderData.RenderInstance(trackInfo);
                }
            }
        }


        public bool CalculateGroupData(ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            var infoExtA = InfoExtA;
            if(infoExtA == null)
                return false;
            if(infoExtA.TrackLaneCount == 0)
                return false;

            bool result = false;
            foreach(var trackInfo in infoExtA.Tracks) {
                if(trackInfo.HasTrackLane(LaneExtA.LaneData.LaneIndex) && trackInfo.CheckNodeFlags(NodeExt.m_flags, Node.m_flags)) {
                    result |= RenderData.CalculateGroupData(trackInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                }
            }
            return result;
        }

        public void PopulateGroupData(int groupX, int groupZ, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData meshData) {
            if(InfoExtA == null)
                return;
            if(InfoExtA.TrackLaneCount == 0)
                return;

            var renderData0 = GenerateRenderData(groupPosition);
            foreach(var trackInfo in InfoExtA.Tracks) {
                if(trackInfo.HasTrackLane(LaneExtA.LaneData.LaneIndex) && trackInfo.CheckNodeFlags(NodeExt.m_flags, Node.m_flags)) {
                    var renderData = renderData0.GetDataFor(trackInfo, TransitionIndex);
                    renderData.PopulateGroupData(trackInfo, groupX, groupZ, ref vertexIndex, ref triangleIndex, meshData);
                }
            }
        }
    }
}
