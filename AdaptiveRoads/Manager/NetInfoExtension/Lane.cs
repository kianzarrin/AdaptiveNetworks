namespace AdaptiveRoads.Manager; 
using KianCommons;
using KianCommons.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static AdaptiveRoads.Manager.NetInfoExtionsion;
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

    public class LaneCollection : Dictionary<NetInfo.Lane, Lane> {
        #region Initialization
        public LaneCollection() : base() { }
        public LaneCollection(IDictionary<NetInfo.Lane, Lane> dict) : base(dict) { }

        public LaneCollection Clone() {
            var dict = this.ToDictionary(pair => pair.Key, pair => pair.Value?.Clone());
            return new LaneCollection(dict);
        }
        #endregion

        #region serialization
        public LaneCollection(NetInfo netInfo, List<Lane> lanes) : base() {
            if (netInfo == null) return;
            if (lanes == null) return;
            for (int laneIndex = 0; laneIndex < lanes.Count; ++laneIndex) {
                var laneInfo = netInfo.m_lanes[laneIndex];
                this[laneInfo] = lanes[laneIndex] ?? new(laneInfo);
            }
        }
        public List<Lane> AsList(NetInfo netInfo) {
            List<Lane> ret = new(Count);
            foreach (var laneInfo in netInfo.m_lanes)
                ret.Add(GetOrCreate(laneInfo));
            return ret;
        }
        #endregion


        public new Lane this[NetInfo.Lane laneInfo] {
            get => this.GetorDefault(laneInfo);
            set => base[laneInfo] = value;
        }

        public Lane this[NetInfo netInfo, int laneIndex] => this.GetorDefault(netInfo.m_lanes[laneIndex]);


        public Lane GetOrCreate(NetInfo.Lane laneInfo) => this[laneInfo] ??= new Lane(laneInfo);
    }
}
