namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
    using TrafficManager.API.Geometry;
    using TrafficManager.API.Manager;
    using TrafficManager.API.Traffic.Data;
    using TrafficManager.API.Util;

    internal class ARTMPEObsever : IObserver<GeometryUpdate> {
        NetworkExtensionManager neMan => NetworkExtensionManager.Instance;
        void IObserver<GeometryUpdate>.OnUpdate(GeometryUpdate subject) {
            if (subject.nodeId is ushort nodeID)
                neMan.UpdateNode(nodeID);
            if (subject.segment is ExtSegment segmentExt)
                neMan.UpdateSegment(segmentExt.segmentId);
            if (subject.replacement is SegmentEndReplacement replacement)
                neMan.UpdateSegment(replacement.newSegmentEndId.SegmentId);
        }
    }
}
