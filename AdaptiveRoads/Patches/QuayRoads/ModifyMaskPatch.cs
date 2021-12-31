using ColossalFramework;
using HarmonyLib;
using UnityEngine;
using AdaptiveRoads.Manager;
using KianCommons;
using System.Reflection;
using System;
using AdaptiveRoads.Data.QuayRoads;
using System.Collections.Generic;
using static KianCommons.ReflectionHelpers;
using JetBrains.Annotations;

namespace AdaptiveRoads.Patches {

    [HarmonyPatch]
    [PreloadPatch]
    static class SegmentModifyMaskPatch {
        static bool Prepare() => ModifyMaskCommon.Prepare();

        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(NetAI), "SegmentModifyMask");
            yield return GetMethod(typeof(PedestrianWayAI), "SegmentModifyMask");
        }
        internal static bool Prefix(ushort segmentID, ref NetSegment data, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float left, ref float right, ref float leftStartY, ref float rightStartY, ref float leftEndY, ref float rightEndY, ref bool __result, NetAI __instance) {
            var net = __instance.m_info.GetMetaData();
            if (net is null) {
#if DEBUGQUAYROADS
                Log.Debug("No AN data found for: \n segmentId: " + segmentID + "name: " + data.Info.name);
#endif
                return true;
            }

            ProfileSection[] profile = net.QuayRoadsProfile;
            if (profile is null) return true;
            Log.Debug("modifying mask for segment " + segmentID.ToString() + ", section " + index);
            bool invert = (data.m_flags & NetSegment.Flags.Invert) != 0;
            return ModifyMaskCommon.ModifyMask(profile, invert, index, ref surface, ref heights, ref edges, ref left, ref right, ref leftStartY, ref rightStartY, ref leftEndY, ref rightEndY, ref __result);
        }
    }

    [HarmonyPatch]
    [PreloadPatch]
    static class SegmentModifyMaskPatchT {
        static bool Prepare() => ModifyMaskCommon.Prepare();
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(CanalAI), "SegmentModifyMask");
            yield return GetMethod(typeof(FloodWallAI), "SegmentModifyMask");
            yield return GetMethod(typeof(QuayAI), "SegmentModifyMask");
        }
        //some overrides use rightT and leftT as argument name instead of right and left
        static bool Prefix(ushort segmentID, ref NetSegment data, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float leftT, ref float rightT, ref float leftStartY, ref float rightStartY, ref float leftEndY, ref float rightEndY, ref bool __result, CanalAI __instance) {
            return SegmentModifyMaskPatch.Prefix(segmentID, ref data, index, ref surface, ref heights, ref edges, ref leftT, ref rightT, ref leftStartY, ref rightStartY, ref leftEndY, ref rightEndY, ref __result, __instance);
        }
    }


    [HarmonyPatch]
    [PreloadPatch]
    static class NodeModifyMaskPatch {
        static bool Prepare() => ModifyMaskCommon.Prepare();

        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(NetAI), "NodeModifyMask");
            yield return GetMethod(typeof(PedestrianPathAI), "NodeModifyMask");
            yield return GetMethod(typeof(PedestrianWayAI), "NodeModifyMask");
        }
        internal static bool Prefix(ushort nodeID, ref NetNode data, ushort segment1, ushort segment2, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float left, ref float right, ref float leftY, ref float rightY, ref bool __result, NetAI __instance) {
            var net = __instance.m_info.GetMetaData();
            if (net is null) return true;

            ProfileSection[] profile = net.QuayRoadsProfile;
            if (profile is null) return true;

            Log.Debug("modifying mask for node " + nodeID.ToString() + ", section " + index);
            NetManager netManager = Singleton<NetManager>.instance;
            bool isStartNode = netManager.m_segments.m_buffer[segment1].m_startNode == nodeID;
            bool segmentInvert = (netManager.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
            bool invert = isStartNode ^ segmentInvert;
            float leftStartY = leftY;
            float rightStartY = rightY;
            float leftEndY = leftY;
            float rightEndY = rightY;
            bool result = ModifyMaskCommon.ModifyMask(profile, invert, index, ref surface, ref heights, ref edges, ref left, ref right, ref leftStartY, ref rightStartY, ref leftEndY, ref rightEndY, ref __result);
            if (isStartNode) {
                leftY = leftStartY;
                rightY = rightStartY;
            } else {
                leftY = leftEndY;
                rightY = rightEndY;
            }
            return result;
        }
    }

    [HarmonyPatch]
    [PreloadPatch]
    static class NodeModifyMaskPatchT {
        static bool Prepare() => ModifyMaskCommon.Prepare();

        //some overrides use rightT and leftT as argument name instead of right and left
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(CableCarPathAI), "NodeModifyMask");
            yield return GetMethod(typeof(CanalAI), "NodeModifyMask");
            yield return GetMethod(typeof(FloodWallAI), "NodeModifyMask");
            yield return GetMethod(typeof(PowerLineAI), "NodeModifyMask");
            yield return GetMethod(typeof(QuayAI), "NodeModifyMask");
            yield return GetMethod(typeof(RoadBaseAI), "NodeModifyMask");
            yield return GetMethod(typeof(SupportCableAI), "NodeModifyMask");
        }
        static bool Prefix(ushort nodeID, ref NetNode data, ushort segment1, ushort segment2, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float leftT, ref float rightT, ref float leftY, ref float rightY, ref bool __result, CableCarPathAI __instance) {
            return NodeModifyMaskPatch.Prefix(nodeID, ref data, segment1, segment2, index, ref surface, ref heights, ref edges, ref leftT, ref rightT, ref leftY, ref rightY, ref __result, __instance);
        }
    }

    static class ModifyMaskCommon {
        // linux and (and possibly Mac) crash with this patch:
        // new harmony fixes this problem
        static bool IsAffectedOS => Application.platform is RuntimePlatform.LinuxPlayer or RuntimePlatform.OSXPlayer;
        internal static bool Prepare() => !IsAffectedOS || typeof(Harmony).VersionOf() >= new Version("2.1.1");

        internal static bool ModifyMask(ProfileSection[] profile, bool invert, int index, ref TerrainModify.Surface surface, ref TerrainModify.Heights heights, ref TerrainModify.Edges edges, ref float leftT, ref float rightT, ref float leftStartY, ref float rightStartY, ref float leftEndY, ref float rightEndY, ref bool __result) {
            if (index >= profile.Length) {
                __result = false;
                return false;
            }

            ProfileSection section = profile[index];
            if (invert) section = section.Inverse();


            heights = section.Heights;
            surface = section.Surface;
            edges = section.Edges;


            leftT = section.LeftX;
            rightT = section.RightX;
            leftStartY = section.LeftStartY;
            leftEndY = section.LeftEndY;
            rightStartY = section.RightStartY;
            rightEndY = section.RightEndY;

            __result = true;
            return false;
        }
    }
}