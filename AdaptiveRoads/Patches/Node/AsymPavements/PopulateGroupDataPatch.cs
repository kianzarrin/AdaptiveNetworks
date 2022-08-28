namespace AdaptiveRoads.Patches.AsymPavements {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons.Patches;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;
    using KianCommons;
    using System.Linq;
    /*
    for (int m = 0; m < 8; m++) {
        ushort segmentID = this.GetSegment(m);
        ...
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

    // non-dc node lod
    [UsedImplicitly]
    [InGamePatch]
    [HarmonyPatch2(typeof(NetNode), typeof(PopulateGroupData), instance: true)]
    static class PopulateGroupDataPatch {
        delegate void PopulateGroupData(NetNode instance, ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            var codes = instructions.ToList();
            MethodInfo mCheckFlags = typeof(NetInfo.Node).GetMethod(nameof(NetInfo.Node.CheckFlags), throwOnError: true);
            var iCheckFlags = codes.Search(c => c.Calls(mCheckFlags), count: 3); // check flags for junction.

            // ushort segmentID = this.GetSegment(m);
            MethodInfo mGetSegment = typeof(NetNode).GetMethod(nameof(NetNode.GetSegment), throwOnError: true);
            var iCallGetSegment = codes.Search(c => c.Calls(mGetSegment), startIndex: iCheckFlags, count: -2);
            var iStoreSegmentID = codes.Search(c => c.IsStLoc(typeof(ushort), original), startIndex: iCallGetSegment);
            var ldSegmentID = codes[iStoreSegmentID].BuildLdLocFromStLoc();

            // ushort segmentID2 = This.GetSegment(i);
            var iCallGetSegment2 = codes.Search(c => c.Calls(mGetSegment), startIndex: iCallGetSegment);
            var iStoreSegmentID2 = codes.Search(c => c.IsStLoc(typeof(ushort), original), startIndex: iCallGetSegment2);
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

        }
    }
}