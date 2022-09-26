namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data;
    using AdaptiveRoads.LifeCycle;
    using AdaptiveRoads.Util;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using UnityEngine;
    using static AdaptiveRoads.UI.ModSettings;

    public static partial class NetInfoExtionsion {
        [AfterField(nameof(NetInfo.Segment.m_backwardForbidden))]
        [Serializable]
        [Optional(AR_MODE)]
        public class Segment : IMetaData, IModel, ISerializable {
            object ICloneable.Clone() => Clone();

            [AfterField(nameof(NetInfo.Segment.m_forwardForbidden))]
            [CustomizableProperty("Forward Extension")]
            public SegmentInfoFlags Forward;

            [CustomizableProperty("Backward Extension")]
            public SegmentInfoFlags Backward;

            [CustomizableProperty("Tail Node")]
            [Optional(SEGMENT_NODE)]
            public VanillaNodeInfoFlagsLong VanillaTailNode;

            [CustomizableProperty("Tail Node Extension")]
            [Optional(SEGMENT_NODE)]
            public NodeInfoFlags TailtNode;

            [CustomizableProperty("Head Node")]
            [Optional(SEGMENT_NODE)]
            public VanillaNodeInfoFlagsLong VanillaHeadNode;

            [CustomizableProperty("Head Node Extension")]
            [Optional(SEGMENT_NODE)]
            public NodeInfoFlags HeadNode;

            [CustomizableProperty("Segment Tail")]
            [Optional(SEGMENT_SEGMENT_END)]
            public SegmentEndInfoFlags Tail;

            [CustomizableProperty("Segment Head")]
            [Optional(SEGMENT_SEGMENT_END)]
            public SegmentEndInfoFlags Head;

            [CustomizableProperty("Custom Data", "Custom User Data")]
            public UserDataInfo UserData;

            [CustomizableProperty("Tiling")]
            [Hint("network tiling value")]
            public float Tiling;

            [CustomizableProperty("Title")]
            [Hint("title to display(asset editor only)")]
            [AfterField(nameof(NetInfo.Node.m_directConnect))]
            public string Title;
            string IModel.Title => Title;

            public bool CheckEndFlags(
                    NetSegmentEnd.Flags tailFlags,
                    NetSegmentEnd.Flags headFlags,
                    NetNode.FlagsLong tailNodeFlags,
                    NetNode.FlagsLong headNodeFlags,
                    NetNodeExt.Flags tailNodeExtFlags,
                    NetNodeExt.Flags headNodeExtFlags) {
                return
                    Tail.CheckFlags(tailFlags) &
                    Head.CheckFlags(headFlags) &
                    VanillaTailNode.CheckFlags(tailNodeFlags) &
                    VanillaHeadNode.CheckFlags(headNodeFlags) &
                    TailtNode.CheckFlags(tailNodeExtFlags) &
                    HeadNode.CheckFlags(headNodeExtFlags);
            }

            public bool CheckFlags(NetSegmentExt.Flags flags,
                    NetSegmentEnd.Flags tailFlags,
                    NetSegmentEnd.Flags headFlags,
                    NetNode.FlagsLong tailNodeFlags,
                    NetNode.FlagsLong headNodeFlags,
                    NetNodeExt.Flags tailNodeExtFlags,
                    NetNodeExt.Flags headNodeExtFlags,
                    UserData userData,
                    bool turnAround) {
                bool ret;
                if(!turnAround) {
                    ret =Forward.CheckFlags(flags) && CheckEndFlags(
                        tailFlags: tailFlags,
                        headFlags: headFlags,
                        tailNodeFlags: tailNodeFlags,
                        headNodeFlags: headNodeFlags,
                        tailNodeExtFlags: tailNodeExtFlags,
                        headNodeExtFlags: headNodeExtFlags);
                } else {
                    Helpers.Swap(ref tailFlags, ref headFlags);
                    Helpers.Swap(ref tailNodeFlags, ref headNodeFlags);
                    ret = Backward.CheckFlags(flags) && CheckEndFlags(
                        tailFlags: tailFlags,
                        headFlags: headFlags,
                        tailNodeFlags: tailNodeFlags,
                        headNodeFlags: headNodeFlags,
                        tailNodeExtFlags: tailNodeExtFlags,
                        headNodeExtFlags: headNodeExtFlags);
                }

                ret = ret && UserData.CheckOrNull(userData);

                return ret;
            }

            internal CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = Forward.UsedCustomFlags | Backward.UsedCustomFlags,
                SegmentEnd = Head.UsedCustomFlags | Tail.UsedCustomFlags,
                Node = HeadNode.UsedCustomFlags | TailtNode.UsedCustomFlags
            };

            [Obsolete("only useful for the purpose of shallow clone", error: true)]
            public Segment() { }
            public Segment Clone() {
                var ret = this.ShalowClone();
                ret.UserData = ret.UserData?.ShalowClone();
                return ret;
            }
            public Segment(NetInfo.Segment template) { }

            #region serialization
            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) {
                OptimizeUserData(); // avoid saving redundant stuff.
                SerializationUtil.GetObjectFields(info, this);
            }

            // deserialization
            public Segment(SerializationInfo info, StreamingContext context) {
                SerializationUtil.SetObjectFields(info, this, silentInvalidCast:true);;
#pragma warning disable CS0618 // Type or member is obsolete
                VanillaNodeInfoFlags.SetObjectFields(info, this);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            #endregion

            public void SetupTiling(NetInfo.Segment segmentInfo) {
                if(Tiling != 0) {
                    segmentInfo.m_material?.SetTiling(Tiling);
                    segmentInfo.m_segmentMaterial?.SetTiling(Tiling);
                    segmentInfo.m_lodMaterial?.SetTiling(Tiling);
                    segmentInfo.m_combinedLod?.m_material?.SetTiling(Mathf.Abs(Tiling));
                }
            }

            /// <summary>
            /// only call in AR mode to allocate arrays for asset editor.
            /// </summary>
            /// <param name="names"></param>
            public void AllocateUserData(UserDataNames names) {
#if DEBUG
                Log.Called(names);
#endif
                UserData ??= new();
                UserData.Allocate(names);
            }
            public void OptimizeUserData() {
#if DEBUG
                Log.Called();
#endif
                if (UserData != null && UserData.IsEmptyOrDefault())
                    UserData = null;
            }
        }
    }
}
