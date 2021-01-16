using AdaptiveRoads.UI.RoadEditor;
using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static KianCommons.Patches.TranspilerUtils;
using ColossalFramework;

namespace AdaptiveRoads.Patches.Node {
    public static class NodeOverlay {
        delegate void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties);
        static MethodInfo mDrawMesh =
            DeclaredMethod<DrawMesh>(typeof(Graphics));

        static MethodInfo mCheckFlags =
            GetMethod(typeof(NetInfo.Node), nameof(NetInfo.Node.CheckFlags));

        public static void Patch(List<CodeInstruction> codes, MethodBase method, int occuranceDrawMesh, int counterGetSegment) {
            CodeInstruction ldNodeID = GetLDArg(method, "nodeID");
            int argLocData = method.GetArgLoc("data");
            CodeInstruction loadRenderData = new CodeInstruction(OpCodes.Ldarg_S, argLocData);

            int iDrawMesh = codes.Search(_c => _c.Calls(mDrawMesh), count: occuranceDrawMesh);
            int iLdLocNodeInfo = codes.Search(
                _c => _c.IsLdLoc(typeof(NetInfo.Node)),
                startIndex: iDrawMesh, count: -1);
            CodeInstruction ldNodeInfo = codes[iLdLocNodeInfo].Clone();

            CodeInstruction[] insertion = new[]{
                ldNodeID,
                loadRenderData,
                ldNodeInfo,
                new CodeInstruction(OpCodes.Call, mOnAfterDrawMesh)
            };
            codes.InsertInstructions(iDrawMesh + 1, insertion, moveLabels: false);
        }

        static MethodInfo mOnAfterDrawMesh = GetMethod(typeof(NodeOverlay), nameof(OnAfterDrawMesh));

        public static void OnAfterDrawMesh(
            ushort nodeID,
            ref RenderManager.Instance renderData,
            NetInfo.Node nodeInfo) {
            if (nodeInfo == Overlay.HoveredInfo)
                Enqueue(nodeID, ref renderData);
            
        }

        static void Enqueue(
            ushort nodeID, ref RenderManager.Instance renderData,
            bool isBendNode = false, bool turnAround = false) {
            int segmentIndex = renderData.m_dataInt0 & 7;
            int segmentIndex2 = renderData.m_dataInt0 >> 4;
            ref var node = ref nodeID.ToNode();
            ushort segmentID = node.GetSegment(segmentIndex);
            ushort segmentID2 = node.GetSegment(segmentIndex2);
            bool isDC = (renderData.m_dataInt0 & 8) != 0; // normal DC
            isDC |= node.m_flags.IsFlagSet(NetNode.Flags.Bend); // bend DC

            var data = new Overlay.NodeData {
                NodeID = nodeID,
                SegmentID = segmentID,
                SegmentID2 = segmentID2,
                IsDC = isDC,
                IsBendNode = isBendNode,
                TurnAround = turnAround,
            };
            Overlay.NodeQueue.Enqueue(data);
        }

        public static void PatchBend(List<CodeInstruction> codes, MethodBase method, int occuranceDrawMesh) {
            CodeInstruction ldNodeID = GetLDArg(method, "nodeID");
            CodeInstruction loadRenderData = GetLDArg(method, "data");
            MethodInfo mCheckFlags = GetMethod(typeof(NetInfo.Segment), nameof(NetInfo.Segment.CheckFlags));

            int iDrawMesh = codes.Search(_c => _c.Calls(mDrawMesh), count: occuranceDrawMesh);
            int iLdLocSegmentInfo = codes.Search(
                _c => _c.IsLdLoc(typeof(NetInfo.Segment)),
                startIndex: iDrawMesh, count: -1);
            CodeInstruction ldSegmentInfo = codes[iLdLocSegmentInfo].Clone();

            int iCheckFlags = codes.Search(_c => _c.Calls(mCheckFlags));
            int iLdaTurnAround = codes.Search(_c =>
                _c.IsLdLocA(typeof(bool), out _),
                startIndex: iCheckFlags, count: -1);
            CodeInstruction loadRefTurnAround = codes[iLdaTurnAround].Clone();

            CodeInstruction[] insertion = new[]{
                ldNodeID,
                loadRenderData,
                ldSegmentInfo,
                loadRefTurnAround,
                new CodeInstruction(OpCodes.Call, mOnAfterDrawMeshBend)
            };
            codes.InsertInstructions(iDrawMesh + 1, insertion, moveLabels: false);
        }

        static MethodInfo mOnAfterDrawMeshBend = GetMethod(typeof(NodeOverlay), nameof(OnAfterDrawMeshBend));
        public static void OnAfterDrawMeshBend(
            ushort nodeID,
            ref RenderManager.Instance renderData,
            NetInfo.Segment segmentInfo,
            ref bool turnAround) {
            if (segmentInfo == Overlay.HoveredInfo) {
                Enqueue(nodeID, ref renderData, isBendNode:true, turnAround: turnAround);
            }
        }
    }
}
