namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
    using ColossalFramework.Math;
    using ColossalFramework;
    using UnityEngine;
    using KianCommons.Math;
    using KianCommons;
    using AdaptiveRoads.Util;
    using Epic.OnlineServices.Presence;
    using System;

    public struct TrackRenderData {
        private static NetManager netMan => NetManager.instance;
        public Matrix4x4 LeftMatrix, RightMatrix;
        public Vector4 MeshScale;
        public Vector4 ObjectIndex; // color location
        public Color Color;
        public Quaternion Rotation => Quaternion.identity;
        public Vector3 Position;

        #region mapping
        public Vector4 SurfaceMapping, HeightMapping;
        private Texture dataTexure0_, dataTexure1_;

        public Texture SurfaceTextureA {
            get => dataTexure0_;
            set => dataTexure0_ = value;
        }
        public Texture SurfaceTextureB {
            get => dataTexure1_;
            set => dataTexure1_ = value;
        }
        public Texture HeightMapTexture {
            get => dataTexure0_;
            set => dataTexure0_ = value;
        }
        #endregion

        public float WindSpeed;
        public bool TurnAround;

        public Bezier3 Bezier;
        public float Length;
        public float Curve;

        /// <summary>
        /// Calculates control matrix and optionally flips/rotate mesh.
        /// Precondition: TurnAround and Flip are already calculated
        /// </summary>
        public void CalculateControlMatrix(OutlineData outline, float vScale) {
            bool rotate = TurnAround;
            bool flip = false;
            if (!rotate) {
                LeftMatrix = NetSegment.CalculateControlMatrix(
                    outline.Left.a, outline.Left.b, outline.Left.c, outline.Left.d,
                    outline.Right.a, outline.Right.b, outline.Right.c, outline.Right.d,
                    Position, vScale);
                RightMatrix = NetSegment.CalculateControlMatrix(
                    outline.Right.a, outline.Right.b, outline.Right.c, outline.Right.d,
                    outline.Left.a, outline.Left.b, outline.Left.c, outline.Left.d,
                    Position, vScale);
            } else {
                LeftMatrix = NetSegment.CalculateControlMatrix(
                    outline.Right.d, outline.Right.c, outline.Right.b, outline.Right.a,
                    outline.Left.d, outline.Left.c, outline.Left.b, outline.Left.a,
                    Position, vScale);
                RightMatrix = NetSegment.CalculateControlMatrix(
                    outline.Left.d, outline.Left.c, outline.Left.b, outline.Left.a,
                    outline.Right.d, outline.Right.c, outline.Right.b, outline.Right.a,
                    Position, vScale);
            }
            if (flip) {
                MeshScale.x *= -1;
                MeshScale.y *= -1;
            }
            Bezier = outline.Center;
            Length = outline.Center.ArcLength();
            Curve = outline.Center.CalculateCurve();
        }

        public void CalculateMapping(NetInfo netInfo) {
            if (netInfo.m_requireSurfaceMaps) {
                TerrainManager.instance.GetSurfaceMapping(this.Position, out var surfaceTextureA, out var surfaceTextureB, out this.SurfaceMapping);
                this.SurfaceTextureA = surfaceTextureA;
                this.SurfaceTextureB = surfaceTextureB;
            } else if (netInfo.m_requireHeightMap) {
                TerrainManager.instance.GetHeightMapping(this.Position, out var heightMapTexture, out this.HeightMapping, out this.SurfaceMapping);
                this.HeightMapTexture = heightMapTexture;
            }
        }

        public TrackRenderData GetDataFor(NetInfoExtionsion.Track trackInfo, int antiFlickerIndex = 0) {
            var ret = this;
            if(trackInfo.m_requireWindSpeed) ret.ObjectIndex.w = WindSpeed;
            if(trackInfo.ScaleToLaneWidth) ret.MeshScale.x = Mathf.Sign(MeshScale.x); //+-1

            float deltaY = trackInfo.VerticalOffset;
            float deltaY2 = 0;
            if(trackInfo.AntiFlickering) deltaY2 = antiFlickerIndex * 0.001f;
            Lift(ref ret.LeftMatrix, deltaY, deltaY2);
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
            if (cameraInfo == null) return;
            if (cameraInfo.CheckRenderDistance(this.Position, trackInfo.m_lodRenderDistance)) {
                netMan.m_materialBlock.Clear();
                netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrix, this.LeftMatrix);
                netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrix, this.RightMatrix);
                netMan.m_materialBlock.SetVector(netMan.ID_MeshScale, this.MeshScale);
                netMan.m_materialBlock.SetVector(netMan.ID_ObjectIndex, this.ObjectIndex);
                netMan.m_materialBlock.SetColor(netMan.ID_Color, this.Color);
                if (trackInfo.m_requireSurfaceMaps && SurfaceTextureA != null) {
                    netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexA, SurfaceTextureA);
                    netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexB, SurfaceTextureB);
                    netMan.m_materialBlock.SetVector(netMan.ID_SurfaceMapping, SurfaceMapping);
                } else if (trackInfo.m_requireHeightMap && HeightMapTexture != null) {
                    netMan.m_materialBlock.SetTexture(netMan.ID_HeightMap, HeightMapTexture);
                    netMan.m_materialBlock.SetVector(netMan.ID_HeightMapping, HeightMapping);
                    netMan.m_materialBlock.SetVector(netMan.ID_SurfaceMapping, SurfaceMapping);
                }
                netMan.m_drawCallData.m_defaultCalls++;
                Graphics.DrawMesh(trackInfo.m_trackMesh, this.Position, this.Rotation, trackInfo.m_trackMaterial, trackInfo.m_layer, null, 0, netMan.m_materialBlock);
            } else {
                NetInfo.LodValue combinedLod = trackInfo.m_combinedLod;
                if (combinedLod != null) {
                    combinedLod.m_leftMatrices[combinedLod.m_lodCount] = this.LeftMatrix;
                    combinedLod.m_rightMatrices[combinedLod.m_lodCount] = this.RightMatrix;
                    combinedLod.m_meshScales[combinedLod.m_lodCount] = this.MeshScale;
                    combinedLod.m_objectIndices[combinedLod.m_lodCount] = this.ObjectIndex;
                    combinedLod.m_meshLocations[combinedLod.m_lodCount] = this.Position;
                    combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, this.Position);
                    combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, this.Position);
                    if (trackInfo.m_requireSurfaceMaps && SurfaceTextureA != combinedLod.m_surfaceTexA) {
                        if (combinedLod.m_lodCount != 0) {
                            NetSegment.RenderLod(cameraInfo, combinedLod);
                        }
                        combinedLod.m_surfaceTexA = this.SurfaceTextureA;
                        combinedLod.m_surfaceTexB = this.SurfaceTextureB;
                        combinedLod.m_surfaceMapping = this.SurfaceMapping;
                    } else if (trackInfo.m_requireHeightMap && HeightMapTexture != combinedLod.m_heightMap) {
                        if (combinedLod.m_lodCount != 0) {
                            NetSegment.RenderLod(cameraInfo, combinedLod);
                        }
                        combinedLod.m_heightMap = this.HeightMapTexture;
                        combinedLod.m_heightMapping = this.HeightMapping;
                        combinedLod.m_surfaceMapping = this.SurfaceMapping;
                    }
                    if (++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
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
