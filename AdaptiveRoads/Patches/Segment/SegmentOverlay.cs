using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using static KianCommons.Patches.TranspilerUtils;
using KianCommons;
using ColossalFramework.Math;
using System;
using UnityEngine;
using AdaptiveRoads.UI.RoadEditor;

namespace AdaptiveRoads.Patches.Segment {
    public static class SegmentOverlay {
        delegate void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties);
        static MethodInfo mDrawMesh =
            DeclaredMethod<DrawMesh>(typeof(Graphics));

        static MethodInfo mCheckFlags =
            GetMethod(typeof(NetInfo.Segment), nameof(NetInfo.Segment.CheckFlags));

        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            int iDrawMesh = codes.Search(_c => _c.Calls(mDrawMesh));
            int iLdLocSegmentInfo = codes.Search(
                _c => _c.IsLdLoc(typeof(NetInfo.Segment)),
                startIndex: iDrawMesh, count:-1);
            CodeInstruction ldSegmentInfo = codes[iLdLocSegmentInfo].Clone();
            CodeInstruction ldSegmentID = GetLDArg(method, "segmentID");

            int iCheckFlags = codes.Search(_c => _c.Calls(mCheckFlags));
            int iLdaTurnAround = codes.Search(_c =>
                _c.IsLdLocA(typeof(bool), out _),
                startIndex: iCheckFlags, count: -1);
            int locTurnAround = (codes[iLdaTurnAround].operand as LocalBuilder).LocalIndex;
            CodeInstruction ldTurnAround = new CodeInstruction(OpCodes.Ldloc_S, locTurnAround);

            var insertion = new[]{
                ldSegmentID,
                ldSegmentInfo,
                ldTurnAround,
                new CodeInstruction(OpCodes.Call, mOnAfterDrawMesh)
            };
            codes.InsertInstructions(iDrawMesh + 1, insertion, moveLabels: false);
        }

        static MethodInfo mOnAfterDrawMesh =
            GetMethod(typeof(SegmentOverlay), nameof(OnAfterDrawMesh));
        public static void OnAfterDrawMesh(
            ushort segmentID,
            NetInfo.Segment segmentInfo,
            bool turnAround) { 
            if(segmentInfo == Overlay.SegmentInfo) {
                var data = new Overlay.SegmentData {
                    SegmentID = segmentID,
                    TurnAround = turnAround,
                };
                Overlay.SegmentQueue.Enqueue(data);
            }
        }
    }
}
