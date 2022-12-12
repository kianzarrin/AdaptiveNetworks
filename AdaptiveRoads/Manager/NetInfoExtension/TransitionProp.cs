namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data;
    using AdaptiveRoads.Data.NetworkExtensions;
    using KianCommons;
    using KianCommons.Serialization;
    using NetworkSkins.Skins.Serialization;
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using static NetLaneProps;

    public static partial class NetInfoExtionsion {
        [Serializable]
        public class TransitionProp : IMetaData {
            #region serialization
            [Obsolete("only useful for the purpose of shallow clone and serialization", error: true)]
            public TransitionProp() { }
            public TransitionProp Clone() {
                var ret = this.ShalowClone();
                ret.Curve = ret.Curve?.ShalowClone();
                ret.SegmentUserData = ret.SegmentUserData?.ShalowClone();
                return ret;
            }

            object ICloneable.Clone() => Clone();

            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) {
                SerializationUtil.GetObjectFields(info, this);
                SerializationUtil.GetObjectProperties(info, this);
            }

            // deserialization
            public TransitionProp(SerializationInfo info, StreamingContext context) {
                SerializationUtil.SetObjectFields(info, this);
                SerializationUtil.SetObjectProperties(info, this);
            }

            static T FindLoaded<T>(string name) where T : PrefabInfo {
                if (name.IsNullorEmpty()) return null;
                return PrefabCollection<T>.FindLoaded(name);
            }

            public string PropInfo {
                get => m_prop?.name;
                set => m_prop = FindLoaded<PropInfo>(value);
            }
            public string TreeInfo {
                get => m_tree?.name;
                set => m_tree = FindLoaded<TreeInfo>(value);
            }
            Vector3Serializable Position {
                get => m_position;
                set => m_position = Position;
            }
            #endregion

            #region flags
            [CustomizableProperty("Node", "Flags")]
            public VanillaNodeInfoFlagsLong VanillaNodeFlags;

            [CustomizableProperty("Node Flags Extension", "Flags")]
            public NodeInfoFlags NodeFlags;

            [CustomizableProperty("Transition Flags", "Flags")]
            public LaneTransitionInfoFlags TransitionFlags;

            [CustomizableProperty("Segment Flags", "Flags")]
            public VanillaSegmentInfoFlags VanillaSegmentFlags;

            [CustomizableProperty("Segment Flags Extension", "Flags")]
            [Hint("Source segment flags")]
            public SegmentInfoFlags SegmentFlags;

            [CustomizableProperty("Segment Custom Data", "Custom Segment User Data")]
            public UserDataInfo SegmentUserData;

            [CustomizableProperty("Segment End Flags", "Flags")]
            [Hint("Source segment flags")]
            public SegmentEndInfoFlags SegmentEndFlags;

            //[CustomizableProperty("Lane", "Flags")]
            //public VanillaLaneInfoFlags VanillaLaneFlags;

            [CustomizableProperty("Lane Flags", "Flags")]
            [Hint("Source lane flags")]
            public LaneInfoFlags LaneFlags;
            #endregion

            [NonSerialized][XmlIgnore]
            public PropInfo m_prop;

            [NonSerialized][XmlIgnore]
            public TreeInfo m_tree;

            [NonSerialized][XmlIgnore]
            public PropInfo m_finalProp;

            [NonSerialized][XmlIgnore]
            public TreeInfo m_finalTree;

            [CustomizableProperty("Position")]
            public Vector3 m_position;

            [CustomizableProperty("Angle")]
            public float m_angle;

            [CustomizableProperty("Segment Offset")]
            public float m_segmentOffset;

            [CustomizableProperty("Repeat Distance")]
            public float m_repeatDistance;

            [CustomizableProperty("Min Length")]
            public float m_minLength;

            [CustomizableProperty("Corner Angle")]
            public float m_cornerAngle;

            [CustomizableProperty("Probability")]
            public int m_probability = 100;

            [CustomizableProperty("Upgradable")]
            public bool m_upgradable;

            [CustomizableProperty("Curve")]
            public Range Curve;

            [Hint("Shift based on track super-elevation.\n" +
                "The amount of shift is proportional to sin(angle) and Catenary height\n" +
                "which can be set in the network properties.")]
            [CustomizableProperty("Catenary")]
            public bool Catenary;

            public bool Check(
                NetNode.FlagsLong vanillaNodeFlags, NetNodeExt.Flags nodeFlags, NetSegmentEnd.Flags segmentEndFlags,
                NetSegment.Flags vanillaSegmentFlags, NetSegmentExt.Flags segmentFlags, UserData segmentUserData,
                LaneTransition.Flags transitionFlags,
                NetLaneExt.Flags laneFlags,
                float laneCurve) =>
                VanillaNodeFlags.CheckFlags(vanillaNodeFlags) && NodeFlags.CheckFlags(nodeFlags) && SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                TransitionFlags.CheckFlags(transitionFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) && VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags) &&
                LaneFlags.CheckFlags(laneFlags) &&
                Curve.CheckRange(laneCurve) &&
                SegmentUserData.CheckOrNull(segmentUserData);


            [NonSerialized2]
            [XmlIgnore]
            internal CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                SegmentEnd = SegmentEndFlags.UsedCustomFlags,
                Node = NodeFlags.UsedCustomFlags,
                Lane = LaneFlags.UsedCustomFlags,
            };


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

            public void Displace(float x) {
                var pos = m_position;
                if (pos.x == 0)
                    return;
                else if (pos.x < 0)
                    pos.x -= x;
                else
                    pos.x += x;
                m_position = pos;
            }
        }
    }
}
