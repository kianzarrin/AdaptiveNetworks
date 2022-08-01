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

        public static void SetData(IEnumerable<NetInfo.Node> nodes) {
            Data = nodes.Select(node => node.Clone()).ToArray();
        }

        public static void SetData(NetInfo.Node node) {
            Data = node.Clone();
        }

        public static void SetData(IEnumerable<NetInfo.Segment> segments) {
            Data = segments.Select(node => node.Clone()).ToArray();
        }

        public static void SetData(NetInfo.Segment segment) {
            Data = segment.Clone();
        }

        public static Array GetDataArray() {
            if(Data is NetLaneProps.Prop prop) {
                return new NetLaneProps.Prop[1] { prop.Clone() };
            }
            if(Data is IEnumerable<NetLaneProps.Prop> props) {
                return props.Select(_prop => _prop.Clone()).ToArray();
            }
            if(Data is NetInfo.Node node) {
                return new NetInfo.Node[1] { node };
            }
            if (Data is IEnumerable<NetInfo.Node> nodes) {
                return nodes.Select(node => node.Clone()).ToArray();
            }
            return null;
        }


    }
}
