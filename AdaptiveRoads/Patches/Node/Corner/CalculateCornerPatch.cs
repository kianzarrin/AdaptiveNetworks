namespace AdaptiveRoads.Patches.Corner; 
using AdaptiveRoads.Util;
using HarmonyLib;
using JetBrains.Annotations;
using KianCommons;
using System.Reflection;
using UnityEngine;
using AdaptiveRoads.Manager;
using KianCommons.Math;
using ColossalFramework.Math;

/// <summary>
/// Shifting needs to happen as early as possible because it is more than just a visual fix. it determines the position of the cars.
/// </summary>
[PreloadPatch]
[HarmonyPatch]
public static class CalculateCornerPatch {
    [UsedImplicitly]
    [HarmonyBefore("CS.Kian.NodeController")]
    static MethodBase TargetMethod() {
        // public void CalculateCorner(ushort segmentID, bool heightOffset, bool start, bool leftSide,
        // out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
        return typeof(NetSegment).GetMethod(
                nameof(NetSegment.CalculateCorner),
                BindingFlags.Public | BindingFlags.Instance, throwOnError: true);
    }

    /// <param name="segmentID">segment to calculate corner</param>
    /// <param name="start">true for start node</param>
    /// <param name="leftSide">going away from the node</param>
    public static void Postfix(bool __runOriginal,
        ushort segmentID, bool start, bool leftSide,
        ref Vector3 cornerPos, ref Vector3 cornerDirection) {
        Shift(
            segmentID: segmentID, start: start, leftSide: leftSide,
            cornerPos: ref cornerPos, cornerDirection: ref cornerDirection);
    }

    public static void Shift(
        ushort segmentID, bool start, bool leftSide,
        ref Vector3 cornerPos, ref Vector3 cornerDirection) {
        ref NetSegment segment = ref segmentID.ToSegment();
        ushort nodeId = segment.GetNode(start);
        float shift = API.GetShift(segmentId: segmentID, nodeId: nodeId);
        if (shift == 0) return;

        var righward = CornerUtil.CalculateRighwardNormal(cornerDirection);
        bool headNode = segment.GetHeadNode() == nodeId;
        if (headNode)
            shift = -shift;
        cornerPos += shift * righward;
    }
}
