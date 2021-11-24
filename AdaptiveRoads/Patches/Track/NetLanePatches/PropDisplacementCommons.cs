using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static KianCommons.Patches.TranspilerUtils;
using static KianCommons.Assertion;
using KianCommons;
using ColossalFramework;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Data.NetworkExtensions;
using KianCommons.Patches;

namespace AdaptiveRoads.Patches.Track.NetLanePatches {
    public static class PropDisplacementCommons {
        // modifies x pos.
        // to modify y pos uncomment this in the transpiler: `c1.operand == fY`
        public static Vector3 CalculatePropPos(ref Vector3 pos0, float t, uint laneID, NetInfo.Lane laneInfo, NetLaneProps.Prop laneProp) {
            var propExt = laneProp?.GetMetaData();
            bool catenary = propExt != null && propExt.Catenary;
            if(!catenary) return pos0;

            Vector3 pos = pos0; // don't modify the original.
            ushort segmentID = laneID.ToLane().m_segment;

            ref var segExt = ref segmentID.ToSegmentExt();

            float angleStart = segExt.Start.TotalAngle;
            float angleEnd = segExt.End.TotalAngle;
            float angle = Mathf.Lerp(angleStart, angleEnd, t);
            float shift = OutlineData.WIRE_HEIGHT * Mathf.Sin(angle);

            //pos.x += shift;
            bool reverse = laneInfo.IsGoingBackward() != segmentID.ToSegment().IsInvert();
            if(reverse)
                pos.x += shift;
            else
                pos.y -= shift;
            return pos;
        }

        static MethodInfo mCalculatePropPos = typeof(PropDisplacementCommons).GetMethod(nameof(CalculatePropPos)) ??
            throw new Exception("mCalculatePropPos is null");

        static FieldInfo fPosition = typeof(NetLaneProps.Prop).
                              GetField(nameof(NetLaneProps.Prop.m_position)) ??
                              throw new Exception("fPosition is null");
        static FieldInfo fX = typeof(Vector3).GetField("x") ?? throw new Exception("fX is null");
        static FieldInfo fY = typeof(Vector3).GetField("y") ?? throw new Exception("fY is null");

        static MethodInfo mPosition =
            typeof(Bezier3)
            .GetMethod(nameof(Bezier3.Position), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new Exception("mPosition is null");

        public static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions, MethodInfo method) {
            try {
                var codes = instructions.ToCodeList();
                bool predicate(int i) {
                    if (i + 2 >= codes.Count) return false;
                    // m_position.x || m_position.z
                    var c0 = codes[i];
                    var c1 = codes[i + 1];
                    var c2 = codes[i + 2];
                    bool ret = c0.operand == fPosition;
                    ret &= c1.operand == fX /*|| c1.operand == fY*/; // uncomment to modify height
                    ret &= c2.opcode == OpCodes.Mul || c2.opcode == OpCodes.Add; // ignore if(pos.x != 0)
                    return ret;
                }



                int index = 0;
                int nInsertions = 0;
                for (int watchdog = 0; ; ++watchdog) {
                    Assert(watchdog < 20, "watchdog");
                    int c = 1;//watchdog == 0 ? 1 : 2; // skip over m_position from previous loop.
                    index = SearchGeneric(codes, predicate, index, throwOnError: false, counter: c);
                    if (index < 0) break; // not found
                    index++; // insert after
                    bool inserted = InsertCall(codes, index, method);
                    if (inserted) nInsertions++;
                }

                //Log.DebugWait($"successfully inserted {nInsertions} calls to CalculatePropPos() in {method.DeclaringType.Name}.{method.Name}");
                return codes;
            }
            catch (Exception e) {
                Log.Exception(e);
                return instructions;
            }
        }

        public static bool InsertCall(List<CodeInstruction> codes, int index, MethodInfo method) {
            CodeInstruction LDLaneID = GetLDArg(method, "laneID");
            CodeInstruction LDLaneInfo = GetLDArg(method, "laneInfo");
            CodeInstruction LDOffset = GetLDOffset(codes, index);
            CodeInstruction LDLaneProp = GetLDProp(method, codes, index);

            if(LDOffset == null)
                return false; // silently return if no offset could be found.

            var insertion = new CodeInstruction[] {
                LDOffset,
                LDLaneID,
                LDLaneInfo,
                LDLaneProp,
                new CodeInstruction(OpCodes.Call, mCalculatePropPos)
            };

            // insert after ldflda prop.m_posion
            InsertInstructions(codes, insertion, index);
            return true;
        }

        /// <summary>
        /// finds the previous call to  prop.m_position.something
        /// and returns a duplicate instruction that loads address to m_position.
        /// </summary>
        public static CodeInstruction GetLDOffset(List<CodeInstruction> codes, int index) {
            index = SearchGeneric(codes, i => codes[i].operand == mPosition, index: index, dir: -1, throwOnError:false);
            index--; // previous instructions should put offset into stack

            if (index < 0) // not found
                return null;

            Assert(codes[index].IsLdloc(), $"{codes[index]}.IsLdloc()"); 
            return new CodeInstruction(codes[index].opcode, codes[index].operand);
        }

        /// <summary>
        /// finds the prev call to load prop (NetLaneProps.Prop) as a local variable
        /// and returns a duplicate of the instruction that loads prop.
        /// </summary>
        public static CodeInstruction GetLDProp(MethodBase method, List<CodeInstruction> codes, int index) {
            int iLdProp = codes.Search(
                _c => _c.IsLdLoc(typeof(NetLaneProps.Prop), method),
                startIndex: index, count: -1);
            if(index < 0) return null;// not found
            return codes[iLdProp].Clone();
        }

    }
}
