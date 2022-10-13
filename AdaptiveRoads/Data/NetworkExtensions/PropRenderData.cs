namespace AdaptiveRoads.Data.NetworkExtensions {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Patches.Lane;
    using AdaptiveRoads.UI.RoadEditor;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using ColossalFramework.Math;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static RenderManager;

    public struct PropRenderData {
        public Vector3 Position;
        public float Angle;

        public static Vector3 CalculatePropPos(
            Vector3 pos, Vector3 tan, float t,
            ushort nodeId, ushort startSegmentId, ushort endSegmentId,
            bool isCatenary) {
            if (!isCatenary) return pos;
            ref var startSegExt = ref startSegmentId.ToSegmentExt();
            ref var endSegExt = ref endSegmentId.ToSegmentExt();
            var infoExt = startSegExt.NetInfoExt;
            if (infoExt == null) return pos;

            var normalCW = VectorUtils.NormalizeXZ(new Vector3(-tan.z, 0, tan.x));
            float angleStart = startSegExt.GetEnd(nodeId).TotalAngle;
            float angleEnd = -endSegExt.GetEnd(nodeId).TotalAngle; // end angle needs minus
            float angle = Mathf.Lerp(angleStart, angleEnd, t);
            float shift = startSegExt.NetInfoExt.CatenaryHeight * Mathf.Sin(angle);

            pos += shift * normalCW;
            return pos;
        }

        public static void Calculate(
            ref LaneTransition laneTransition,
            ref TrackRenderData trackRenderData,
            NetInfoExtionsion.Track trackInfo,
            List<PropRenderData> data) {
            var props = trackInfo.Props;
            if (props != null) {
                int nProps = props.Length;
                if (nProps == 0)
                    return;

                float startAngle = laneTransition.SegmentA.CornerAngle(laneTransition.NodeID) * (Mathf.PI / 128f);
                float endAngle = laneTransition.SegmentD.CornerAngle(laneTransition.NodeID) * (Mathf.PI / 128f);
                Vector4 objectIndex1 = new Vector4(trackRenderData.ObjectIndex.x, trackRenderData.ObjectIndex.y, 1f, trackRenderData.WindSpeed);
                Vector4 objectIndex2 = new Vector4(trackRenderData.ObjectIndex.z, trackRenderData.ObjectIndex.w, 1f, trackRenderData.WindSpeed);
                InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
                if (currentMode != 0 && !trackInfo.ParentInfo.m_netAI.ColorizeProps(currentMode)) {
                    objectIndex1.z = 0f;
                    objectIndex2.z = 0f;
                }

                int seed0 = unchecked((int)laneTransition.LaneIDSource + laneTransition.AntiFlickerIndex);
                for (int iProp = 0; iProp < nProps; iProp++) {
                    var prop = props[iProp];
                    if (trackRenderData.Length >= prop.m_minLength) {
                        int repeatCountTimes2 = 2;
                        if (prop.m_repeatDistance > 1f) {
                            repeatCountTimes2 *= Mathf.Max(1, Mathf.RoundToInt(trackRenderData.Length / prop.m_repeatDistance));
                        }
                        int expectedEntryCount = data.Count + (repeatCountTimes2 + 1 >> 1);

                        //if (prop.Check(...))
                        {
                            float offset = prop.m_segmentOffset * 0.5f;
                            if (trackRenderData.Length != 0f) {
                                offset = Mathf.Clamp(offset + prop.m_position.z / trackRenderData.Length, -0.5f, 0.5f);
                            }
                            PropInfo propInfo = prop.m_finalProp;
                            Randomizer randomizer = new Randomizer(unchecked(seed0 + iProp));
                            if (propInfo != null /*&& (layerMask & 1 << propInfo.m_prefabDataLayer) != 0*/) {
                                for (int repeateIndex2 = 1; repeateIndex2 <= repeatCountTimes2; repeateIndex2 += 2) {
                                    if (randomizer.Int32(100u) < prop.m_probability) {
                                        float t = offset + (float)repeateIndex2 / (float)repeatCountTimes2;
                                        PropInfo variation = propInfo.GetVariation(ref randomizer);
                                        //float scale = variation.m_minScale + (float)randomizer.Int32(10000u) * (variation.m_maxScale - variation.m_minScale) * 0.0001f;
                                        //Color color = variation.GetColor(ref randomizer);

                                        Vector3 pos;
                                        float finalAngle;
                                        {
                                            pos = trackRenderData.Bezier.Position(t);
                                            Vector3 tan = trackRenderData.Bezier.Tangent(t);
                                            if (tan == Vector3.zero) {
                                                continue;
                                            }

                                            var propPosition = CalculatePropPos(
                                                pos:prop.m_position, tan: tan, t: t,
                                                nodeId: laneTransition.NodeID,
                                                startSegmentId: laneTransition.segmentID_A, endSegmentId: laneTransition.segmentID_D,
                                                isCatenary: prop.Catenary);

                                            tan.y = 0f;
                                            if (prop.m_position.x != 0f) {
                                                tan.Normalize();
                                                pos += tan * propPosition.x;
                                            }
                                            finalAngle = Mathf.Atan2(tan.x, 0f - tan.z);
                                            if (prop.m_cornerAngle != 0f || propPosition.x != 0f) {
                                                float angleDiff = endAngle - startAngle;
                                                if (angleDiff > (float)Math.PI) {
                                                    angleDiff -= (float)Math.PI * 2f;
                                                }
                                                if (angleDiff < -(float)Math.PI) {
                                                    angleDiff += (float)Math.PI * 2f;
                                                }
                                                angleDiff = startAngle + angleDiff * t - finalAngle;
                                                if (angleDiff > (float)Math.PI) {
                                                    angleDiff -= (float)Math.PI * 2f;
                                                }
                                                if (angleDiff < -(float)Math.PI) {
                                                    angleDiff += (float)Math.PI * 2f;
                                                }
                                                finalAngle += angleDiff * prop.m_cornerAngle;
                                                if (angleDiff != 0f && prop.m_position.x != 0f) {
                                                    float d = Mathf.Tan(angleDiff);
                                                    pos.x += tan.x * d * propPosition.x;
                                                    pos.z += tan.z * d * propPosition.x;
                                                }
                                            }
                                            if (variation.m_requireWaterMap) {
                                                pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(pos, timeLerp: false, 0f);
                                            } else {
                                                pos.y = Singleton<TerrainManager>.instance.SampleDetailHeight(pos);
                                            }
                                            pos.y += propPosition.y;
                                            finalAngle += prop.m_angle * Mathf.Deg2Rad;
                                        }

                                        data.Add(new PropRenderData {
                                            Position = pos,
                                            Angle = finalAngle,
                                        });

                                        //if (cameraInfo.CheckRenderDistance(pos, variation.m_maxRenderDistance)) {
                                        //    Vector4 objectIndex = (t <= 0.5f) ? objectIndex1 : objectIndex2;
                                        //    InstanceID instanceID = new InstanceID { NetNode = laneTransition.NodeID };
                                        //    if (variation.m_requireWaterMap) {
                                        //        if (heightMap == null) {
                                        //            Singleton<TerrainManager>.instance.GetHeightMapping(laneTransition.Node.m_position, out heightMap, out heightMapping, out surfaceMapping);
                                        //        }
                                        //        if (waterHeightMap == null) {
                                        //            Singleton<TerrainManager>.instance.GetWaterMapping(laneTransition.Node.m_position, out waterHeightMap, out waterHeightMapping, out waterSurfaceMapping);
                                        //        }
                                        //        PropInstance.RenderInstance(cameraInfo, variation, instanceID, pos, scale, finalAngle, color, objectIndex, true, heightMap, heightMapping, surfaceMapping, waterHeightMap, waterHeightMapping, waterSurfaceMapping);
                                        //    } else if (!variation.m_requireHeightMap) {
                                        //        PropInstance.RenderInstance(cameraInfo, variation, instanceID, pos, scale, finalAngle, color, objectIndex, true);
                                        //    }
                                        //}
                                    }
                                }
                            }
                            TreeInfo finalTree = prop.m_finalTree;
                            if (finalTree != null /*&& (layerMask & 1 << finalTree.m_prefabDataLayer) != 0*/) {
                                for (int repeateIndex2 = 1; repeateIndex2 <= repeatCountTimes2; repeateIndex2 += 2) {
                                    if (randomizer.Int32(100u) < prop.m_probability) {
                                        TreeInfo variation = finalTree.GetVariation(ref randomizer);
                                        float scale = variation.m_minScale + (float)randomizer.Int32(10000u) * (variation.m_maxScale - variation.m_minScale) * 0.0001f;
                                        float brightness = variation.m_minBrightness + (float)randomizer.Int32(10000u) * (variation.m_maxBrightness - variation.m_minBrightness) * 0.0001f;

                                        Vector3 position;
                                        {
                                            float t = offset + (float)repeateIndex2 / (float)repeatCountTimes2;
                                            var propPosition = prop.m_position;
                                            position = trackRenderData.Bezier.Position(t);
                                            if (prop.m_position.x != 0f) {
                                                Vector3 tan = trackRenderData.Bezier.Tangent(t);
                                                tan.y = 0f;
                                                tan = Vector3.Normalize(tan);
                                                position.x += tan.z * propPosition.x;
                                                position.z -= tan.x * propPosition.x;
                                            }
                                            position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                                            position.y += propPosition.y;
                                        }
                                        data.Add(new PropRenderData {
                                            Position = position,
                                        });
                                        //global::TreeInstance.RenderInstance(cameraInfo, variation, position, scale, brightness, RenderManager.DefaultColorLocation);
                                    }
                                }
                            }

                            while (data.Count < expectedEntryCount) data.Add(default);// fill dummies to make prop indeces predictable.
                        }
                    }
                }
            }
        }

        public static void Render(
            RenderManager.CameraInfo cameraInfo,
            ref LaneTransition laneTransition,
            ref TrackRenderData trackRenderData,
            NetInfoExtionsion.Track trackInfo,
            int layerMask,
            PropRenderData[] data,
            ref int propIndex) {
            var props = trackInfo.Props;

            // this code from NetSegment.RenderInstance does not make sense. specially the '&&' part
            //if ((layerMask & trackInfo.ParentInfo.m_propLayers) == 0 &&
            //    !cameraInfo.CheckRenderDistance(trackRenderData.Position, trackInfo.ParentInfo.m_maxPropDistance + 128f)) {
            //    return;
            //}

            if (props != null) {
                int nProps = props.Length;
                if (nProps == 0)
                    return;

                Vector4 heightMapping = default;
                Vector4 surfaceMapping = default;
                Vector4 waterHeightMapping = default;
                Vector4 waterSurfaceMapping = default;
                Texture heightMap = null;
                Texture waterHeightMap = null;

                NetNode.FlagsLong vanillaNodeFlags = laneTransition.Node.flags;
                vanillaNodeFlags |= (NetNode.FlagsLong)laneTransition.DCFlags;
                NetNodeExt.Flags nodeFlags = laneTransition.NodeID.ToNodeExt().m_flags;


                //float startAngle = laneTransition.SegmentA.CornerAngle(laneTransition.NodeID) * (Mathf.PI / 128f);
                //float endAngle = laneTransition.SegmentD.CornerAngle(laneTransition.NodeID) * (Mathf.PI / 128f);
                Vector4 objectIndex1 = new Vector4(trackRenderData.ObjectIndex.x, trackRenderData.ObjectIndex.y, 1f, trackRenderData.WindSpeed);
                Vector4 objectIndex2 = new Vector4(trackRenderData.ObjectIndex.z, trackRenderData.ObjectIndex.w, 1f, trackRenderData.WindSpeed);
                InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
                if (currentMode != 0 && !trackInfo.ParentInfo.m_netAI.ColorizeProps(currentMode)) {
                    objectIndex1.z = 0f;
                    objectIndex2.z = 0f;
                }

                int seed0 = unchecked((int)laneTransition.LaneIDSource + laneTransition.AntiFlickerIndex);
                var sourceSegmentEndFlags = laneTransition.SegmentExtA.GetEnd(laneTransition.NodeID).m_flags;
                var userData = laneTransition.SegmentExtA.UserData;
                for (int iProp = 0; iProp < nProps; iProp++) {
                    var prop = props[iProp];
                    if (trackRenderData.Length >= prop.m_minLength) {
                        int repeatCountTimes2 = 2;
                        if (prop.m_repeatDistance > 1f) {
                            repeatCountTimes2 *= Mathf.Max(1, Mathf.RoundToInt(trackRenderData.Length / prop.m_repeatDistance));
                        }
                        int expectedPropIndex = propIndex + (repeatCountTimes2 + 1 >> 1);

                        if (prop.Check(
                            vanillaNodeFlags: vanillaNodeFlags, nodeFlags: nodeFlags, segmentEndFlags: sourceSegmentEndFlags,
                            vanillaSegmentFlags: laneTransition.SegmentA.m_flags, laneTransition.SegmentExtA.m_flags, segmentUserData: userData,
                            transitionFlags: laneTransition.m_flags,
                            laneFlags: laneTransition.LaneExtA.m_flags,
                            trackRenderData.Curve)) {
                            float offset = prop.m_segmentOffset * 0.5f;
                            if (trackRenderData.Length != 0f) {
                                offset = Mathf.Clamp(offset + prop.m_position.z / trackRenderData.Length, -0.5f, 0.5f);
                            }
                            PropInfo propInfo = prop.m_finalProp;
                            Randomizer randomizer = new Randomizer(unchecked(seed0 + iProp));
                            if (propInfo != null && (layerMask & 1 << propInfo.m_prefabDataLayer) != 0) {
                                for (int repeateIndex2 = 1; repeateIndex2 <= repeatCountTimes2; repeateIndex2 += 2) {
                                    if (randomizer.Int32(100u) < prop.m_probability) {
                                        float t = offset + (float)repeateIndex2 / (float)repeatCountTimes2;
                                        PropInfo variation = propInfo.GetVariation(ref randomizer);
                                        float scale = variation.m_minScale + (float)randomizer.Int32(10000u) * (variation.m_maxScale - variation.m_minScale) * 0.0001f;
                                        Color color = variation.GetColor(ref randomizer);

                                        var currentData = data[propIndex++];
                                        Vector3 pos = currentData.Position;
                                        float finalAngle = currentData.Angle;

                                        if (cameraInfo.CheckRenderDistance(pos, variation.m_maxRenderDistance)) {
                                            Vector4 objectIndex = (t <= 0.5f) ? objectIndex1 : objectIndex2;
                                            InstanceID instanceID = new InstanceID { NetNode = laneTransition.NodeID };
                                            if (variation.m_requireWaterMap) {
                                                if (heightMap == null) {
                                                    Singleton<TerrainManager>.instance.GetHeightMapping(laneTransition.Node.m_position, out heightMap, out heightMapping, out surfaceMapping);
                                                }
                                                if (waterHeightMap == null) {
                                                    Singleton<TerrainManager>.instance.GetWaterMapping(laneTransition.Node.m_position, out waterHeightMap, out waterHeightMapping, out waterSurfaceMapping);
                                                }
                                                OnAfterRenderInstance(prop, variation, pos, finalAngle, scale);
                                                PropInstance.RenderInstance(cameraInfo, variation, instanceID, pos, scale, finalAngle, color, objectIndex, true, heightMap, heightMapping, surfaceMapping, waterHeightMap, waterHeightMapping, waterSurfaceMapping);
                                            } else if (!variation.m_requireHeightMap) {
                                                OnAfterRenderInstance(prop, variation, pos, finalAngle, scale);
                                                PropInstance.RenderInstance(cameraInfo, variation, instanceID, pos, scale, finalAngle, color, objectIndex, true);
                                            }
                                        }
                                    }
                                }
                            }
                            TreeInfo finalTree = prop.m_finalTree;
                            if (finalTree != null && (layerMask & 1 << finalTree.m_prefabDataLayer) != 0) {
                                for (int repeateIndex2 = 1; repeateIndex2 <= repeatCountTimes2; repeateIndex2 += 2) {
                                    if (randomizer.Int32(100u) < prop.m_probability) {
                                        TreeInfo variation = finalTree.GetVariation(ref randomizer);
                                        float scale = variation.m_minScale + (float)randomizer.Int32(10000u) * (variation.m_maxScale - variation.m_minScale) * 0.0001f;
                                        float brightness = variation.m_minBrightness + (float)randomizer.Int32(10000u) * (variation.m_maxBrightness - variation.m_minBrightness) * 0.0001f;

                                        var currentData = data[propIndex++];

                                        global::TreeInstance.RenderInstance(cameraInfo, variation, currentData.Position, scale, brightness, RenderManager.DefaultColorLocation);
                                    }
                                }
                            }
                        }

                        propIndex = expectedPropIndex;
                    }
                }
            }

        }

        static void OnAfterRenderInstance(
            NetInfoExtionsion.TransitionProp prop,
            PropInfo variation,
            Vector3 pos,
            float angle,
            float scale) {
            if (prop == Overlay.HoveredInfo) {
                Overlay.PropQueue.Enqueue(new Overlay.PropData {
                    Angle = angle,
                    Scale = scale,
                    Pos = pos,
                    Prop = variation,
                });
            }
        }
    }
}
