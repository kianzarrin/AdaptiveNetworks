using AdaptiveRoads.UI.RoadEditor;
using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static KianCommons.Patches.TranspilerUtils;

namespace AdaptiveRoads.Patches.Node {
    public static class NodeOverlay {
        delegate void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties);
        static MethodInfo mDrawMesh =
            DeclaredMethod<DrawMesh>(typeof(Graphics));

        static MethodInfo mCheckFlags =
            GetMethod(typeof(NetInfo.Node), nameof(NetInfo.Node.CheckFlags));

        public static void Patch(List<CodeInstruction> codes, MethodBase method, int occurance) {
            CodeInstruction ldNodeID = GetLDArg(method, "nodeID");
            int argLocData = method.GetArgLoc("data");
            CodeInstruction ldaRenderData = new CodeInstruction(OpCodes.Ldarga_S, argLocData);

            int iDrawMesh = codes.Search(_c => _c.Calls(mDrawMesh), count: occurance);
            int iLdLocNodeInfo = codes.Search(
                _c => _c.IsLdLoc(typeof(NetInfo.Node)),
                startIndex: iDrawMesh, count: -1);
            CodeInstruction ldNodeInfo = codes[iLdLocNodeInfo].Clone();

             CodeInstruction[] insertion = new[]{
                ldNodeID,
                ldaRenderData,
                ldNodeInfo,
                new CodeInstruction(OpCodes.Call, mOnAfterDrawMesh)
            };
            codes.InsertInstructions(iDrawMesh + 1, insertion, moveLabels: false);
        }

        static MethodInfo mOnAfterDrawMesh =
            GetMethod(typeof(NodeOverlay), nameof(OnAfterDrawMesh));
        public static void OnAfterDrawMesh(
            ushort nodeID,
            ref RenderManager.Instance renderData,
            NetInfo.Node nodeInfo) {
            int segmentIndex = renderData.m_dataInt0 & 7;
            int segmentIndex2 = renderData.m_dataInt0 >> 4;
            ushort segmentID = nodeID.ToNode().GetSegment(segmentIndex);
            ushort segmentID2 = nodeID.ToNode().GetSegment(segmentIndex2);

            if (nodeInfo == Overlay.HoveredInfo) {
                var data = new Overlay.NodeData {
                    NodeID = nodeID,
                    SegmentID = segmentID,
                    SegmentID2 = segmentID2,
                };
                Overlay.NodeQueue.Enqueue(data);
            }
        }

        public static void PatchBend(List<CodeInstruction> codes, MethodBase method, int occurance) {
            CodeInstruction ldNodeID = GetLDArg(method, "nodeID");
            CodeInstruction loadRenderData = GetLDArg(method, "data");
            MethodInfo mCheckFlags = GetMethod(typeof(NetInfo.Segment), nameof(NetInfo.Segment.CheckFlags));

            int iDrawMesh = codes.Search(_c => _c.Calls(mDrawMesh), count: occurance);
            int iLdLocSegmentInfo = codes.Search(
                _c => _c.IsLdLoc(typeof(NetInfo.Segment)),
                startIndex: iDrawMesh, count: -1);
            CodeInstruction ldNSegmentInfo = codes[iLdLocSegmentInfo].Clone();

            int iCheckFlags = codes.Search(_c => _c.Calls(mCheckFlags));
            int iLdaTurnAround = codes.Search(_c =>
                _c.IsLdLocA(typeof(bool), out _),
                startIndex: iCheckFlags, count: -1);
            var locTurnAround = codes[iLdaTurnAround].operand;
            CodeInstruction ldTurnAround = new CodeInstruction(OpCodes.Ldloc_S, locTurnAround);

            CodeInstruction[] insertion = new[]{
                ldNodeID,
                loadRenderData,
                ldNSegmentInfo,
                new CodeInstruction(OpCodes.Call, mOnAfterDrawMesh)
            };
            codes.InsertInstructions(iDrawMesh + 1, insertion, moveLabels: false);
        }

        static MethodInfo mOnAfterDrawMeshBend =
            GetMethod(typeof(NodeOverlay), nameof(OnAfterDrawMeshBend));
        public static void OnAfterDrawMeshBend(
            ushort nodeID,
            ref RenderManager.Instance renderData,
            NetInfo.Segment segmentInfo,
            ref bool turnAround) {
            int segmentIndex = renderData.m_dataInt0 & 7;
            int segmentIndex2 = renderData.m_dataInt0 >> 4;
            ushort segmentID = nodeID.ToNode().GetSegment(segmentIndex);
            ushort segmentID2 = nodeID.ToNode().GetSegment(segmentIndex2);

            if (segmentInfo == Overlay.HoveredInfo) {
                var data = new Overlay.NodeData {
                    NodeID = nodeID,
                    SegmentID = segmentID,
                    SegmentID2 = segmentID2,
                    IsBendNode = true,
                    TurnAround = turnAround,
                };
                Overlay.NodeQueue.Enqueue(data);
            }
        }
    }
}
