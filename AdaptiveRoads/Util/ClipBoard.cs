using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;

namespace AdaptiveRoads.Util {
    internal static class ClipBoard {
        private static object Data;

        public static bool HasData<T>() {
            return Data is T || (Data is IEnumerable<T>) ;
        }

        public static void SetData(IEnumerable<NetLaneProps.Prop> props) {
            Data = props.Select(prop=>prop.Clone());
        }
        public static void SetData(NetLaneProps.Prop prop) {
            Data = prop.Clone();
        }

        public static Array GetDataArray() {
            if(Data is NetLaneProps.Prop prop) {
                return new NetLaneProps.Prop[] { prop.Clone() };
            }
            if(Data is IEnumerable<NetLaneProps.Prop> ar) {
                return ar.Select(_prop => _prop.Clone()).ToArray();
            }
            return null;
        }


    }
}
