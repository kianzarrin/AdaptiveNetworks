namespace AdaptiveRoads.Patches.Parking {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using System.Reflection;
    using UnityEngine;
    using AdaptiveRoads.Manager;
    using KianCommons.Patches;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using ColossalFramework.Math;
    using ICities;

    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch(typeof(PassengerCarAI), "FindParkingSpaceRoadSide")]
    static class ParkingAnglePatch {
        delegate Quaternion LookRotation(Vector3 forward);
        static MethodInfo mLookRotation => TranspilerUtils.DeclaredMethod<LookRotation>(typeof(Quaternion));
        static MethodInfo mGetLaneID_ => typeof(PathManager).GetMethod(nameof(PathManager.GetLaneID), throwOnError: true);
        static MethodInfo mFixValues_ => typeof(ParkingAnglePatch).GetMethod(nameof(FixValues), throwOnError: true);
        static MethodInfo mRotate_ => typeof(ParkingAnglePatch).GetMethod(nameof(Rotate), throwOnError: true);
        internal static float Angle;
        internal static float OneOverCosAngle;

        // this can be done via prefix by calling FindPathPosition and GetLaneID
        // but that would reduce performance in a performance critical part of the code.
        [SuppressMessage("Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "transpiler itself is not performance critocal")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase origin) {
            // inject a call after PathManager.GetLaneID() to fix values.
            foreach (var code in instructions) {
                yield return code;
                if (code.Calls(mGetLaneID_)) {
                    // laneID is already on the stack
                    var LoadRefPathPos = instructions.ToList().Find(c => c.IsLdLocA(typeof(PathUnit.Position), out _));
                    yield return LoadRefPathPos.Clone();
                    yield return new CodeInstruction(OpCodes.Ldarga_S, origin.GetArgLoc("width"));
                    yield return new CodeInstruction(OpCodes.Ldarga_S, origin.GetArgLoc("length"));
                    yield return new CodeInstruction(OpCodes.Call, mFixValues_); // uint FixValues(uint laneID, ref float width, ref float length)
                } else if (code.Calls(mLookRotation)) {
                    // parkRot is already on the stack
                    yield return new CodeInstruction(OpCodes.Call, mRotate_); // Quaternion Rotate(Quaternion parkRot)
                }
            }
        }

        /// <summary>
        /// fixes the width and the length of the bezier that it takes for the car to park.
        /// </summary>
        /// <param name="laneID">this is not the parking lane ID.
        /// this is only a lane on the same side of road that we want to find parking</param>
        /// <param name="pathPos"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static uint FixValues(uint laneID, ref PathUnit.Position pathPos, ref float width, ref float length) {
            var info = pathPos.m_segment.ToSegment().Info;
            var net = info?.GetMetaData();
            Angle = net?.ParkingAngleDegrees ?? 0;
            if (Angle > 30) {
                OneOverCosAngle = net.OneOverCosOfParkingAngle;
                var laneInfo = info.m_lanes[pathPos.m_lane];
                width = FixWidth(length, OneOverCosAngle, laneInfo.m_width);
                length = FixLength(width, OneOverCosAngle);
            }
            return laneID;
        }

        static Quaternion Rotate(Quaternion parkRot) {
            if (Angle != 0)
                parkRot *= Quaternion.Euler(0, Angle, 0);
            return parkRot;
        }

        static float FixWidth(float oneOverCosOfParkingAngle, float carLength, float laneWith) =>
            Mathf.Min(carLength * oneOverCosOfParkingAngle, laneWith);

        static float FixLength(float oneOverCosOfParkingAngle, float carWidth) =>
            carWidth * oneOverCosOfParkingAngle;
    }

    [InGamePatch]
    [UsedImplicitly]
    [HarmonyPatch]
    // angled cars need less gap only for their doors
    static class GapPatch {
        delegate ushort CheckOverlap(ushort ignoreParked, ref Bezier3 bezier, Vector3 pos, Vector3 dir, float offset, float length, ushort otherID, ref VehicleParked otherData, ref bool overlap, ref float minPos, ref float maxPos);
        static FieldInfo fSize_ = typeof(VehicleInfoGen).GetField(nameof(VehicleInfoGen.m_size));
        static MethodInfo mFixGap = typeof(GapPatch).GetMethod(nameof(FixGap), throwOnError: true);
        const float SIDE_GAP = 0.2f;
        static MethodBase TargetMethod() => TranspilerUtils.DeclaredMethod<CheckOverlap>(typeof(PassengerCarAI));
        

        [SuppressMessage("Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "transpiler itself is not performance critocal")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase origin) {
            var codes = instructions.ToList();
            int iLoadSize = codes.Search(c=>c.LoadsField(fSize_, byAddress:true));
            int iLoad1 = codes.Search(c => c.LoadsConstant(1f), startIndex: iLoadSize); // gap between cars
            codes.InsertInstructions(
                iLoad1 + 1,
                new CodeInstruction(OpCodes.Call, mFixGap));
            return codes;
        }

        static float FixGap(float gap) {
            if(ParkingAnglePatch.Angle > 30)
                return SIDE_GAP * ParkingAnglePatch.OneOverCosAngle; 
            else
                return gap;
        }
    }
}