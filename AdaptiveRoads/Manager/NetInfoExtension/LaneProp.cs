namespace AdaptiveRoads.Manager {
    using AdaptiveRoads.Data;
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Runtime.Serialization;
    using static AdaptiveRoads.UI.ModSettings;
    using static HintExtension;
    using static KianCommons.ReflectionHelpers;

    public static partial class NetInfoExtionsion {
        [AfterField(nameof(NetLaneProps.Prop.m_endFlagsForbidden))]
        [Serializable]
        [Optional(AR_MODE)]
        public class LaneProp : IMetaData {
            #region serialization
            [Obsolete("only useful for the purpose of shallow clone and serialization", error: true)]
            public LaneProp() { }
            public LaneProp Clone() {
                var ret = this.ShalowClone();
                ret.ForwardSpeedLimit = ret.ForwardSpeedLimit?.ShalowClone();
                ret.BackwardSpeedLimit = ret.BackwardSpeedLimit?.ShalowClone();
                ret.LaneSpeedLimit = ret.LaneSpeedLimit?.ShalowClone();
                ret.SegmentCurve = ret.SegmentCurve?.ShalowClone();
                ret.LaneCurve = ret.LaneCurve?.ShalowClone();
                ret.SegmentUserData = ret.SegmentUserData?.ShalowClone();
                return ret;
            }

            object ICloneable.Clone() => Clone();
            public LaneProp(NetLaneProps.Prop template) { }

            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.GetObjectFields(info, this);

            // deserialization
            public LaneProp(SerializationInfo info, StreamingContext context) {
                SerializationUtil.SetObjectFields(info, this);

                // backward compatibility: SpeedLimit, AverageSpeedLimit
                SerializationUtil.SetObjectProperties(info, this);
            }

            [Obsolete("for backward compatibility only", error: true)]
            private Range SpeedLimit {
                set => LaneSpeedLimit = value;
            }

            [Obsolete("for backward compatibility only", error: true)]
            private Range AverageSpeedLimit {
                set => ForwardSpeedLimit = BackwardSpeedLimit = value;
            }

            #endregion

            [CustomizableProperty("Lane")]
            public LaneInfoFlags LaneFlags = new LaneInfoFlags();

            [CustomizableProperty("Tail Node Extension")]
            [Hint(LANE_HEAD_TAIL)]
            public NodeInfoFlags StartNodeFlags = new NodeInfoFlags();

            [CustomizableProperty("Head Node Extension")]
            [Hint(LANE_HEAD_TAIL)]
            public NodeInfoFlags EndNodeFlags = new NodeInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("Segment Tail")]
            [Optional(LANE_SEGMENT_END)]
            public SegmentEndInfoFlags SegmentStartFlags = new SegmentEndInfoFlags();

            [Hint(LANE_HEAD_TAIL)]
            [CustomizableProperty("Segment Head")]
            [Optional(LANE_SEGMENT_END)]
            public SegmentEndInfoFlags SegmentEndFlags = new SegmentEndInfoFlags();

            [CustomizableProperty("Segment")]
            [Optional(LANE_SEGMENT)]
            public VanillaSegmentInfoFlags VanillaSegmentFlags = new VanillaSegmentInfoFlags();

            [CustomizableProperty("Segment Extension")]
            [Optional(LANE_SEGMENT)]
            public SegmentInfoFlags SegmentFlags = new SegmentInfoFlags();

            [CustomizableProperty("Segment Custom Data", "Custom Segment User Data")]
            public UserDataInfo SegmentUserData;

            [CustomizableProperty("Lane Speed Limit Range")]
            public Range LaneSpeedLimit; // null => N/A

            [Hint("Max speed limit of all forward lanes(considering LHT)")]
            [CustomizableProperty("Forward Lanes")]
            public Range ForwardSpeedLimit; // null => N/A

            [Hint("Max speed limit of all backward lanes(considering LHT)")]
            [CustomizableProperty("Backward Lanes")]
            public Range BackwardSpeedLimit; // null => N/A

            [CustomizableProperty("Lane Curve")]
            public Range LaneCurve;

            [CustomizableProperty("Segment Curve")]
            public Range SegmentCurve;

            [Hint("Shift due track super-elevation. " +
                "The amount of shift is proportional to sin(angle) and Catenary height which can be set in the network properties.")]
            [CustomizableProperty("Catenary")]
            public bool Catenary;

            [Hint(
                "initializes random sequence generator (bing it!) when probability < 100%.\n" +
                "props with the same (non-zero) seed appear in the same place.")]
            [CustomizableProperty("Seed")]
            [AfterField(nameof(NetLaneProps.Prop.m_probability))]
            public int SeedIndex;

            /// <param name="laneSpeed">game speed</param>
            /// <param name="forwardSpeedLimit">game speed</param>
            /// <param name="backwardSpeedLimit">game speed</param>
            public bool Check(
                NetLaneExt.Flags laneFlags,
                NetSegmentExt.Flags segmentFlags,
                NetSegment.Flags vanillaSegmentFlags,
                NetNodeExt.Flags startNodeFlags, NetNodeExt.Flags endNodeFlags,
                NetSegmentEnd.Flags segmentStartFlags, NetSegmentEnd.Flags segmentEndFlags,
                float laneSpeed, float forwardSpeedLimit, float backwardSpeedLimit,
                float segmentCurve, float laneCurve, UserData segmentUserData) =>
                LaneFlags.CheckFlags(laneFlags) &&
                SegmentFlags.CheckFlags(segmentFlags) &&
                VanillaSegmentFlags.CheckFlags(vanillaSegmentFlags) &&
                SegmentStartFlags.CheckFlags(segmentStartFlags) &&
                SegmentEndFlags.CheckFlags(segmentEndFlags) &&
                StartNodeFlags.CheckFlags(startNodeFlags) &&
                EndNodeFlags.CheckFlags(endNodeFlags) &&
                LaneSpeedLimit.CheckRange(laneSpeed) &&
                ForwardSpeedLimit.CheckRange(forwardSpeedLimit) &&
                BackwardSpeedLimit.CheckRange(backwardSpeedLimit) &&
                SegmentCurve.CheckRange(segmentCurve) &&
                LaneCurve.CheckRange(laneCurve) &&
                SegmentUserData.CheckOrNull(segmentUserData);

            internal CustomFlags UsedCustomFlags => new CustomFlags {
                Segment = SegmentFlags.UsedCustomFlags,
                SegmentEnd = SegmentStartFlags.UsedCustomFlags | SegmentEndFlags.UsedCustomFlags,
                Lane = LaneFlags.UsedCustomFlags,
                Node = StartNodeFlags.UsedCustomFlags | EndNodeFlags.UsedCustomFlags
            };
            public void AllocateUserData(UserDataNames names) {
#if DEBUG
                Log.Called(names);
#endif
                SegmentUserData ??= new();
                SegmentUserData.Allocate(names);
            }
            public void OptimizeUserData() {
#if DEBUG
                Log.Called();
#endif
                if (SegmentUserData != null && SegmentUserData.IsEmptyOrDefault())
                    SegmentUserData = null;
            }
        }
    }
}
