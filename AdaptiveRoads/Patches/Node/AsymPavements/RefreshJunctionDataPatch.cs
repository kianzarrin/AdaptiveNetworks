namespace AdaptiveRoads.Patches.AsymPavements {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;

    // non-dc node
    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch2(typeof(NetNode), typeof(RefreshJunctionData), instance: true)]
    static class RefreshJunctionDataPatch {
        delegate void RefreshJunctionData(NetNode instance, ushort nodeID, int segmentIndex, ushort nodeSegment, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data);

        static bool Fix(ushort segmentId, ushort segmentId2, bool right) {
            ref NetSegment segment = ref segmentId.ToSegment();
            ref NetSegment segment2 = ref segmentId2.ToSegment();
            NetInfo info = segment.Info;
            NetInfo info2 = segment2.Info;

            bool sym = info.HasSymPavements();
            bool sym2 = info2.HasSymPavements();
            if (sym && sym2) {
                return true;
            }

            ushort nodeId = segment.GetSharedNode(segmentId2);
            bool reverse = segment.IsInvert() ^ segment.IsStartNode(nodeId);
            bool reverse2 = segment2.IsInvert() ^ segment2.IsStartNode(nodeId);


            bool b = reverse != right;
            bool wide = info.PW(b) >= info.PW(!b);

            bool b2 = reverse2 == right;
            bool wide2 = info2.PW(b2) >= info2.PW(!b2);

            bool ret = wide && wide2;
            return ret.LogRet($"Fix({segmentId} -> {segmentId2}, right={right}) : wide={wide} wide2={wide2}");
        }

        static bool Prefix(ushort nodeID, int segmentIndex, [HarmonyArgument("nodeSegment")]ushort segmentId, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data) {
            ref NetNode node = ref nodeID.ToNode();
            data.m_position = node.m_position;
            data.m_rotation = Quaternion.identity;
            data.m_initialized = true;
            Vector3 cornerPos_right = Vector3.zero;
            Vector3 cornerPos_left = Vector3.zero;
            Vector3 cornerDir_right = Vector3.zero;
            Vector3 cornerDir_left = Vector3.zero;
            Vector3 cornerPosA_right = Vector3.zero;
            Vector3 cornerPosA_left = Vector3.zero;
            Vector3 cornerDirA_right = Vector3.zero;
            Vector3 cornerDirA_left = Vector3.zero;
            Vector3 cornerPos5 = Vector3.zero;
            Vector3 cornerPos6 = Vector3.zero;
            Vector3 cornerDirection5 = Vector3.zero;
            Vector3 cornerDirection6 = Vector3.zero;
            NetSegment segment = segmentId.ToSegment();
            NetInfo info = segment.Info;
            float vScale = info.m_netAI.GetVScale();
            ItemClass connectionClass = info.GetConnectionClass();
            Vector3 dir = ((nodeID != segment.m_startNode) ? segment.m_endDirection : segment.m_startDirection);
            float dot_A = -4f;
            float dot_B = -4f;
            ushort segmentID_A = 0;
            ushort segmentID_B = 0;
            for (int i = 0; i < 8; i++) {
                ushort segmentID2 = node.GetSegment(i);
                if (segmentID2 == 0 || segmentID2 == segmentId) {
                    continue;
                }
                ref NetSegment segment2 = ref segmentID2.ToSegment();
                NetInfo info2 = segment2.Info;
                ItemClass connectionClass2 = info2.GetConnectionClass();
                if (connectionClass.m_service != connectionClass2.m_service || ((info.m_onlySameConnectionGroup || info2.m_onlySameConnectionGroup) && (info.m_connectGroup & info2.m_connectGroup) == 0)) {
                    continue;
                }
                Vector3 dir2 = ((nodeID != segment2.m_startNode) ? segment2.m_endDirection : segment2.m_startDirection);
                float dot = dir.x * dir2.x + dir.z * dir2.z;
                if (dir2.z * dir.x - dir2.x * dir.z < 0f) {
                    if (dot > dot_A) {
                        dot_A = dot;
                        segmentID_A = segmentID2;
                    }
                    dot = -2f - dot;
                    if (dot > dot_B) {
                        dot_B = dot;
                        segmentID_B = segmentID2;
                    }
                } else {
                    if (dot > dot_B) {
                        dot_B = dot;
                        segmentID_B = segmentID2;
                    }
                    dot = -2f - dot;
                    if (dot > dot_A) {
                        dot_A = dot;
                        segmentID_A = segmentID2;
                    }
                }
            }
            bool bStartNode = segment.m_startNode == nodeID;
            segment.CalculateCorner(segmentId, heightOffset: true, bStartNode, leftSide: false, out cornerPos_right, out cornerDir_right, out var smooth);
            segment.CalculateCorner(segmentId, heightOffset: true, bStartNode, leftSide: true, out cornerPos_left, out cornerDir_left, out smooth);

            // A=Right B=Left
            if (segmentID_A != 0 && segmentID_B != 0) {
                float pavementRatio_avgA = Commons.ModifyPavement(info.m_pavementWidth, segmentId, segmentID_A, 1) / info.m_halfWidth * 0.5f;
                float widthRatioA = 1f;
                float startRatioA = 0.3f;
                float endRatioA = 0.3f;
                if (segmentID_A != 0) {
                    NetSegment segment_A = segmentID_A.ToSegment();
                    NetInfo infoA = segment_A.Info;
                    bStartNode = segment_A.m_startNode == nodeID;
                    segment_A.CalculateCorner(segmentID_A, heightOffset: true, bStartNode, leftSide: true, out cornerPosA_right, out cornerDirA_right, out smooth);
                    segment_A.CalculateCorner(segmentID_A, heightOffset: true, bStartNode, leftSide: false, out cornerPosA_left, out cornerDirA_left, out smooth);
                    float pavementRatioA = Commons.ModifyPavement(infoA.m_pavementWidth, segmentID_A, segmentId, 2) / infoA.m_halfWidth * 0.5f;
                    if (dot_A > -0.5f && info.m_pavementWidth > 0 && Fix(segmentId, segmentID_A, true)) {
                        startRatioA = Mathf.Clamp(startRatioA * (2f * pavementRatioA / (pavementRatio_avgA + pavementRatioA)), 0.05f, 0.7f);
                        endRatioA = Mathf.Clamp(endRatioA * (2f * pavementRatio_avgA / (pavementRatio_avgA + pavementRatioA)), 0.05f, 0.7f);
                    }
                    pavementRatio_avgA = (pavementRatio_avgA + pavementRatioA) * 0.5f;
                    widthRatioA = 2f * info.m_halfWidth / (info.m_halfWidth + infoA.m_halfWidth);
                }
                float pavementRatio_avgB = Commons.ModifyPavement(info.m_pavementWidth, segmentId, segmentID_B, 3) / info.m_halfWidth * 0.5f;
                float widthRatioB = 1f;
                float startRatioB = 0.3f;
                float endRatioB = 0.3f;
                if (segmentID_B != 0) {
                    NetSegment segment_B = segmentID_B.ToSegment();
                    NetInfo infoB = segment_B.Info;
                    bStartNode = segment_B.m_startNode == nodeID;
                    segment_B.CalculateCorner(segmentID_B, heightOffset: true, bStartNode, leftSide: true, out cornerPos5, out cornerDirection5, out smooth);
                    segment_B.CalculateCorner(segmentID_B, heightOffset: true, bStartNode, leftSide: false, out cornerPos6, out cornerDirection6, out smooth);
                    float pavementRatioB = Commons.ModifyPavement(infoB.m_pavementWidth, segmentID_B, segmentId, 4) / infoB.m_halfWidth * 0.5f;
                    if (dot_B > -0.5f && info.m_pavementWidth > 0 && Fix(segmentId, segmentID_B, false)) {
                        startRatioB = Mathf.Clamp(startRatioB * (2f * pavementRatioB / (pavementRatio_avgB + pavementRatioB)), 0.05f, 0.7f);
                        endRatioB = Mathf.Clamp(endRatioB * (2f * pavementRatio_avgB / (pavementRatio_avgB + pavementRatioB)), 0.05f, 0.7f);
                    }
                    pavementRatio_avgB = (pavementRatio_avgB + pavementRatioB) * 0.5f;
                    widthRatioB = 2f * info.m_halfWidth / (info.m_halfWidth + infoB.m_halfWidth);
                }
                NetSegment.CalculateMiddlePoints(cornerPos_right, -cornerDir_right, cornerPosA_right, -cornerDirA_right, smoothStart: true, smoothEnd: true, startRatioA, endRatioA, out var bpointA_right, out var cpointA_right);
                NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPosA_left, -cornerDirA_left, smoothStart: true, smoothEnd: true, startRatioA, endRatioA, out var bpoint_Aleft, out var cpoint_Aleft);
                NetSegment.CalculateMiddlePoints(cornerPos_right, -cornerDir_right, cornerPos5, -cornerDirection5, smoothStart: true, smoothEnd: true, startRatioB, endRatioB, out var bpoint_Bright, out var cpoint_Bright);
                NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPos6, -cornerDirection6, smoothStart: true, smoothEnd: true, startRatioB, endRatioB, out var bpoint_Bleft, out var cpoint_Bleft);
                data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos_right, bpointA_right, cpointA_right, cornerPosA_right, cornerPos_right, bpointA_right, cpointA_right, cornerPosA_right, node.m_position, vScale);
                data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, bpoint_Aleft, cpoint_Aleft, cornerPosA_left, cornerPos_left, bpoint_Aleft, cpoint_Aleft, cornerPosA_left, node.m_position, vScale);
                data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, bpoint_Bright, cpoint_Bright, cornerPos5, cornerPos_right, bpoint_Bright, cpoint_Bright, cornerPos5, node.m_position, vScale);
                data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, bpoint_Bleft, cpoint_Bleft, cornerPos6, cornerPos_left, bpoint_Bleft, cpoint_Bleft, cornerPos6, node.m_position, vScale);
                data.m_dataVector0 = new Vector4(
                    0.5f / info.m_halfWidth,
                    1f / info.m_segmentLength,
                    0.5f - Commons.ModifyPavement(info.m_pavementWidth, segmentId, segmentId, 5) / info.m_halfWidth * 0.5f,
                    Commons.ModifyPavement(info.m_pavementWidth, segmentId, segmentId, 6) / info.m_halfWidth * 0.5f);
                data.m_dataVector1 = centerPos - data.m_position;
                data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
                data.m_dataVector2 = new Vector4(pavementRatio_avgA, widthRatioA, pavementRatio_avgB, widthRatioB);
            } else {
                centerPos.x = (cornerPos_right.x + cornerPos_left.x) * 0.5f;
                centerPos.z = (cornerPos_right.z + cornerPos_left.z) * 0.5f;
                cornerPosA_right = cornerPos_left;
                cornerPosA_left = cornerPos_right;
                cornerDirA_right = cornerDir_left;
                cornerDirA_left = cornerDir_right;
                float endRaduis = info.m_netAI.GetEndRadius() * 1.33333337f;
                Vector3 vector3 = cornerPos_right - cornerDir_right * endRaduis;
                Vector3 vector4 = cornerPosA_right - cornerDirA_right * endRaduis;
                Vector3 vector5 = cornerPos_left - cornerDir_left * endRaduis;
                Vector3 vector6 = cornerPosA_left - cornerDirA_left * endRaduis;
                Vector3 vector7 = cornerPos_right + cornerDir_right * endRaduis;
                Vector3 vector8 = cornerPosA_right + cornerDirA_right * endRaduis;
                Vector3 vector9 = cornerPos_left + cornerDir_left * endRaduis;
                Vector3 vector10 = cornerPosA_left + cornerDirA_left * endRaduis;
                data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos_right, vector3, vector4, cornerPosA_right, cornerPos_right, vector3, vector4, cornerPosA_right, node.m_position, vScale);
                data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, vector9, vector10, cornerPosA_left, cornerPos_left, vector9, vector10, cornerPosA_left, node.m_position, vScale);
                data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, vector7, vector8, cornerPosA_right, cornerPos_right, vector7, vector8, cornerPosA_right, node.m_position, vScale);
                data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, vector5, vector6, cornerPosA_left, cornerPos_left, vector5, vector6, cornerPosA_left, node.m_position, vScale);
                data.m_dataMatrix0.SetRow(3, data.m_dataMatrix0.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_extraData.m_dataMatrix2.SetRow(3, data.m_extraData.m_dataMatrix2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_extraData.m_dataMatrix3.SetRow(3, data.m_extraData.m_dataMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_dataMatrix1.SetRow(3, data.m_dataMatrix1.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
                data.m_dataVector1 = centerPos - data.m_position;
                data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
                data.m_dataVector2 = new Vector4(info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f, info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f);
            }
            Vector4 colorLocation;
            Vector4 vector11;
            if (NetNode.BlendJunction(nodeID)) {
                colorLocation = RenderManager.GetColorLocation((uint)(86016 + nodeID));
                vector11 = colorLocation;
            } else {
                colorLocation = RenderManager.GetColorLocation((uint)(49152 + segmentId));
                vector11 = RenderManager.GetColorLocation((uint)(86016 + nodeID));
            }
            data.m_extraData.m_dataVector4 = new Vector4(colorLocation.x, colorLocation.y, vector11.x, vector11.y);
            data.m_dataInt0 = segmentIndex;
            data.m_dataColor0 = info.m_color;
            data.m_dataColor0.a = 0f;
            data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
            if (info.m_requireSurfaceMaps) {
                Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector3);
            }
            instanceIndex = data.m_nextInstance;

            return false; //repalce
        }

        static IEnumerable<CodeInstruction> Transpiler_NotRun(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            return instructions;
            try {
/*
float dot_A = -4f;
float dot_B = -4f;
ushort segmentID_A = 0;
ushort segmentID_B = 0;
for (int i = 0; i < 8; i++) {
    ushort segmentID2 = This.GetSegment(i);
    if (segmentID2 != 0 && segmentID2 != SegmentID) {
        NetInfo info2 = instance.m_segments.m_buffer[segmentID2].Info;
        ItemClass connectionClass2 = info2.GetConnectionClass();
        if (connectionClass.m_service == connectionClass2.m_service) {
            NetSegment segment2 = segmentID2.ToSegment();
            bool bStartNode2 = nodeID != segment2.m_startNode;
            Vector3 dir2 = segment2.GetDirection(nodeID);
            float dot = dir.x * dir2.x + dir.z * dir2.z;
            float determinent = dir2.z * dir.x - dir2.x * dir.z;
            bool bRight = determinent > 0;
            bool bWide = dot < 0;
            // 180 -> det=0 dot=-1
            if (!bRight) {
                if (dot > dot_A) // most acute
                {
                    dot_A = dot;
                    segmentID_A = segmentID2;
                }
                dot = -2f - dot;
                if (dot > dot_B) // widest
                {
                    dot_B = dot;
                    segmentID_B = segmentID2;
                }
            } else {
                if (dot > dot_B) // most acute
                {
                    dot_B = dot;
                    segmentID_B = segmentID2;
                }
                dot = -2f - dot;
                if (dot > dot_A) // widest
                {
                    dot_A = dot;
                    segmentID_A = segmentID2;
                }
            }
        }
    }
}
*/

                var ldSegmentID = GetLDArg(original, "nodeSegment");
                var codes = instructions.ToCodeList();

                // ushort segmentID2 = This.GetSegment(i);
                MethodInfo mGetSegment = typeof(NetNode).GetMethod(nameof(NetNode.GetSegment), throwOnError: true);
                int iCallGetSegment = codes.Search(code => code.Calls(mGetSegment));
                int iStoreSegmentID2 = codes.Search(code => code.IsStLoc(typeof(ushort), original), startIndex: iCallGetSegment);
                int locSegmentID2 = codes[iStoreSegmentID2].GetLoc();

                // segmentID_A = segmentID2;
                int iLoadSegmentID2 = codes.Search(code => code.IsLdLoc(locSegmentID2), startIndex: iStoreSegmentID2);
                int iStoreSegmentID_A = codes.Search(code => code.IsStLoc(typeof(ushort), original), startIndex: iLoadSegmentID2);
                var ldSegmentIDA = codes[iStoreSegmentID_A].BuildLdLocFromStLoc();

                // segmentID_B = segmentID2;
                int iLoadSegmentID2_second = codes.Search(code => code.IsLdLoc(locSegmentID2), startIndex: iStoreSegmentID_A);
                int iStoreSegmentID_B = codes.Search(code => code.IsStLoc(typeof(ushort), original), startIndex: iLoadSegmentID2_second);
                var ldSegmentIDB = codes[iStoreSegmentID_B].BuildLdLocFromStLoc();

                return Commons.ApplyPatch(codes, ldSegmentID, ldSegmentIDA, ldSegmentIDB);
            } catch (Exception ex) {
                ex.Log();
                return instructions;
            }
        }
    }
}