namespace AdaptiveRoads.Patches.RenderShift {
    using AdaptiveRoads.Manager;
    using AdaptiveRoads.Util;
    using HarmonyLib;
    using KianCommons.Patches;
    using KianCommons;
    using UnityEngine;
    using System.Reflection;
    using System;

    [HarmonyPatch]
    public static class NetToolPatch0 {
        delegate ToolBase.ToolErrors CreateNode(NetInfo info, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint, FastList<NetTool.NodePosition> nodeBuffer, int maxSegments, bool test, bool testEnds, bool visualize, bool autoFix, bool needMoney, bool invert, bool switchDir, ushort relocateBuildingID, out ushort firstNode, out ushort lastNode, out ushort segment, out int cost, out int productionRate);
        static MethodBase TargetMethod() => TranspilerUtils.DeclaredMethod<CreateNode>(typeof(NetTool));

        static void Prefix(FastList<NetTool.NodePosition> nodeBuffer) {
            nodeBuffer_ = nodeBuffer;
            NetToolPatch.CreateNodePrefix();
        }

        static FastList<NetTool.NodePosition> nodeBuffer_;
        public static NetInfo Info {
            get {
                if(nodeBuffer_ != null && nodeBuffer_.m_size > 0) {
                    return nodeBuffer_[0].m_nodeInfo;
                } else {
                    return ToolsModifierControl.GetTool<NetTool>().Prefab;
                }
            }
        }
    }

    [HarmonyPatch]
    public static class NetToolPatch {
        static int nCalledRenderNode_ = 0;
        static NetInfo Info => NetToolPatch0.Info;
        public static void CreateNodePrefix() {
            nCalledRenderNode_ = 0;
        }

        [HarmonyPatch(typeof(NetTool), "RenderNode")]
        [HarmonyPrefix]
        public static void RenderNodePrefix(NetInfo info, ref Vector3 position, Vector3 direction) {
            nCalledRenderNode_++;
            float shift = info?.GetMetaData()?.Shift ?? 0;
            if (shift != 0) {
                if (nCalledRenderNode_ == 2) {
                    // second call => the direction is backward.
                    direction = -direction; // flip it forward
                }
                Shift(ref position, direction, shift);
            }
        }


        [HarmonyPatch(typeof(NetTool), "RenderSegment")]
        [HarmonyPrefix]
        static void RenderSegmentPrefix(
        NetInfo info,
        ref Vector3 startPosition, ref Vector3 endPosition,
        Vector3 startDirection, Vector3 endDirection) {
            float shift = info?.GetMetaData()?.Shift ?? 0;
            if (shift != 0) {
                Shift(ref startPosition, startDirection, shift);
                Shift(ref endPosition, endDirection, shift);
            }
        }

        [HarmonyPatch(typeof(NetTool), "RenderNodeBuilding")]
        [HarmonyPrefix]
        static void RenderNodeBuildingPrefix(ref Vector3 position, Vector3 direction) {
            float shift = Info?.GetMetaData()?.Shift ?? 0;
            if (shift != 0) {
                Shift(ref position, direction, shift);
            }
        }


        [HarmonyPatch(typeof(NetTool), "TestNodeBuilding")]
        [HarmonyPrefix]
        static void TestNodeBuildingPrefix(ref Vector3 position, Vector3 direction) => RenderNodeBuildingPrefix(ref position, direction);

        static void Shift(ref Vector3 pos, Vector3 dir, float shift) {
            var righward = CornerUtil.CalculateRighwardNormal(dir);
            pos += shift * righward;
        }

        #region fix overlay
        public static void RenderOverlayPrefix(NetTool __instance, NetInfo info,
            ref NetTool.ControlPoint startPoint, ref NetTool.ControlPoint middlePoint, ref NetTool.ControlPoint endPoint) {
            try {
                float shift = info?.GetMetaData()?.Shift ?? 0;
                if (shift != 0) {
                    Shift(ref startPoint.m_position, middlePoint.m_direction, shift);
                    Shift(ref middlePoint.m_position, middlePoint.m_direction, shift);
                    Shift(ref endPoint.m_position, endPoint.m_direction, shift);
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }
        #endregion
    }

    [HarmonyPatch2(typeof(NetTool), typeof(RenderOverlay))]
    public static class NetToolRenderOverlayPatch {
        private delegate void RenderOverlay(RenderManager.CameraInfo cameraInfo, NetInfo info, Color color, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint);
        static void Prefix(NetTool __instance, NetInfo info,
            ref NetTool.ControlPoint startPoint, ref NetTool.ControlPoint middlePoint, ref NetTool.ControlPoint endPoint) {
            NetToolPatch.RenderOverlayPrefix(__instance, info, ref startPoint, ref middlePoint, ref endPoint);
        }
    }
}
