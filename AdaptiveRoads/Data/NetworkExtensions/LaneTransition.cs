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

    public struct OutlineData {
        public Bezier3 Center, Left, Right;
        public Vector3 DirA, DirD;
        public bool SmoothA, SmoothD;

        public OutlineData(Vector3 a, Vector3 d, Vector3 dirA, Vector3 dirD, float width, bool smoothA, bool smoothD) {
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
            var netMan = NetManager.instance;
            foreach(var trackInfo in infoExtA.Tracks) {
                if(trackInfo.HasTrackLane(laneIndexA) && trackInfo.CheckNodeFlags(NodeExt.m_flags, Node.m_flags)) {
                    var objectIndex = RenderData.GetObjectIndex(trackInfo.m_requireWindSpeed);
                    if(cameraInfo.CheckRenderDistance(RenderData.Position, trackInfo.m_lodRenderDistance)) {
                        netMan.m_materialBlock.Clear();
                        netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrix, RenderData.LeftMatrix);
                        netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrix, RenderData.RightMatrix);
                        netMan.m_materialBlock.SetVector(netMan.ID_MeshScale, RenderData.MeshScale);
                        netMan.m_materialBlock.SetVector(netMan.ID_ObjectIndex, objectIndex);
                        netMan.m_materialBlock.SetColor(netMan.ID_Color, RenderData.Color);
                        netMan.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(trackInfo.m_trackMesh, RenderData.Position, RenderData.Rotation, trackInfo.m_trackMaterial, trackInfo.m_layer, null, 0, netMan.m_materialBlock);
                    } else {
                        NetInfo.LodValue combinedLod = trackInfo.m_combinedLod;
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


        public bool CalculateGroupData(ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            var infoExtA = InfoExtA;
            if(infoExtA == null)
                return false;
            if(infoExtA.TrackLaneCount == 0)
                return false;
            bool result = false;

            foreach(var trackInfo in infoExtA.Tracks) {
                if(trackInfo.HasTrackLane(LaneExtA.LaneData.LaneIndex) && trackInfo.CheckNodeFlags(NodeExt.m_flags, Node.m_flags)) {
                    if(trackInfo.m_combinedLod != null) {
                        var tempSegmentInfo = NetInfoExtionsion.Net.TempSegmentInfo(trackInfo);
                        NetSegment.CalculateGroupData(tempSegmentInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                        result = true;
                    }
                }
            }

            return result;
        }

        public void PopulateGroupData(int groupX, int groupZ, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData meshData) {
            if(InfoExtA == null)
                return;
            if(InfoExtA.TrackLaneCount == 0)
                return;
            var RenderData = GenerateRenderData(groupPosition);
            foreach(var trackInfo in InfoExtA.Tracks) {
                if(trackInfo.HasTrackLane(LaneExtA.LaneData.LaneIndex) && trackInfo.CheckNodeFlags(NodeExt.m_flags, Node.m_flags)) {
                    if(trackInfo.m_combinedLod != null) {
                        var tempSegmentInfo = NetInfoExtionsion.Net.TempSegmentInfo(trackInfo);
                        Vector4 objectIndex = RenderData.GetObjectIndex(trackInfo.m_requireWindSpeed);
                        bool _ = false;
                        NetSegment.PopulateGroupData(
                            InfoA, tempSegmentInfo,
                            leftMatrix: RenderData.LeftMatrix, rightMatrix: RenderData.RightMatrix,
                            meshScale: RenderData.MeshScale  , objectIndex: objectIndex,
                            ref vertexIndex, ref triangleIndex, groupPosition, meshData, ref _);
                    }
                }
            }
        }
    }
}
