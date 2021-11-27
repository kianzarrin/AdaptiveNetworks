namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
    using UnityEngine;

    public struct TrackRenderData {
        private static NetManager netMan => NetManager.instance;
        public Matrix4x4 LeftMatrix, RightMatrix;
        public Vector4 MeshScale;
        public Vector4 ObjectIndex;
        public Color Color;
        public Quaternion Rotation => Quaternion.Slerp(Quaternion.Euler(30, 0, 0), Quaternion.Euler(0, 0, 30), 0.5f);
        public Vector3 Position;
        public float WindSpeed;
        public bool TurnAround;

        public TrackRenderData GetDataFor(NetInfoExtionsion.Track trackInfo, int antiFlickerIndex = 0) {
            var ret = this;
            if(trackInfo.m_requireWindSpeed) ret.ObjectIndex.w = WindSpeed;
            if(trackInfo.ScaleToLaneWidth) ret.MeshScale.x = 1f;

            float deltaY = trackInfo.VerticalOffset;
            float deltaY2 = 0;
            if(trackInfo.AntiFlickering) deltaY2 = antiFlickerIndex * 0.001f;
            Lift(ref ret.LeftMatrix,deltaY,deltaY2);
            Lift(ref ret.RightMatrix, deltaY, deltaY2);

            return ret;
            static void Lift(ref Matrix4x4 mat, float deltaY, float deltaY2) {
                // row 1 is for y
                // cols: 0=a 1=b 2=c 3=d
                mat.m10 += deltaY; //a.y
                mat.m11 += deltaY + deltaY2; //b.y
                mat.m12 += deltaY + deltaY2; //c.y
                mat.m13 += deltaY; //d.y
            }
        }

        public void RenderInstance(NetInfoExtionsion.Track trackInfo, RenderManager.CameraInfo cameraInfo) {
            if(cameraInfo == null || cameraInfo.CheckRenderDistance(this.Position, trackInfo.m_lodRenderDistance)) {
                netMan.m_materialBlock.Clear();
                netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrix, this.LeftMatrix);
                netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrix, this.RightMatrix);
                netMan.m_materialBlock.SetVector(netMan.ID_MeshScale, this.MeshScale);
                netMan.m_materialBlock.SetVector(netMan.ID_ObjectIndex, this.ObjectIndex);
                netMan.m_materialBlock.SetColor(netMan.ID_Color, this.Color);
                netMan.m_drawCallData.m_defaultCalls++;
                Graphics.DrawMesh(trackInfo.m_trackMesh, this.Position, this.Rotation, trackInfo.m_trackMaterial, trackInfo.m_layer, null, 0, netMan.m_materialBlock);
            } else {
                NetInfo.LodValue combinedLod = trackInfo.m_combinedLod;
                if(combinedLod != null) {
                    combinedLod.m_leftMatrices[combinedLod.m_lodCount] = this.LeftMatrix;
                    combinedLod.m_rightMatrices[combinedLod.m_lodCount] = this.RightMatrix;
                    combinedLod.m_meshScales[combinedLod.m_lodCount] = this.MeshScale;
                    combinedLod.m_objectIndices[combinedLod.m_lodCount] = this.ObjectIndex;
                    combinedLod.m_meshLocations[combinedLod.m_lodCount] = this.Position;
                    combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, this.Position);
                    combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, this.Position);
                    if(++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
                        NetSegment.RenderLod(cameraInfo, combinedLod);
                    }
                }
            }
        }

        public bool CalculateGroupData(NetInfoExtionsion.Track trackInfo, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            if(trackInfo.m_combinedLod != null) {
                var tempSegmentInfo = NetInfoExtionsion.Net.TempSegmentInfo(trackInfo);
                NetSegment.CalculateGroupData(tempSegmentInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                return true;
            }
            return false;
        }

        public void PopulateGroupData(NetInfoExtionsion.Track trackInfo, int groupX, int groupZ, ref int vertexIndex, ref int triangleIndex, RenderGroup.MeshData meshData) {
            if(trackInfo.m_combinedLod != null) {
                var tempSegmentInfo = NetInfoExtionsion.Net.TempSegmentInfo(trackInfo);
                bool _ = false;
                NetSegment.PopulateGroupData(
                    trackInfo.ParentInfo, tempSegmentInfo,
                    leftMatrix: this.LeftMatrix, rightMatrix: this.RightMatrix,
                    meshScale: this.MeshScale, objectIndex: this.ObjectIndex,
                    ref vertexIndex, ref triangleIndex, this.Position, meshData, ref _);
            }
        }
    }


}
