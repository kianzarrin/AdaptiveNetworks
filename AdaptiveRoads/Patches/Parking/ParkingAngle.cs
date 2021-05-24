namespace AdaptiveRoads.Patches.Parking {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System.Reflection;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Linq;

    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch(typeof(PassengerCarAI), "FindParkingSpaceRoadSide")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "not performance critical")]
    static class ParkingAngle {
        //delegate bool FindPathPosition(Vector3 position, ItemClass.Service service, NetInfo.LaneType laneType, VehicleInfo.VehicleType vehicleType, bool allowUnderground, bool requireConnect, float maxDistance, out PathUnit.Position pathPos);
        //static MethodInfo mFindPathPosition = DeclaredMethod<FindPathPosition>(typeof(PathManager));
        static MethodInfo mGetLaneID => typeof(PathManager).GetMethod(nameof(PathManager.GetLaneID), throwOnError: true);
        static MethodInfo mFixValues => typeof(ParkingAngle).GetMethod(nameof(ParkingAngle.FixValues), throwOnError: true);

        static float angle_;
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase origin) {
            try {
                var codes = instructions.ToList();
                var loadRefPathPos = codes.Find(c => c.IsLdLocA(typeof(PathUnit.Position), out _));
                var loadRefWidth = new CodeInstruction(OpCodes.Ldarga_S, origin.GetArgLoc("width"));
                var loadRefLength = new CodeInstruction(OpCodes.Ldarga_S, origin.GetArgLoc("length"));
                var callFixValues = new CodeInstruction(OpCodes.Call, mFixValues);
                var iCallGetLaneID = codes.FindIndex(c => c.Calls(mGetLaneID));
                codes.InsertInstructions(iCallGetLaneID, new CodeInstruction[]{
                    // laneID is already on the stack
                    loadRefPathPos.Clone(),
                    loadRefWidth,
                    loadRefLength,
                    callFixValues,
                });
                return codes;
            } catch (Exception ex) {
                ex.Log();
                return instructions;
            }
        }

        static void Postfix(ref Quaternion parkRot) {
            if (angle_ != 0) {
                Rotate(ref parkRot, angle_);
            }
            angle_ = 0;
        }

        public static uint FixValues(uint laneID, ref PathUnit.Position pathPos, ref float width, ref float length) {
            angle_ = laneID.ToLane().m_segment.ToSegment().Info.GetMetaData()?.ParkingAngleDegrees ?? 0;
            if (angle_ > 30) {
                width = FixWidth(length, angle_);
                length = FixLength(width, angle_);
            }
            return laneID;
        }

        public static void Rotate(ref Quaternion parkRot, float parkingAngleRad) =>
            parkRot *= Quaternion.Euler(0, parkingAngleRad, 0);

        // patch this :float num8 = (info.m_lanes[(int)pathPos.m_lane].m_width - **width**) * 0.5f;
        public static float FixWidth(float parkingAngleRad, float carLength) => carLength / Mathf.Cos(parkingAngleRad);
        public static float FixLength(float parkingAngleRad, float carWidth) => carWidth / Mathf.Cos(parkingAngleRad);

    }
}
