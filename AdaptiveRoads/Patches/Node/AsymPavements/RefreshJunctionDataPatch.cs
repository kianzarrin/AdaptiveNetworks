namespace AdaptiveRoads.Patches.AsymPavements {
    using ColossalFramework.Math;
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;
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

    // non-dc node
    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch2(typeof(NetNode), typeof(RefreshJunctionData), instance:true)]
    static class RefreshJunctionDataPatch {
        delegate void RefreshJunctionData(NetNode instance, ushort nodeID, int segmentIndex, ushort nodeSegment, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
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