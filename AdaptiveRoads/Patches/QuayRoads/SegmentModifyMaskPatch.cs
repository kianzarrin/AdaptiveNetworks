#if QUAY_ROADS_SHOW
using ColossalFramework;
using HarmonyLib;
using UnityEngine;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Data;
namespace AdaptiveRoads.Patches
{

    [HarmonyPatch]
    [InGamePatch]

static class SegmentModifyMaskPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetAI), "SegmentModifyMask")]
        static bool SegmentModifyMaskPrefix(ushort segmentID, ref NetSegment data, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float left, ref float right, ref float leftStartY, ref float rightStartY, ref float leftEndY, ref float rightEndY, ref bool __result, ref RoadAI __instance)
        {
            var net = __instance.m_info.GetMetaData();
            if (net is null) return true;
            if (!net.UseOneSidedTerrainModification) return true;

            ProfileSection[] profile = Profiles.OneSidedRoadProfile; //TODO: different profiles by mesh type
            //Debug.Log(segmentID);
            bool invert = (data.m_flags & NetSegment.Flags.Invert) != 0;
            ushort startNodeId = data.m_startNode;
            ushort endNodeId = data.m_endNode;
            float halfWidth = __instance.m_info.m_halfWidth; //TODO: respect bridge etc.
            NetManager netManager = Singleton<NetManager>.instance;
            Vector3 startPos = netManager.m_nodes.m_buffer[startNodeId].m_position;
            Vector3 endPos = netManager.m_nodes.m_buffer[endNodeId].m_position;
            Vector3 startLeftPos;
            Vector3 startRightPos;
            Vector3 endLeftPos;
            Vector3 endRightPos;
            data.CalculateCorner(segmentID, heightOffset: false, start: true, leftSide: true, out startLeftPos, out Vector3 dir1, out bool smooth1);
            data.CalculateCorner(segmentID, heightOffset: false, start: true, leftSide: false, out startRightPos, out Vector3 dir2, out bool smooth2);
            data.CalculateCorner(segmentID, heightOffset: false, start: false, leftSide: true, out endLeftPos, out Vector3 dir3, out bool smooth3);
            data.CalculateCorner(segmentID, heightOffset: false, start: false, leftSide: false, out endRightPos, out Vector3 dir4, out bool smooth4);
            return ModifyMask(profile, startPos, endPos, startLeftPos, startRightPos, endLeftPos, endRightPos, halfWidth, invert, index, ref surface, ref heights, ref edges, ref left, ref right, ref leftStartY, ref rightStartY, ref leftEndY, ref rightEndY, ref __result);

        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetAI), "NodeModifyMask")]
        static bool NodeModifyMaskPrefix(ushort nodeID, ref NetNode data, ushort segment1, ushort segment2, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float left, ref float right, ref float leftY, ref float rightY, ref bool __result, ref RoadAI __instance)
        {
            return true;
            if ((data.m_flags & NetNode.Flags.Bend) == 0)
            {
                //return true; //this is not a bend node, no patch needed
            }
            __result = false;
            return false;
        }*/



        static bool ModifyMask(ProfileSection[] profile, Vector3 startPos, Vector3 endPos, Vector3 startLeftPos, Vector3 startRightPos, Vector3 endLeftPos, Vector3 endRightPos, float halfWidth, bool invert, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float leftT, ref float rightT, ref float leftStartY, ref float rightStartY, ref float leftEndY, ref float rightEndY, ref bool __result)
        {
            if (index >= profile.Length)
            {
                __result = false;
                return false;
            }

            TerrainManager terrainManager = Singleton<TerrainManager>.instance;
            ProfileSection section = profile[index];
            if (invert) section = section.Inverse();

            if (section.Heights.HasValue)
            {
                heights = section.Heights.Value;
            }
            if (section.Surface.HasValue)
            {
                surface = section.Surface.Value;
            }
            if (section.EdgeFlags.HasValue)
            {
                edges = section.EdgeFlags.Value;

            }
            leftT = section.PosRel[0] + section.PosAbs[0] / (2f * halfWidth);
            rightT = section.PosRel[1] + section.PosAbs[1] / (2f * halfWidth);
            leftStartY = section.HeightOffset[0];
            leftEndY = section.HeightOffset[1];
            rightStartY = section.HeightOffset[2];
            rightEndY = section.HeightOffset[3];
            if (section.HeightTerrain[0] != 0)
            {
                Vector3 sectionStartLeftPos = Vector3.Lerp(startLeftPos, startRightPos, leftT);
                leftStartY += section.HeightTerrain[0] * (terrainManager.SampleOriginalRawHeightSmooth(sectionStartLeftPos) - startPos.y);
            }
            if (section.HeightTerrain[1] != 0)
            {
                Vector3 sectionEndLeftPos = Vector3.Lerp(endLeftPos, endRightPos, leftT);
                leftEndY += section.HeightTerrain[0] * (terrainManager.SampleOriginalRawHeightSmooth(sectionEndLeftPos) - endPos.y);
            }
            if (section.HeightTerrain[2] != 0)
            {
                Vector3 sectionStartRightPos = Vector3.Lerp(startLeftPos, startRightPos, rightT);
                rightStartY += section.HeightTerrain[0] * (terrainManager.SampleOriginalRawHeightSmooth(sectionStartRightPos) - startPos.y);
            }
            if (section.HeightTerrain[3] != 0)
            {
                Vector3 sectionEndRightPos = Vector3.Lerp(endLeftPos, endRightPos, rightT);
                rightEndY += section.HeightTerrain[0] * (terrainManager.SampleOriginalRawHeightSmooth(sectionEndRightPos) - endPos.y);
            }

            __result = true;
            return false;
        }
        private static void Swap<T>(ref T A, ref T B)
        {
            T temp = A;
            A = B;
            B = temp;
        }
        /*
        //maybe find a way to call SampleSlopeHeightOffset on QuaiAI instead of reimplementing?
        //in QuaiAI this is only used for the height at 100% = center+halfWidth, but the sample is taken at 100%+16u and rescaled by (halfWidth-8u)/(halfWidth+8u)
        //behaviour here differs from QuaiAI: position is configurable; invert flag is assumend to already be taken care of.
        static float getTerrainHeightOffset(ushort nodeID, float positionRight)
        {
            NetManager netManager = Singleton<NetManager>.instance;

            //replicationg vanilla behaviour: use first and last segment connected to node as direction reference (why? idk)
            ushort segment1ID = 0;
            ushort segment2ID = 0;
            for (int i = 0; i < 8; i++)
            {
                ushort candidateID = netManager.m_nodes.m_buffer[nodeID].GetSegment(i); //for some reason, vanilla code seems to avoid creating a local variable for the NetNode. Maybe because NetNode is a struct and not a class? Would that mean that it would be copied otherwise? Or is it maybe just the result of inlining?
                if (candidateID != 0)
                {
                    if (segment1ID == 0)
                    {
                        segment1ID = candidateID;
                    }
                    else
                    {
                        segment2ID = candidateID;
                    }
                }
            }
            return getTerrainHeightOffset(nodeID, segment1ID, segment2ID, positionRight);
        }
        static float getTerrainHeightOffset(ushort nodeID, ushort segment1ID, ushort segment2ID, float positionRight)
        {
            NetManager netManager = Singleton<NetManager>.instance;
            TerrainManager terrainManager = Singleton<TerrainManager>.instance;
            bool segment1Start = netManager.m_segments.m_buffer[segment1ID].m_startNode == nodeID;
            Vector3 segment1Right;
            Vector3 right;
            if (segment1Start)
            {
                Vector3 segment1Forward = netManager.m_segments.m_buffer[segment1ID].m_startDirection;
                segment1Right = new Vector3(segment1Forward.z, 0f, 0f - segment1Forward.x);
            }
            else
            {
                Vector3 segment1Backward = netManager.m_segments.m_buffer[segment1ID].m_endDirection;
                segment1Right = new Vector3(0f - segment1Backward.z, 0f, segment1Backward.x);
            }

            if (segment2ID != 0)
            {
                bool segment2Start = netManager.m_segments.m_buffer[segment2ID].m_startNode == nodeID;
                Vector3 segment2Right;
                if (segment2Start) //TODO: the flip conditions in vanilla seem different, but I don't understand them (yet)
                {
                    Vector3 segment2Forward = netManager.m_segments.m_buffer[segment2ID].m_startDirection;
                    segment2Right = new Vector3(segment2Forward.z, 0f, 0f - segment2Forward.x);
                }
                else
                {
                    Vector3 segment2Backward = netManager.m_segments.m_buffer[segment2ID].m_endDirection;
                    segment2Right = new Vector3(0f - segment2Backward.z, 0f, segment2Backward.x);
                }

                right = (segment1Right + segment2Right).normalized;
            }
            else
            {
                right = segment1Right.normalized;
            }

            Vector3 nodePosition = netManager.m_nodes.m_buffer[nodeID].m_position;
            Vector3 samplePosition = nodePosition + right * positionRight;
            float heightDifference = terrainManager.SampleOriginalRawHeightSmooth(samplePosition) - nodePosition.y;
            return heightDifference; //QuayAI does some rescaling here; Don't know how to generalize that, and about it's purpose.
        }
        */
    }
}
#endif