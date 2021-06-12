namespace AdaptiveRoads.Util {
    using KianCommons;
    using TrafficManager.Manager.Impl;
    using System.Diagnostics;
    using TrafficManager.API.Manager;
    using System.Collections.Generic;
    using System.Linq;
    using TrafficManager.API.Traffic.Enums;
    using AdaptiveRoads.Manager;
    using CSUtil.Commons;
    using UnityEngine;
    using static KianCommons.DirectionUtil;

    static class ArrowDirectionUtil {
        internal static NetLaneExt.Flags GetArrowExt(ushort segmentId1, ushort segmentId2) {
            ushort nodeID = NetUtil.GetSharedNode(segmentId1, segmentId2);
            if (nodeID == 0) return 0;
            return CalculateArrowExt(
                segmentId1.ToSegment().GetDirection(nodeID),
                segmentId2.ToSegment().GetDirection(nodeID));
        }

        internal static NetLaneExt.Flags CalculateArrowExt(Vector3 sourceDir, Vector3 targetDir) {
            sourceDir.y = 0;
            sourceDir.Normalize();

            targetDir.y = 0;
            targetDir.Normalize();
            float c = Vector3.Cross(sourceDir, targetDir).y;
            float d = Vector3.Dot(sourceDir, targetDir);

            if (Mathf.Abs(c) < 9.9999994E-11f) // epsilon
                return NetLaneExt.Flags.UTurn;

            if (d > 0.5f) {
                // sharp : (-60° : +60°)
                if (c > 0)
                    return NetLaneExt.Flags.LeftSharp; // (+0° : +60°)
                else
                    return NetLaneExt.Flags.RightSharp; // (-0° : -60°)
            } else if (d > -0.5f) {
                // normal : [+-60° : +-120°]
                if (c > 0)
                    return NetLaneExt.Flags.LeftModerate; // [+60° : +120°]
                else
                    return NetLaneExt.Flags.RightModerate; // [-60° : -120°]
            } else if (c >= 0.5f) {
                return NetLaneExt.Flags.LeftSlight; // (+60° : +120°]
            } else if (c <= 0.5f) {
                return NetLaneExt.Flags.RightSlight; // (-60° : -120°]
            }
            return 0; // forward
        }
    }
}
