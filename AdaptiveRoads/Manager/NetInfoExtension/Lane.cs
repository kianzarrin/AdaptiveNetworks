namespace AdaptiveRoads.Manager {
    using KianCommons;
    using KianCommons.Serialization;
    using System;
    using System.Runtime.Serialization;
    using static KianCommons.ReflectionHelpers;

    public static partial class NetInfoExtionsion {
        [Serializable]
        public class Lane : IMetaData {
            #region serialization
            [Obsolete("only useful for the purpose of shallow clone and serialization", error: true)]
            public Lane() { }
            public Lane Clone() {
                var ret = this.ShalowClone();
                ret.LaneTags = ret.LaneTags.Clone();
                return ret;
            }

            object ICloneable.Clone() => Clone();
            public Lane(NetInfo.Lane template) { }

            //serialization
            public void GetObjectData(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.GetObjectFields(info, this);

            // deserialization
            public Lane(SerializationInfo info, StreamingContext context) =>
                SerializationUtil.SetObjectFields(info, this);
            #endregion

            public LaneTagsT LaneTags = new(null);
        }
    }
}
