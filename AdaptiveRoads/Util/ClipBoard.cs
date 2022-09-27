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
                Log.Debug($"SourceInfo set to {SourceInfo}, laneInex={laneIndex}");
            } else if( target is NetInfo.Node node) {
                SourceInfo = node.GetParent(out _);
                SourceLane = null;
                Log.Debug($"SourceInfo set to {SourceInfo}");
            } else if (target is NetInfo.Segment segment) {
                SourceInfo = segment.GetParent(out _);
                SourceLane = null;
                Log.Debug($"SourceInfo set to {SourceInfo}");
            } else {
                throw new NotImplementedException(target.ToString());
            }
        }

        public static int Count { get; private set; }

        public static bool HasData<T>() {
            return Data is T || (Data is IEnumerable<T> e && e.Any()) ;
        }

        public static void SetData(IEnumerable<NetLaneProps.Prop> props) {
            // The output of Select() is only valid as long as props is alive.
            // but if props goes out of scope, I need to have a copy for my self.
            // that is why I convert to array.
            SetSource(props.FirstOrDefault());
            var data = props.Select(prop => prop.Clone()).ToArray();
            Data = data;
            Count = data.Length;
            Log.Debug("ClipBoard.SetData() -> Data.count=" + Count /*+ Environment.StackTrace*/);
        }

        public static void SetData(NetLaneProps.Prop prop) {
            SetSource(prop);
            Data = prop.Clone();
            Count = 1;
            Log.Debug("ClipBoard.SetData() -> Data=" + Data /*+ Environment.StackTrace*/);
        }

        public static void SetData(IEnumerable<NetInfo.Node> nodes) {
            SetSource(nodes.FirstOrDefault());
            var data = nodes.Select(node => node.Clone()).ToArray();
            Data = data;
            Count = data.Length;
        }

        public static void SetData(NetInfo.Node node) {
            SetSource(node);
            Data = node.Clone();
            Count = 1;
        }

        public static void SetData(IEnumerable<NetInfo.Segment> segments) {
            SetSource(segments.FirstOrDefault());
            Data = segments.Select(node => node.Clone()).ToArray();
            Count = (Data as Array).Length;
        }

        public static void SetData(NetInfo.Segment segment) {
            SetSource(segment);
            Data = segment.Clone();
            Count = 1;
        }

        public static Array GetDataArray() {
            return
                (Array)GetProps() ??
                (Array)GetNodes() ??
                (Array)GetSegments();
        }

        public static NetLaneProps.Prop[] GetProps() {
            if (Data is NetLaneProps.Prop prop) {
                return new NetLaneProps.Prop[1] { prop.Clone() };
            } else if (Data is IEnumerable<NetLaneProps.Prop> props) {
                return props.Select(_prop => _prop.Clone()).ToArray();
            }
            return null;
        }
        public static NetInfo.Node[] GetNodes() {
            if (Data is NetInfo.Node node) {
                return new NetInfo.Node[1] { node.Clone() };
            } else if (Data is IEnumerable<NetInfo.Node> nodes) {
                return nodes.Select(node => node.Clone()).ToArray();
            }
            return null;
        }
        public static NetInfo.Segment[] GetSegments() {
            if (Data is NetInfo.Segment segment) {
                return new NetInfo.Segment[1] { segment.Clone() };
            } else if (Data is IEnumerable<NetInfo.Segment> segments) {
                return segments.Select(segment => segment.Clone()).ToArray();
            }
            return null;
        }
    }
}
