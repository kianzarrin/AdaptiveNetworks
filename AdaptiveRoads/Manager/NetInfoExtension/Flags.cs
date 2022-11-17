namespace AdaptiveRoads.Manager {
    using KianCommons;
    using System;
    using AdaptiveRoads.Data.NetworkExtensions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using AdaptiveRoads.Data.Flags;
    using KianCommons.Serialization;
    using static AdaptiveRoads.Manager.NetInfoExtionsion;
    using System.Xml.Serialization;
    using Epic.OnlineServices.Presence;

    public static partial class NetInfoExtionsion {
        [Serializable]
        public class Range {
            public float Lower, Upper;
            public bool InRange(float value) => Lower <= value && value < Upper;
            public override string ToString() => $"[{Lower}:{Upper})";
        }

        [Serializable]
        [FlagPair]
        public struct VanillaSegmentInfoFlags {
            [BitMask]
            public NetSegment.Flags Required, Forbidden;
            public bool CheckFlags(NetSegment.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [Obsolete("use VanillaNodeInfoFlagsLong instead")]
        //[FlagPair]
        [Serializable]
        public struct VanillaNodeInfoFlags {
            [BitMask]
            public NetNode.Flags Required, Forbidden;
            //public bool CheckFlags(NetNode.Flags flags) => flags.CheckFlags(Required, Forbidden);

            public static explicit operator VanillaNodeInfoFlagsLong(VanillaNodeInfoFlags flags) {
                return new VanillaNodeInfoFlagsLong {
                    Required = (NetNode.FlagsLong)flags.Required,
                    Forbidden = (NetNode.FlagsLong)flags.Forbidden,
                };
            }

            // legacy deserialization.
            public static void SetObjectFields(SerializationInfo info, object target) {
                try {
                    foreach (SerializationEntry item in info) {
                        if (item.Value is VanillaNodeInfoFlags flags) {
                            VanillaNodeInfoFlagsLong val = (VanillaNodeInfoFlagsLong)flags;
                            FieldInfo field = target.GetType().GetField(item.Name, ReflectionHelpers.COPYABLE);
                            if (field != null) {
                                field.SetValue(target, val);
                            }
                        }
                    }
                } catch (Exception ex) { ex.Log(); }
            }
        }

        [FlagPair]
        [Serializable]
        public struct VanillaNodeInfoFlagsLong {
            [BitMask]
            public NetNode.FlagsLong Required, Forbidden;
            public bool CheckFlags(NetNode.FlagsLong flags) => flags.CheckFlags(Required, Forbidden);
        }


        [Serializable]
        [FlagPair]
        public struct VanillaLaneInfoFlags {
            [BitMask]
            public NetLane.Flags Required, Forbidden;
            public bool CheckFlags(NetLane.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetSegment.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetSegmentFlags))]
        [Serializable]
        public struct SegmentInfoFlags {
            [BitMask]
            public NetSegmentExt.Flags Required, Forbidden;
            internal NetSegmentExt.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentExt.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair]
        [Serializable]
        [Hint("segment specific node flags")]
        public struct SegmentEndInfoFlags {
            [BitMask]
            public NetSegmentEnd.Flags Required, Forbidden;
            internal NetSegmentEnd.Flags UsedCustomFlags => (Required | Forbidden) & NetSegmentEnd.Flags.CustomsMask;
            public bool CheckFlags(NetSegmentEnd.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }

        [FlagPair(MergeWithEnum = typeof(NetNode.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetNode.Flags2))]
        [FlagPair(MergeWithEnum = typeof(NetNode.FlagsLong))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlags))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlags2))]
        [FlagPair(MergeWithEnum = typeof(NetNodeFlagsLong))]
        [Serializable]
        public struct NodeInfoFlags {
            public NetNodeExt.Flags Required, Forbidden;
            internal NetNodeExt.Flags UsedCustomFlags => (Required | Forbidden) & NetNodeExt.Flags.CustomsMask;
            public bool CheckFlags(NetNodeExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [FlagPair(MergeWithEnum = typeof(NetLane.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetLaneFlags))]
        [Serializable]
        public struct LaneInfoFlags {
            [BitMask]
            public NetLaneExt.Flags Required, Forbidden;
            internal NetLaneExt.Flags UsedCustomFlags => (Required | Forbidden) & NetLaneExt.Flags.CustomsMask;
            public bool CheckFlags(NetLaneExt.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [FlagPair(MergeWithEnum = typeof(NetLane.Flags))]
        [FlagPair(MergeWithEnum = typeof(NetLaneFlags))]
        [Serializable]
        public struct LaneTransitionInfoFlags {
            [BitMask]
            public LaneTransition.Flags Required, Forbidden;
            public bool CheckFlags(LaneTransition.Flags flags) => flags.CheckFlags(Required, Forbidden);
        }


        [Serializable]
        public struct TagsInfo {
            private static string[] EMPTY => DynamicFlagsUtil.EMPTY_TAGS;
            private static DynamicFlags NONE => DynamicFlagsUtil.NONE;
            public string[] Required = EMPTY, Forbidden = EMPTY;
            public bool ForbidAll = false;
            public byte MinMatch = 0, MaxMatch = 7;
            public byte MinMismatch = 0, MaxMismatch = 7;

            [NonSerialized]
            internal DynamicFlags FlagsRequired = NONE, FlagsForbidden = NONE;
            [NonSerialized]
            private bool needCheck_ = false, needCheckLimits_ = false;

            public TagsInfo() { }

            public void Recalculate() {
                // todo recalculate dynamic flags.
                NetInfo.AddTags(Required);
                NetInfo.AddTags(Forbidden);
                FlagsRequired = NetInfo.GetFlags(Required);
                FlagsForbidden = NetInfo.GetFlags(Forbidden);

                needCheckLimits_ = MaxMismatch < 7 || MaxMatch < 7 || MinMatch > 0 || MinMismatch > 0;
                needCheck_ = needCheckLimits_ || !Required.IsNullorEmpty() || !Forbidden.IsNullorEmpty();
            }

            public bool CheckTags(ushort nodeId) {
                if (!needCheck_) {
                    return true;
                } else if (needCheckLimits_) {
                    return CheckTagsLimit(nodeId);
                } else {
                    return CheckTags(nodeId.ToNode().m_tags);
                }
            }

            public bool CheckTags(ushort nodeId, NetInfo segmentNetInfo) {
                if (!needCheck_) {
                    return true;
                } else if (!needCheckLimits_ || CheckTagsLimit(nodeId)) {
                    return CheckTags(segmentNetInfo.m_netTags);
                } else {
                    return false;
                }
            }

            private bool CheckTags(DynamicFlags flags) =>
                DynamicFlags.Check(flags, FlagsRequired, ForbidAll ? NetInfo.allTags : FlagsForbidden);

            private bool CheckTagsLimit(ushort nodeID) {
                ref NetNode node = ref nodeID.ToNode();
                int nMisMatch = 0;
                int n_match = 0;
                for (int segmentIndex = 0; segmentIndex < 8; segmentIndex++) {
                    ushort segmentId = node.GetSegment(segmentIndex);
                    if (segmentId == 0) continue;
                    ref NetSegment segment = ref segmentId.ToSegment();
                    if (CheckTags(segment.Info.m_netTags)) {
                        n_match++;
                        if (n_match > MaxMatch) {
                            return false;
                        }
                    } else {
                        nMisMatch++;
                        if (nMisMatch > MaxMismatch) {
                            return false;
                        }
                    }
                }
                if (nMisMatch < MinMismatch || n_match < MinMatch) {
                    return false;
                }
                return true;
            }
        }

        public static bool IsNullOrNone(this TagBase tagBase) => tagBase == null || tagBase.IsNone();

        [Serializable]
        public abstract class TagBase : ISerializable {
            public TagBase(string[] tags) {
                Tags = tags ?? DynamicFlagsUtil.EMPTY_TAGS;
                Flags = DynamicFlagsUtil.NONE;
                Recalculate();
            }

            public abstract TagSource Source { get; }

            [NonSerialized]
            [XmlIgnore]
            public DynamicFlags Flags = DynamicFlagsUtil.NONE;

            private string[] Tags = DynamicFlagsUtil.EMPTY_TAGS;

            public void Recalculate() {
                Tags ??= DynamicFlagsUtil.EMPTY_TAGS;
                Source.RegisterTags(Tags);
                Flags = Source.GetFlags(Tags);
                if (Flags.IsEmpty)
                    Flags = DynamicFlagsUtil.NONE; // simplify.
            }

            public virtual bool Check(DynamicFlags flags) => Flags.IsAnyFlagSet(flags);

            public bool IsNone() => Flags.IsEmpty;

            public virtual bool CheckOrNone(DynamicFlags flags) => IsNone() || Flags.IsAnyFlagSet(flags);



            [XmlElement("Tag")]
            public string[] Selected {
                get => Tags ?? DynamicFlagsUtil.EMPTY_TAGS;
                set {
                    Tags = value ?? DynamicFlagsUtil.EMPTY_TAGS;
                    Recalculate();
                }
            }

            public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
                SerializationUtil.GetObjectFields(info, this);
                SerializationUtil.GetObjectProperties(info, this);
            }

            public TagBase(SerializationInfo info, StreamingContext context) {
                SerializationUtil.SetObjectFields(info, this);
                SerializationUtil.SetObjectProperties(info, this);
                Recalculate();
            }
        }

        [Serializable]
        [XmlRoot("LaneTags")]
        public class LaneTagsT : TagBase {
            public static TagSource TagSource = new TagSource();
            public override TagSource Source => TagSource;

            public LaneTagsT(SerializationInfo info, StreamingContext context) : base(info, context) { }
            public LaneTagsT (string []tags) : base(tags){}
            public LaneTagsT Clone() => new LaneTagsT(Selected);

        }

        public class CustomConnectGroupT : TagBase {
            public static TagSource TagSource = new TagSource();
            public override TagSource Source => TagSource;

            public CustomConnectGroupT(SerializationInfo info, StreamingContext context) : base(info, context) { }
            public CustomConnectGroupT(string[] tags) : base(tags) { }
            public CustomConnectGroupT Clone() => new CustomConnectGroupT(Selected);
        }
    }
}


