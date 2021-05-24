namespace AdaptiveRoads.Patches.Parking {
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System.Reflection;
    using UnityEngine;
    using static ColossalFramework.Math.VectorUtils;
    using AdaptiveRoads.Util;
    using AdaptiveRoads.Manager;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using static KianCommons.Patches.TranspilerUtils;
    using System.Diagnostics;
    using KianCommons.Math;
    using ColossalFramework.Math;

    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch(typeof(PassengerCarAI))]
    static class ParkingAngle {
        delegate bool FindPathPosition(Vector3 position, ItemClass.Service service, NetInfo.LaneType laneType, VehicleInfo.VehicleType vehicleType, bool allowUnderground, bool requireConnect, float maxDistance, out PathUnit.Position pathPos);
        static MethodInfo mFindPathPosition = DeclaredMethod<FindPathPosition>(typeof(PathManager));

        [HarmonyTranspiler]
        [HarmonyPatch("FindParkingSpaceRoadSide")]
        static IEnumerable<CodeInstruction> FindParkingSpaceRoadSideTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach(var code in instructions) {
                yield return code;
                if (code.Calls("Normalize")) {
                    yield return new CodeInstruction(OpCodes.Ldloc_3); // laneID
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 16); // direction (output from CalculatePositionAndDirection)
                    var mRotate = typeof(ParkingAngle).GetMethod(nameof(Rotate), throwOnError: true);
                    yield return new CodeInstruction(OpCodes.Call, mRotate);
                } 
            }
        }

        public static void Rotate(uint laneID, ref Vector3 v) {
            ushort segmentID = laneID.ToLane().m_segment;
            float angle = segmentID.ToSegment().Info.GetMetaData()?.ParkingAngleDegrees ?? 0;
            if (angle == 0)
                return;

            var v2 = v.ToCS2D().RotateRadCCW(-Mathf.Deg2Rad * angle);
            v.x = v2.x;
            v.z = v2.y;
        }

        // patch this :float num8 = (info.m_lanes[(int)pathPos.m_lane].m_width - **width**) * 0.5f;
        public static float Width(float parkingAngleRad, float carLength) => carLength / Mathf.Cos(parkingAngleRad);
        public static float Length(float parkingAngleRad, float carWidth) => carWidth / Mathf.Cos(parkingAngleRad);





    }

    [InGamePatch]
    [UsedImplicitly]
    static class CheckOverlapPatch {
        public delegate bool CheckOverlap(ushort ignoreParked, ref Bezier3 bezier, float offset, float length, out float minPos, out float maxPos);
        static MethodBase TargetMethod() => DeclaredMethod<CheckOverlap>(typeof(PassengerCarAI));

        static void Prefix(ref float length) {

        }
            


    }
}
