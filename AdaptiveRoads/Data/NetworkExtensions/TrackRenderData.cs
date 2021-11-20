namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
    using UnityEngine;

    public struct TrackRenderData {
        private static NetManager netMan => NetManager.instance;
        public Matrix4x4 LeftMatrix, RightMatrix;
        public Vector4 MeshScale;
        public Vector4 ObjectIndex;
        public Color Color;
        public Quaternion Rotation => Quaternion.identity;
        public Vector3 Position;
        public float WindSpeed;

        public TrackRenderData GetDataFor(NetInfoExtionsion.Track trackInfo, int antiFlickerIndex = 0) {
            var ret = this;
            if(trackInfo.m_requireWindSpeed) ret.ObjectIndex.w = WindSpeed;
            if(trackInfo.ScaleToLaneWidth) ret.MeshScale.x = 1f;

            float deltaY = trackInfo.VerticalOffset;
            if(trackInfo.AntiFlickering) deltaY += antiFlickerIndex * 0.001f;
            if(deltaY != 0) {
                ref var l = ref ret.LeftMatrix;
                ref var r = ref ret.RightMatrix;
                // cols: 0=a 1=b 2=c 3=d
                // row 1 is for y
                // only raise the middle points:
                l.m11 += deltaY; //b.y
                l.m21 += deltaY; //c.y
                r.m11 += deltaY; //b.y
                r.m21 += deltaY; //c.y
            }
            return ret;
        }


        public void RenderInstance(NetInfoExtionsion.Track trackInfo) {
            var cameraInfo = RenderManager.instance.CurrentCameraInfo;
            if(cameraInfo.CheckRenderDistance(this.Position, trackInfo.m_lodRenderDistance)) {
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
