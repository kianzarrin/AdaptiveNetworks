namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
    using TrafficManager.API.Geometry;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Util;
    using KianCommons;
    using System;
    using TrafficManager.API.Traffic;

    internal class ARTMPEObsever : IObserver<GeometryUpdate> {
        NetworkExtensionManager netMan => NetworkExtensionManager.Instance;
        void IObserver<GeometryUpdate>.OnUpdate(GeometryUpdate subject) {
            try {
                if (subject.nodeId is ushort nodeID)
                    netMan.UpdateNode(nodeID);
                if (subject.segment is ExtSegment segmentExt)
                    netMan.UpdateSegment(segmentExt.segmentId);
                if (subject.replacement.newSegmentEndId is ISegmentEndId newSegmentEndId) 
                    netMan.UpdateSegment(newSegmentEndId.SegmentId);
            } catch(Exception ex) {
                ex.Log();
            }
        }
    }
}
