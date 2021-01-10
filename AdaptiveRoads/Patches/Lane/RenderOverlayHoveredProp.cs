using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using static KianCommons.Patches.TranspilerUtils;
using KianCommons;
using ColossalFramework.Math;
using System;
using UnityEngine;
using AdaptiveRoads.UI.RoadEditor;

namespace AdaptiveRoads.Patches.Lane {
    public static class RenderOverlayHoveredProp {
        static MethodInfo mPosition =
            typeof(Bezier3).GetMethod(nameof(Bezier3.Position), new[] { typeof(float) })??
            throw new Exception("mPosition is null");

        static FieldInfo f_minScale =
            typeof(PropInfo).GetField(nameof(PropInfo.m_minScale)) ??
            throw new Exception("f_minScale is null");

        static FieldInfo f_angle =
            typeof(NetLaneProps.Prop).GetField(nameof(NetLaneProps.Prop.m_angle)) ??
            throw new Exception("f_angle is null");

        static FieldInfo f_requireWaterMap =
            typeof(PropInfo).GetField(nameof(PropInfo.m_requireWaterMap)) ??
            throw new Exception("f_requireWaterMap is null");

        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            int iPosition = codes.Search(_c => _c.Calls(mPosition));
            int iStLocPos = codes.Search(_c => _c.IsStLoc(typeof(Vector3)), startIndex:iPosition );
            CodeInstruction ldPos = codes[iStLocPos].BuildLdLocFromStLoc();

            int iMinScale = codes.Search(_c => _c.LoadsField(f_minScale));
            int iStScale = codes.Search(_c => _c.IsStloc(), startIndex: iMinScale);
            CodeInstruction ldScale = codes[iStScale].BuildLdLocFromStLoc();

            int iAngle = codes.Search(_c => _c.LoadsField(f_angle));
            int iStAngle = codes.Search(_c => _c.IsStloc(), startIndex: iAngle);
            int iLdProp = codes.Search(
                _c => _c.IsLdLoc(typeof(NetLaneProps.Prop)),
                startIndex: iAngle, count:-1);
            CodeInstruction ldAngle = codes[iStAngle].BuildLdLocFromStLoc();
            CodeInstruction ldProp = codes[iLdProp].Clone();

            int iRequireWaterMap = codes.Search(_c => _c.LoadsField(f_requireWaterMap));
            int iLdVariation = codes.Search(
                _c => _c.IsLdLoc(typeof(PropInfo)),
                startIndex: iRequireWaterMap, count: -1);
            CodeInstruction ldVariation = codes[iLdVariation].Clone();

            codes.InsertInstructions(iLdVariation, new[]{
                ldProp,
                ldVariation,
                ldPos,
                ldAngle,
                ldScale,
                new CodeInstruction(OpCodes.Call, mOnAfterRenderInstance),
            });
        }

        static MethodInfo mOnAfterRenderInstance = GetMethod(typeof(RenderOverlayHoveredProp), nameof(OnAfterRenderInstance));
        public static void OnAfterRenderInstance(
            NetLaneProps.Prop prop,
            PropInfo propInfo,
            Vector3 pos,
            float angle,
            float scale) {
            if (prop == Rendering.Prop) {
                Rendering.PropRenderQueue.Enqueue(new Rendering.PropData {
                    Angle = angle,
                    Scale = scale,
                    Pos = pos,
                    Prop = propInfo,
                });
            }
        }



    }
}
