using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using KianCommons;

namespace AdaptiveRoads.Util {
    internal static class ClipBoard {
        private static object Data;

        public static bool HasData<T>() {
            return Data is T || (Data is IEnumerable<T> e && e.Any()) ;
        }

        public static void SetData(IEnumerable<NetLaneProps.Prop> props) {
            // The output of Select() is only valid as long as props is alive.
            // but if props goes out of scope, I need to have a copy for my self.
            // that is why I convert to array.
            Data = props.Select(prop=>prop.Clone()).ToArray(); 
            int n = (Data as IEnumerable<NetLaneProps.Prop>).Count();
            Log.Debug("ClipBoard.SetData() -> Data.count=" + n /*+ Environment.StackTrace*/);
        }

        public static void SetData(NetLaneProps.Prop prop) {
            Data = prop.Clone();
            Log.Debug("ClipBoard.SetData() -> Data=" + Data /*+ Environment.StackTrace*/);
        }

        public static Array GetDataArray() {
            if(Data is NetLaneProps.Prop prop) {
                return new NetLaneProps.Prop[] { prop.Clone() };
            }
            if(Data is IEnumerable<NetLaneProps.Prop> e) {
                return e.Select(_prop => _prop.Clone()).ToArray();
            }
            return null;
        }


    }
}
