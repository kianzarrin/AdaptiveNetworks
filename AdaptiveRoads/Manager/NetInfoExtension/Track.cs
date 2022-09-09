namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data;
    using AdaptiveRoads.Data.NetworkExtensions;
    using AdaptiveRoads.LifeCycle;
    using AdaptiveRoads.UI.RoadEditor.Bitmask;
    using AdaptiveRoads.Util;
    using ColossalFramework;
    using JetBrains.Annotations;
    using KianCommons;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using UnityEngine;
    using static AdaptiveRoads.UI.ModSettings;
    using static KianCommons.ReflectionHelpers;
    using Vector3Serializable = KianCommons.Serialization.Vector3Serializable;

    public static partial class NetInfoExtionsion {
        [Serializable]
        public class Track : ICloneable, ISerializable {
            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Track() { }
            public Track Clone() {
                try {
                    var ret = this.ShalowClone();
                    ret.SegmentUserData = ret.SegmentUserData?.ShalowClone();
                    return ret;
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }
            object ICloneable.Clone() => this.Clone();
            public Track(NetInfo template) {
                try {
                    Assertion.Assert(template, "template");
                    var lanes = template.m_lanes;
                    for (int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                        if (lanes[laneIndex].m_vehicleType.IsFlagSet(TrackUtils.TRACK_VEHICLE_TYPES))
                            LaneIndeces |= 1ul << laneIndex;
                    }
                    if (LaneIndeces == 0) {
                        for (int laneIndex = 0; laneIndex < lanes.Length; ++laneIndex) {
                            if (lanes[laneIndex].m_vehicleType.IsFlagSet(VehicleInfo.VehicleType.Bicycle))
                                LaneIndeces |= 1ul << laneIndex;
                        }
                    }
                } catch (Exception ex) {
                    ex.Log();
                    throw;
                }
            }

            public void ReleaseModel() {
                try {
                    UnityEngine.Object.Destroy(m_mesh);
                    UnityEngine.Object.Destroy(m_lodMesh);
                    AssetEditorRoadUtils.ReleaseMaterial(m_material);
                    AssetEditorRoadUtils.ReleaseMaterial(m_lodMaterial);
                } catch (Exception ex) {
                    ex.Log();
                }
            }

            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) {
                try {
                    if(Log.VERBOSE) Log.Called();
                    var package = PackageManagerUtil.SavingPackage;
                    Assertion.NotNull(package, "package");
                    var fields = this.GetType().GetFields(ReflectionHelpers.COPYABLE).Where(field => !field.HasAttribute<NonSerializedAttribute>());
                    foreach(FieldInfo field in fields) {
                        var type = field.GetType();
                        object value = field.GetValue(this);
                        if (type == typeof(Vector3)) {
                            //Vector3Serializable v = (Vector3Serializable)field.GetValue(instance);
                            info.AddValue(field.Name, value, typeof(Vector3Serializable));
                        } else if (value is Mesh mesh) {
                            Log.Debug($"package.AddAsset mesh : {mesh} InMainThread={Helpers.InMainThread()}");
                            var asset = package.AddAsset(mesh.name, mesh, true);
                            info.AddValue(field.Name, asset.checksum);
                        } else if (value is Material material) {
                            Log.Debug($"package.AddAsset material : {material} InMainThread={Helpers.InMainThread()}");
                            var asset = package.AddAsset(material.name, material, true);
                            info.AddValue(field.Name, asset.checksum);
                        } else {
                            info.AddValue(field.Name, value, field.FieldType);
                        }
                    }
                } catch(Exception ex) {
                    ex.Log();
                }
            }

            // deserialization
            public Track(SerializationInfo info, StreamingContext context) {
                try {
                    if (Log.VERBOSE) Log.Called();
                    //Log.Debug("Track(SerializationInfo info, StreamingContext context) Called");
                    foreach (SerializationEntry item in info) {
                        FieldInfo field = this.GetType().GetField(item.Name, ReflectionHelpers.COPYABLE);
                        if (field != null) {
                            object val;
                            if (field.FieldType == typeof(Mesh)) {
                                bool lod = field.Name.Contains("lod");
                                string checksum = item.Value as string;
                                val = LSMRevisited.GetMesh(checksum, AssetDataExtension.CurrentBasicNetInfo);
                            } else if (field.FieldType == typeof(Material)) {
                                bool lod = field.Name.Contains("lod");
                                string checksum = item.Value as string;
                                val = LSMRevisited.GetMaterial(checksum, AssetDataExtension.CurrentBasicNetInfo);
#pragma warning disable CS0618 // Type or member is obsolete
                            } else if(item.Value is VanillaNodeInfoFlags flags) {
#pragma warning restore CS0618 // Type or member is obsolete
                                val = (VanillaNodeInfoFlagsLong)flags;
                            } else {
                                val = Convert.ChangeType(item.Value, field.FieldType);
                            }
                            field.SetValue(this, val);
                        } 
                    }
                } catch (Exception ex) { ex.Log(); }
                if(m_lodMesh == null || m_lodMaterial == null) {
                    Log.Warning($"lod mesh = {m_lodMesh.ToSTR()} , lod material = {m_lodMaterial.ToSTR()}"); 
                }
                // Log.Debug("Track(SerializationInfo info, StreamingContext context) Succeeded");
                if (Log.VERBOSE) Log.Succeeded();
            }
            #endregion

            public void Recalculate(NetInfo netInfo) {
                try {
                    Assertion.Assert(Helpers.InMainThread(), "in main thread");
                    Log.Called(netInfo, $"inMainthread={Helpers.InMainThread()}");
                    this.ParentInfo = netInfo;
                    Assertion.NotNull(ParentInfo, "ParentInfo");
                    Tags.Recalculate();
                    LaneTags.Recalculate();
                    this.m_trackMesh = this.m_mesh;
                    if(this.m_mesh) {
                        float corner1 = netInfo.m_minHeight - netInfo.m_maxSlope * 64f - 10f;
                        float corner2 = netInfo.m_maxHeight + netInfo.m_maxSlope * 64f + 10f;
                        this.m_mesh.bounds = new Bounds(new Vector3(0f, (corner1 + corner2) * 0.5f, 0f), new Vector3(128f, corner2 - corner1, 128f));
                    }
                    string tag = this.m_material?.GetTag("NetType", searchFallbacks: false);
                    if (tag == "TerrainSurface") {
                        this.m_requireSurfaceMaps = true;
                    } else {
                        m_requireSurfaceMaps = false;
                    }
                    if (tag == "Fence") {
                        m_requireHeightMap = true;
                    } else {
                        m_requireHeightMap = false;
                    }
                    if (tag == "PowerLine") {
                        this.m_requireWindSpeed = true;
                        this.m_preserveUVs = true;
                        this.m_generateTangents = false;
                        this.m_layer = LayerMask.NameToLayer("PowerLines");
                    } else if(tag == "MetroTunnel") {
                        this.m_requireWindSpeed = false;
                        this.m_preserveUVs = false;
                        this.m_generateTangents = false;
                        this.m_layer = LayerMask.NameToLayer("MetroTunnels");
                    } else {
                        this.m_requireWindSpeed = false;
                        this.m_preserveUVs = false;
                        this.m_generateTangents = false;
                        this.m_layer = netInfo.m_prefabDataLayer;
                    }
                    if(this.m_material) {
                        this.m_trackMaterial = new Material(this.m_material);
                        if (UseKeywordNETSEGMENT)
                            this.m_trackMaterial.EnableKeyword("NET_SEGMENT");
                        Color color = this.m_material.color;
                        color.a = 0f;
                        this.m_trackMaterial.color = color;
                        Texture2D texture2D = this.m_material.mainTexture as Texture2D;
                        if(texture2D != null && texture2D.format == TextureFormat.DXT5) {
                            CODebugBase<LogChannel>.Warn(LogChannel.Core, "Segment diffuse is DXT5: " + netInfo.gameObject.name, netInfo.gameObject);
                        }
                    } else {
                        m_trackMaterial = null;
                    }
                    LaneCount = EnumBitMaskExtensions.CountOnes(LaneIndeces);
                    UpdateScale(netInfo);
                    FixRenderOrder();
                } catch(Exception ex) { ex.Log(); }
            }

            private void FixRenderOrder() {
                try {
                    m_trackMaterial.FixRenderQueue();
                    m_lodMaterial.FixRenderQueue();
                } catch (Exception ex) {
                    ex.Log();
                }
            }

            private void UpdateScale(NetInfo info) {
                try {
                    SetupThinWires(info);
                    SetupTiling(info);
                } catch (Exception ex) {
                    ex.Log();
                }
            }

            private void SetupThinWires(NetInfo info) {
                try {
                    if (info?.m_netAI is not TrainTrackBaseAI) return;
                    if (!m_requireWindSpeed) return;
                    if (ThinWires) {
                        Vector2 scale = new Vector2(3.5f, 1.0f);
                        if (m_material) m_material.mainTextureScale = scale;
                        if (m_trackMaterial) m_trackMaterial.mainTextureScale = scale;
                        if (m_lodMaterial) m_lodMaterial.mainTextureScale = scale;
                        if (m_combinedLod?.m_material) m_combinedLod.m_material.mainTextureScale = scale;
                    }
                } catch (Exception ex) {
                    ex.Log();
                }
            }

            private void SetupTiling(NetInfo info) {
                try {
                    if (Tiling != 0) {
                        m_material?.SetTiling(Tiling);
                        m_trackMaterial?.SetTiling(Tiling);
                        m_lodMaterial?.SetTiling(Tiling);
                        m_combinedLod?.m_material?.SetTiling(Mathf.Abs(Tiling));
                    }
                } catch (Exception ex) {
                    ex.Log();
                }
            }

            #region materials
            public Mesh m_mesh;

            public Mesh m_lodMesh;

            public Material m_material;

            public Material m_lodMaterial;

            [NonSerialized]
            public NetInfo.LodValue m_combinedLod;

            [NonSerialized]
            public Mesh m_trackMesh;

            [NonSerialized]
            public Material m_trackMaterial;
            #endregion

            [NonSerialized]
            public NetInfo ParentInfo;

            [NonSerialized]
            public int CachedArrayIndex;

            [NonSerialized]
            public float m_lodRenderDistance;

            [NonSerialized]
            public bool m_requireWindSpeed;

            [NonSerialized]
            public bool m_requireSurfaceMaps;

            [NonSerialized]
            public bool m_requireHeightMap;

            [NonSerialized]
            public bool m_preserveUVs;

            [NonSerialized]
            public bool m_generateTangents;

            [NonSerialized]
            public int m_layer;
            [CustomizableProperty("Vertical Offset")]
            public float VerticalOffset;

            [CustomizableProperty("Anti-flickering")]
            [Hint("moves the tracks up and down by a random few millimeters to avoid z-fighting")]
            public bool AntiFlickering;

            [CustomizableProperty("Scale to lane width")]
            [Hint("the width of the rendered mesh is proportional to the width of the lane.\n" +
                "1 unit in blender means the mesh will be as wide as the lane")]
            public bool ScaleToLaneWidth;

            //[CustomizableProperty("Low Priority")]
            [Hint("Other tracks with DC node take priority")]
            public bool IgnoreDC;

            [CustomizableProperty("Lanes to apply to")]
            [BitMaskLanes]
            public UInt64 LaneIndeces;

            [NonSerialized]
            public int LaneCount;

            public bool HasTrackLane(int laneIndex) => ((1ul << laneIndex) & LaneIndeces) != 0;

            #region flags
            [Hint("Renders on segments + bend nodes)")]
            [CustomizableProperty("Render Segments")]
            public bool RenderSegment = true;

            [Hint("Renders on junction")]
            [CustomizableProperty("Render Junctions")]
            public bool RenderNode = true;

            [Hint("Treats Bend node as segment. if off, bend nodes use junction renderer")]
            [CustomizableProperty("Bend nodes use segment renderer")]
            public bool TreatBendAsSegment = true;
            internal bool TreatBendAsNode => !TreatBendAsSegment;

            //[Hint("Renders only preferred lane transitions - not lane changes. \n" +
            //    "TMPE calculates transition cost. For cars that go straight transition cost is zero.")]
            //[CustomizableProperty("Preferred Transition Only")]
            //public bool RequireMatching = false;

            [CustomizableProperty("Segment")]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Extension")]
            [Hint("checked on segments.\n" +
                "flags for source segment is also checked on all nodes (both junction and bend).")]
            public SegmentInfoFlags SegmentFlags;

            //[CustomizableProperty("Lane")]
            public VanillaLaneInfoFlags VanillaLaneFlags;

            [CustomizableProperty("Lane Extension")]
            [Hint("checked on segment lanes.\n" +
                "flags for source lane is also checked on all nodes (both junction and bend) transitions.")]
            public LaneInfoFlags LaneFlags;

            [CustomizableProperty("Node")]
            public VanillaNodeInfoFlagsLong VanillaNodeFlags;

            [CustomizableProperty("Node Extension")]
            [Hint("Only checked on junctions ")] // (not bend nodes if bend nodes as treated as segments)
            public NodeInfoFlags NodeFlags;

            [CustomizableProperty("Transition")]
            [Hint("TMPE routing between 2 lanes.")] 
            public LaneTransitionInfoFlags LaneTransitionFlags;

            [CustomizableProperty("Tags")]
            public TagsInfo Tags;

            [CustomizableProperty("Lane Tags")]
            [Hint("Match with target lane tags")]
            [NonSerialized]
            [XmlIgnore]
            public LaneTagsT LaneTags = new LaneTagsT(null);

            [CustomizableProperty("Segment Custom Data", "Custom Segment User Data")]
            public UserDataInfo SegmentUserData;

            [CustomizableProperty("Tiling")]
            [Hint("network tiling value (length wise texture scale)")]
            public float Tiling;

            [CustomizableProperty("use NET_SEGMENT keyword")]
            [Hint("This helps to bend wires.")]
            public bool UseKeywordNETSEGMENT = true;

            public bool CheckNodeFlags
                (NetNodeExt.Flags nodeFlags, NetNode.FlagsLong vanillaNodeFlags,
                NetSegmentExt.Flags sourceSegmentFlags, NetSegment.Flags startVanillaSegmentFlags,
                NetLaneExt.Flags laneFalgs, NetLane.Flags vanillaLaneFlags,
                UserData segmentUserData) =>
                RenderNode &&
                NodeFlags.CheckFlags(nodeFlags) && VanillaNodeFlags.CheckFlags(vanillaNodeFlags) &&
                SegmentFlags.CheckFlags(sourceSegmentFlags) && VanillaSegmentFlags.CheckFlags(startVanillaSegmentFlags) &&
                LaneFlags.CheckFlags(laneFalgs) && VanillaLaneFlags.CheckFlags(vanillaLaneFlags) &&
                SegmentUserData.CheckOrNull(segmentUserData);


            public bool CheckSegmentFlags(
                NetSegmentExt.Flags segmentFlags, NetSegment.Flags vanillaSegmentFlags,
                NetLaneExt.Flags laneFalgs, NetLane.Flags vanillaLaneFlags,
                UserData segmentUserData) =>
                RenderSegment &&
                SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags)
                && LaneFlags.CheckFlags(laneFalgs) && VanillaLaneFlags.CheckFlags(vanillaLaneFlags) &&
                SegmentUserData.CheckOrNull(segmentUserData);

            public bool CheckLaneTransitionFlag(LaneTransition.Flags flags) =>
                LaneTransitionFlags.CheckFlags(flags);

            internal CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                Node = NodeFlags.UsedCustomFlags,
                Lane = LaneFlags.UsedCustomFlags,
            };
            #endregion


            /// <summary>
            /// only call in AR mode to allocate arrays for asset editor.
            /// </summary>
            /// <param name="names"></param>
            public void AllocateUserData(UserDataNames names) {
                try {
#if DEBUG
                    Log.Called(names);
#endif
                    SegmentUserData ??= new();
                    SegmentUserData.Allocate(names);
                } catch (Exception ex) {
                    ex.Log();
                }
            }
            public void OptimizeUserData() {
                try {
#if DEBUG
                    Log.Called();
#endif
                    if (SegmentUserData != null && SegmentUserData.IsEmptyOrDefault())
                        SegmentUserData = null;
                } catch (Exception ex) {
                    ex.Log();
                }
            }

        }
    }
}
