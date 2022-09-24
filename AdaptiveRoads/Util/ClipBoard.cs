using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using KianCommons;

namespace AdaptiveRoads.Util {
    internal static class ClipBoard {
        private static object Data;
        public static NetInfo SourceInfo { get; private set; }
        public static NetInfo.Lane SourceLane { get; private set; }

        private static void SetSource(object target) {
            if(target == null) {
                return;
            }else if (target is NetLaneProps.Prop prop) {
                SourceInfo = prop.GetParent(out int laneIndex, out _);
                SourceLane = SourceInfo?.m_lanes?[laneIndex];
            } else if( target is NetInfo.Node node) {
                SourceInfo = node.GetParent(out _);
                SourceLane = null;
            } else if (target is NetInfo.Segment segment) {
                SourceInfo = segment.GetParent(out _);
                SourceLane = null;
            } else {
                throw new NotImplementedException(target.ToString());
            }
        }

        public static bool HasData<T>() {
            return Data is T || (Data is IEnumerable<T> e && e.Any()) ;
        }

        public static void SetData(IEnumerable<NetLaneProps.Prop> props) {
            // The output of Select() is only valid as long as props is alive.
            // but if props goes out of scope, I need to have a copy for my self.
            // that is why I convert to array.
            Data = props.Select(prop=>prop.Clone()).ToArray();
            SetSource(props.FirstOrDefault());

            int n = (Data as IEnumerable<NetLaneProps.Prop>).Count();
            Log.Debug("ClipBoard.SetData() -> Data.count=" + n /*+ Environment.StackTrace*/);
        }

        public static void SetData(NetLaneProps.Prop prop) {
            Data = prop.Clone();
            SetSource(prop);
            Log.Debug("ClipBoard.SetData() -> Data=" + Data /*+ Environment.StackTrace*/);
        }

        public static void SetData(IEnumerable<NetInfo.Node> nodes) {
            SetSource(nodes.FirstOrDefault());
            Data = nodes.Select(node => node.Clone()).ToArray();
        }

        public static void SetData(NetInfo.Node node) {
            SetSource(node);
            Data = node.Clone();
        }

        public static void SetData(IEnumerable<NetInfo.Segment> segments) {
            SetSource(segments.FirstOrDefault());
            Data = segments.Select(node => node.Clone()).ToArray();
        }

        public static void SetData(NetInfo.Segment segment) {
            SetSource(segment);
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
            if (Data is IEnumerable<NetInfo.Segment> segments) {
                return segments.Select(segment => segment.Clone()).ToArray();
            }
            return null;
        }
    }
}
