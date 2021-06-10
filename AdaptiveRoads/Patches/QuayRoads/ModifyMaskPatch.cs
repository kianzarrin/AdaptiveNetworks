#if QUAY_ROADS_SHOW
using ColossalFramework;
using HarmonyLib;
using UnityEngine;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Data;
using KianCommons;
using System.Reflection;
using System;

namespace AdaptiveRoads.Patches
{

    [HarmonyPatch]
    [InGamePatch]

static class ModifyMaskPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetAI), "SegmentModifyMask")]
        static bool SegmentModifyMaskPrefix(ushort segmentID, ref NetSegment data, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float left, ref float right, ref float leftStartY, ref float rightStartY, ref float leftEndY, ref float rightEndY, ref bool __result, ref RoadAI __instance)
        {
            var net = __instance.m_info.GetMetaData();
            if (net is null) return true;
            if (!net.UseOneSidedTerrainModification) return true;

            ProfileSection[] profile = Profiles.HighRightOneSidedRoadProfile; //TODO: different profiles by mesh type
            Log.Debug("modifying mask for segment " + segmentID.ToString() + ", section " + index);
            bool invert = (data.m_flags & NetSegment.Flags.Invert) != 0;
            float halfWidth = __instance.m_info.m_halfWidth; //TODO: respect bridge etc.
            return ModifyMask(profile, halfWidth, invert, index, ref surface, ref heights, ref edges, ref left, ref right, ref leftStartY, ref rightStartY, ref leftEndY, ref rightEndY, ref __result);

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetAI), "NodeModifyMask")]
        static bool NodeModifyMaskPrefix(ushort nodeID, ref NetNode data, ushort segment1, ushort segment2, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float left, ref float right, ref float leftY, ref float rightY, ref bool __result, ref RoadAI __instance)
        {
            var net = __instance.m_info.GetMetaData();
            if (net is null) return true;
            if (!net.UseOneSidedTerrainModification) return true;

            ProfileSection[] profile = Profiles.HighRightOneSidedRoadProfile; //TODO: different profiles by mesh type
            Log.Debug("modifying mask for node " + nodeID.ToString() + ", section " + index);
            NetManager netManager = Singleton<NetManager>.instance;
            bool isStartNode = netManager.m_segments.m_buffer[(int)segment1].m_startNode == nodeID;
            bool segmentInvert = (netManager.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
            bool invert = isStartNode ^ segmentInvert;
            float halfWidth = __instance.m_info.m_halfWidth; //TODO: respect bridge etc.
            float leftStartY = leftY;
            float rightStartY = rightY;
            float leftEndY = leftY;
            float rightEndY = rightY;
            bool result  = ModifyMask(profile, halfWidth, invert, index, ref surface, ref heights, ref edges, ref left, ref right, ref leftStartY, ref rightStartY, ref leftEndY, ref rightEndY, ref __result);
            if (isStartNode) {
                leftY = leftStartY;
                rightY = rightStartY;
            } else {
                leftY = leftEndY;
                rightY = rightEndY;
            }
            return result;
        }



        static bool ModifyMask(ProfileSection[] profile, float halfWidth, bool invert, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float leftT, ref float rightT, ref float leftStartY, ref float rightStartY, ref float leftEndY, ref float rightEndY, ref bool __result)
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

            __result = true;
            return false;
        }
        private static void Swap<T>(ref T A, ref T B)
        {
            T temp = A;
            A = B;
            B = temp;
        }
    }
}
#endif