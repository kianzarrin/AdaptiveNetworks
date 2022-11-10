namespace AdaptiveRoads.Patches.Lane;

using AdaptiveRoads.Manager;
using ColossalFramework.Math;
using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static DistrictPark;

[HarmonyPatch]
[InGamePatch]
[HarmonyBefore("com.klyte.redirectors.PS")]
public static class RenderInstance {
    // public void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, uint laneID, NetInfo.Lane laneInfo, NetNode.Flags startFlags, NetNode.Flags endFlags, Color startColor, Color endColor, float startAngle, float endAngle, bool invert, int layerMask, Vector4 objectIndex1, Vector4 objectIndex2, ref RenderManager.Instance data, ref int propIndex)
    static MethodBase TargetMethod() => ReflectionHelpers.GetMethod(
        typeof(NetLane), nameof(NetLane.RenderInstance));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original, ILGenerator il) {
        try {
            var codes = TranspilerUtils.ToCodeList(instructions);
            CheckPropFlagsCommons.PatchCheckFlags(codes, original, il);
            Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
            return codes;
        } catch (Exception e) {
            Log.Error(e.ToString());
            throw e;
        }
    }
} // end class

[HarmonyPatch]
[HarmonyBefore("com.klyte.redirectors.PS")]
public static class RenderInstanceOverlayPatch {
    static MethodBase TargetMethod() => ReflectionHelpers.GetMethod(
        typeof(NetLane), nameof(NetLane.RenderInstance));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
        try {
            var codes = TranspilerUtils.ToCodeList(instructions);
            SeedIndexCommons.Patch(codes, original);
            PropOverlay.Patch(codes, original);
            TreeOverlay.Patch(codes, original);
            Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
            return codes;
        } catch (Exception e) {
            Log.Error(e.ToString());
            throw e;
        }
    }
}

[HarmonyPatch]
[InGamePatch]
[HarmonyBefore("com.klyte.redirectors.PS")]
public static class RenderInstance_JunctionDistancePatch {
    static MethodBase TargetMethod() => typeof(NetLane).GetMethod(nameof(NetLane.RenderInstance), throwOnError: true);
    delegate int Int32(uint range); // Randomizer.Int32(uint range);

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase origin) {
        var fRepeateDistance = typeof(NetLaneProps.Prop).GetField(nameof(NetLaneProps.Prop.m_repeatDistance));
        var fLength = typeof(NetLane).GetField(nameof(NetLane.m_length));
        var mModifyLenght = typeof(RenderInstance_JunctionDistancePatch).GetMethod(nameof(ModifyLenght), throwOnError: true);
        var mModifyT = typeof(RenderInstance_JunctionDistancePatch).GetMethod(nameof(ModifyT), throwOnError: true);
        var mRandomizer_Int32 = DelegateUtil.GetMethod<Int32>(typeof(Randomizer), throwOnError: true);
        var codes = TranspilerUtils.ToCodeList(instructions);

        int iLoadRepeatDistance = codes.Search(c => c.LoadsField(fRepeateDistance), count:2);
        int iLength = codes.Search(c => c.LoadsField(fLength), startIndex: iLoadRepeatDistance, count: -1);
        int iLdProp = codes.Search(
            _c => _c.IsLdLoc(typeof(NetLaneProps.Prop), origin),
            startIndex: iLength, count: -1);

        codes.InsertInstructions(iLength + 1, new[] {
            // length is already on the stack
            codes[iLdProp].Clone(),
            TranspilerUtils.GetLDArg(origin, "laneID"),
            new CodeInstruction(OpCodes.Call, mModifyLenght),
        });

        int iRandomizer_Int32 = codes.Search(c => c.Calls(mRandomizer_Int32), startIndex: iLength);
        int iDiv = codes.Search(c => c.opcode == OpCodes.Div, startIndex: iRandomizer_Int32); // float t = halfSegmentOffset + (float)j / (float)repeatCountTimes2;
        codes.InsertInstructions(iDiv + 1, new[] {
            // t is already on the stack
            codes[iLdProp].Clone(),
            TranspilerUtils.GetLDArg(origin, "laneID"),
            new CodeInstruction(OpCodes.Call, mModifyT),
        });

        return codes;
    }

    public static float ModifyLenght(float lenght0, NetLaneProps.Prop prop, uint laneId) {
        if (prop?.GetMetaData() is NetInfoExtionsion.LaneProp propExt) {
            // each end of the junction already has distance of repeatDistance/2
            float jd = propExt.JunctionDistance - prop.m_repeatDistance * 0.5f;
            if (jd > 0) {
                ref NetLane lane = ref laneId.ToLane();
                ushort segmentId = lane.m_segment;
                ref NetSegment segment = ref segmentId.ToSegment();
                bool junction1 = segment.m_startNode.ToNode().IsJunction();
                bool junction2 = segment.m_endNode.ToNode().IsJunction();
                float jd1 = junction1 ? jd : 0;
                float jd2 = junction2 ? jd : 0;

                return Math.Max(1, lenght0 - jd1 - jd2);
            }
        }
        return lenght0;
    }

    public static float ModifyT(float t0, NetLaneProps.Prop prop, uint laneId) {
        if (prop?.GetMetaData() is NetInfoExtionsion.LaneProp propExt) {
            // each end of the junction already has distance of repeatDistance/2
            float jd = propExt.JunctionDistance - prop.m_repeatDistance * 0.5f;
            if (jd > 0) {
                ref NetLane lane = ref laneId.ToLane();
                ushort segmentId = lane.m_segment;
                ref NetSegment segment = ref segmentId.ToSegment();
                bool junction1 = segment.m_startNode.ToNode().IsJunction();
                bool junction2 = segment.m_endNode.ToNode().IsJunction();
                float jd1 = junction1 ? jd : 0;
                float jd2 = junction2 ? jd : 0;

                float len = lane.m_length;
                float r = 1 - (jd1 + jd2) / len;
                return jd1 / len + t0 * r;
            }
        }
        return t0;
    }
}
