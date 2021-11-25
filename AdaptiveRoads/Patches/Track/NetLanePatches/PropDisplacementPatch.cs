using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using KianCommons;
using KianCommons.Patches;
using static KianCommons.ReflectionHelpers;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Data.NetworkExtensions;

namespace AdaptiveRoads.Patches.Track.NetLanePatches {
    [HarmonyPatch]
    [InGamePatch]
    public static class PropDisplacementPatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return typeof(NetLane).GetMethod(nameof(NetLane.RenderInstance), BindingFlags.Public | BindingFlags.Instance,true);
            yield return typeof(NetLane).GetMethod(nameof(NetLane.PopulateGroupData), BindingFlags.Public | BindingFlags.Instance,true);
        }

        static MethodInfo mCalculatePropPos = typeof(PropDisplacementPatch).GetMethod(nameof(CalculatePropPos), throwOnError: true);

        static MethodInfo mPosition =
            typeof(Bezier3).GetMethod(nameof(Bezier3.Position), BindingFlags.Public | BindingFlags.Instance, throwOnError: true);

        static MethodInfo mTangent =
            typeof(Bezier3).GetMethod(nameof(Bezier3.Tangent), BindingFlags.Public | BindingFlags.Instance, true);

        static FieldInfo fX = GetField<Vector3>("x");
        static FieldInfo fY = GetField<Vector3>("y");
        static FieldInfo fZ = GetField<Vector3>("z");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase origin) { 
            try {
                var codes = instructions.ToCodeList();
                var LDLaneID = TranspilerUtils.GetLDArg(origin, "laneID");
                var LDLaneInfo = TranspilerUtils.GetLDArg(origin, "laneInfo");

                int iLoadLaneProp = codes.Search(c => c.IsLdLoc(typeof(NetLaneProps.Prop), origin));
                Assertion.GTEq(iLoadLaneProp, 0, "iLoadLaneProp");
                var LoadLaneProp = codes[iLoadLaneProp].Clone();

                int iCallPos = codes.Search(c => c.Calls(mPosition));
                Assertion.GTEq(iCallPos, 0, "iCallPos");
                int iStorePos = codes.Search(c => c.IsStLoc(typeof(Vector3), origin), startIndex: iCallPos);
                Assertion.GTEq(iStorePos, 0, "iStorePos");
                codes[iStorePos].IsStLoc(out int locPos);
                Assertion.GTEq(locPos, 0, "locPos");
                var LoadRefPos = new CodeInstruction(OpCodes.Ldloca_S, locPos);

                int iLoadOffset = codes.Search(c => c.IsLdLoc(typeof(float), origin), startIndex: iCallPos, count: -1);
                Assertion.GTEq(iLoadOffset, 0, "iLoadOffset");
                var LoadOffset = codes[iLoadOffset].Clone();

                int iCallTan = codes.Search(c => c.Calls(mTangent));
                Assertion.GTEq(iCallPos, 0, "iCallTan");
                int iStoreTan = codes.Search(c => c.IsStLoc(typeof(Vector3), origin), startIndex: iCallTan);
                Assertion.GTEq(iStoreTan, 0, "iStoreTan");
                var LoadTan = codes[iStoreTan].BuildLdLocFromStLoc();


                codes.InsertInstructions(
                    iStoreTan + 1, //after storing tangent
                    new [] {
                        LoadRefPos,
                        LoadTan,
                        LoadOffset,
                        LDLaneID,
                        LDLaneInfo,
                        LoadLaneProp,
                        new CodeInstruction(OpCodes.Call, mCalculatePropPos),
                    },
                    moveLabels:false);
                return codes;
            } catch(Exception ex) {
                ex.Log();
                return instructions;
            }
        }

        public static void CalculatePropPos(
            ref Vector3 pos, Vector3 tan, float offset,
            uint laneID, NetInfo.Lane laneInfo,
            NetLaneProps.Prop laneProp) {
            //Log.Called(offset, "lane:" + laneID, "lane pos:" + laneInfo.m_position, laneProp?.m_finalProp);
            var propExt = laneProp?.GetMetaData();
            bool catenary = propExt != null && propExt.Catenary;
            Log.Debug($"catenary={catenary}", false);
            if(!catenary) return;

            ushort segmentID = laneID.ToLane().m_segment;
            Log.Debug($"segmentID={segmentID}", false);

            ref var segExt = ref segmentID.ToSegmentExt();

            var normalCW = VectorUtils.NormalizeXZ(new Vector3(-tan.z, 0, tan.x));
            float angleStart = segExt.Start.TotalAngle;
            float angleEnd = -segExt.End.TotalAngle; // end angle needs minus
            float angle = Mathf.Lerp(angleStart, angleEnd, offset);
            float shift = segExt.NetInfoExt.CatenaryHeight * Mathf.Sin(angle);

            pos += shift * normalCW;
            //bool reverse = laneInfo.IsGoingBackward(segmentID.ToSegment().IsInvert());
        }

    }
}
