namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
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

    public struct BezierData {
        public Bezier3 Center, Left, Right;
        public Vector3 DirA, DirD;
        public bool SmoothA, SmoothD;

        public BezierData(Vector3 a, Vector3 d, Vector3 dirA, Vector3 dirD, float width, bool smoothA, bool smoothD) {
            {
                SmoothA = smoothA;
                Center.a = a;
                DirA = dirA;
                var normal = new Vector3(-dirA.z, 0, dirA.x);
                normal = VectorUtils.NormalizeXZ(normal);
                Left.a = a + normal * width * 0.5f;
                Right.a = a - normal * width * 0.5f;
            }
            {
                SmoothD = smoothD;
                DirD = dirD;
                Center.d = d;
                var normal = new Vector3(-dirD.z, 0, dirD.x);
                normal = -VectorUtils.NormalizeXZ(normal);
                Left.d = d + normal * width * 0.5f;
                Right.d = d - normal * width * 0.5f;
            }
            NetSegment.CalculateMiddlePoints(Center.a, DirA, Center.d, DirD, SmoothA, SmoothD, out Center.b, out Center.c);
            NetSegment.CalculateMiddlePoints(Left.a, DirA, Left.d, DirD, SmoothA, SmoothD, out Left.b, out Left.c);
            NetSegment.CalculateMiddlePoints(Right.a, DirA, Right.d, DirD, SmoothA, SmoothD, out Right.b, out Right.c);
        }
    }

    public struct TrackRenderData {
        public Matrix4x4 LeftMatrix, RightMatrix;
        public Vector4 MeshScale;
        public Vector4 ObjectIndex;
        public Color Color;
        public Quaternion Rotation => Quaternion.identity;
        public Vector3 Position;
        public float WindSpeed;
        public Vector4 GetObjectIndex(bool requiresWindSpeed) {
            Vector4 objectIndex = ObjectIndex;
            if(requiresWindSpeed) objectIndex.w = WindSpeed;
            return objectIndex;
        }
    }

    public struct LaneTransition {
        public ushort NodeID;
        public uint LaneIDSource; // dominant
        public uint LaneIDTarget;

        public void Init(uint laneID1, uint laneID2) {
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
        }

        public BezierData OutLine;
        public TrackRenderData RenderData;

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
        ref NetSegmentExt SegmentExtA => ref LaneA.m_segment.ToSegmentExt();
        NetInfoExtionsion.Net InfoExtA => SegmentExtA.NetInfoExt;
        NetInfo.Lane laneInfoA => LaneExtA.LaneData.LaneInfo;
        int laneIndexA => LaneExtA.LaneData.LaneIndex;

        float Width => laneInfoA.m_width;
        #endregion


        public void Calculate() {
            Vector3 a, dirA;
            if(SegmentA.IsStartNode(NodeID)) {
                a = LaneA.m_bezier.a;
                dirA = LaneExtA.DirA;
            } else {
                a = LaneA.m_bezier.d;
                dirA = LaneExtA.DirD;
            }

            Vector3 d, dirD;
            if(SegmentD.IsStartNode(NodeID)) {
                d = LaneD.m_bezier.a;
                dirD = LaneExtD.DirA;
            } else {
                d = LaneD.m_bezier.d;
                dirD = LaneExtD.DirD;
            }

            OutLine = new BezierData(a, d, dirA, dirD, Width, true, true);
            RenderData.Position = (a + d) * 0.5f;

            RenderData.MeshScale = new Vector4(1f / Width, 1f / InfoA.m_segmentLength, 1f, 1f);
            bool turnAround = laneInfoA.IsGoingBackward(); // TODO is this logic sufficient? is this line even necessary?
            if(turnAround) {
                RenderData.MeshScale.x *= -1;
                RenderData.MeshScale.y *= -1;
            }

            float vScale = InfoA.m_netAI.GetVScale();
            RenderData.LeftMatrix = NetSegment.CalculateControlMatrix(
                OutLine.Left.a, OutLine.Left.b, OutLine.Left.c, OutLine.Left.d,
                OutLine.Right.a, OutLine.Right.b, OutLine.Right.c, OutLine.Right.d,
                RenderData.Position, vScale);
            RenderData.RightMatrix = NetSegment.CalculateControlMatrix(
                OutLine.Right.a, OutLine.Right.b, OutLine.Right.c, OutLine.Right.d,
                OutLine.Left.a, OutLine.Left.b, OutLine.Left.c, OutLine.Left.d,
                RenderData.Position, vScale);


            RenderData.WindSpeed = Singleton<WeatherManager>.instance.GetWindSpeed(RenderData.Position);
            RenderData.Color = Info.m_color;
            RenderData.Color.a = 0;
            Vector4 colorLocationA;
            Vector4 colorLocationD;
            if(NetNode.BlendJunction(NodeID)) {
                colorLocationD = colorLocationA = RenderManager.GetColorLocation(TrackManager.NODE_HOLDER + NodeID);
            } else {
                colorLocationA = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segmentID_A);
                colorLocationD = RenderManager.GetColorLocation(TrackManager.SEGMENT_HOLDER + segmentID_D);
            }
            RenderData.ObjectIndex = new Vector4(colorLocationA.x, colorLocationA.y, colorLocationD.x, colorLocationD.y);
        }

        public void RenderTrackInstance(RenderManager.CameraInfo cameraInfo, int layerMask) {
            if(InfoA == null || (layerMask & InfoA.m_netLayers) == 0)
                return;
            var netManager = Singleton<NetManager>.instance;
            foreach(var track in InfoExtA.Tracks) {
                if(track.HasTrackLane(laneIndexA) && track.CheckNodeFlags(NodeExt.m_flags, Node.m_flags)) {
                    var objectIndex = RenderData.GetObjectIndex(track.m_requireWindSpeed);
                    if(cameraInfo.CheckRenderDistance(RenderData.Position, track.m_lodRenderDistance)) {
                        netManager.m_materialBlock.Clear();
                        netManager.m_materialBlock.SetMatrix(netManager.ID_LeftMatrix, RenderData.LeftMatrix);
                        netManager.m_materialBlock.SetMatrix(netManager.ID_RightMatrix, RenderData.RightMatrix);
                        netManager.m_materialBlock.SetVector(netManager.ID_MeshScale, RenderData.MeshScale);
                        netManager.m_materialBlock.SetVector(netManager.ID_ObjectIndex, objectIndex);
                        netManager.m_materialBlock.SetColor(netManager.ID_Color, RenderData.Color);
                        NetManager.instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(track.m_trackMesh, RenderData.Position, RenderData.Rotation, track.m_trackMaterial, track.m_layer, null, 0, netManager.m_materialBlock);
                    } else {
                        NetInfo.LodValue combinedLod = track.m_combinedLod;
                        if(combinedLod == null) continue;

                        combinedLod.m_leftMatrices[combinedLod.m_lodCount] = RenderData.LeftMatrix;
                        combinedLod.m_rightMatrices[combinedLod.m_lodCount] = RenderData.RightMatrix;
                        combinedLod.m_meshScales[combinedLod.m_lodCount] = RenderData.MeshScale;
                        combinedLod.m_objectIndices[combinedLod.m_lodCount] = objectIndex;
                        combinedLod.m_meshLocations[combinedLod.m_lodCount] = RenderData.Position;
                        combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, RenderData.Position);
                        combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, RenderData.Position);
                        if(++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
                            NetSegment.RenderLod(cameraInfo, combinedLod);
                        }
                    }
                }
            }
        }

        // refresh render data
        // render
        // calculate
        // populate
    }
}
