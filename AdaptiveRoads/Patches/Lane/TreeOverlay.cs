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
using System.Linq;

namespace AdaptiveRoads.Patches.Lane {
    public static class TreeOverlay {
        static MethodInfo mPosition =
            typeof(Bezier3).GetMethod(nameof(Bezier3.Position), new[] { typeof(float) })??
            throw new Exception("mPosition is null");

        static MethodInfo mGetTreeVariation = GetMethod(typeof(TreeInfo), nameof(TreeInfo.GetVariation));

        static FieldInfo f_minScale =
            typeof(TreeInfo).GetField(nameof(TreeInfo.m_minScale)) ??
            throw new Exception("f_minScale is null");

        static FieldInfo f_angle =
            typeof(NetLaneProps.Prop).GetField(nameof(NetLaneProps.Prop.m_angle)) ??
            throw new Exception("f_angle is null");

        static MethodInfo mRenerTreeInstance =
            typeof(TreeInstance).GetMethod("RenderInstance", BindingFlags.Public | BindingFlags.Static);


        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            int iStFinalTree = codes.Search(_c => _c.IsStLoc(typeof(TreeInfo)));
            int iLdProp = codes.Search(
                _c => _c.IsLdLoc(typeof(NetLaneProps.Prop)),
                startIndex: iStFinalTree, count: -1);
            CodeInstruction ldProp = codes[iLdProp].Clone();

            Log.Debug("Tree Overlay Patch ALL:\n" + codes.IL2STR(), false);
            Log.Debug($"Tree Overlay Patch After {iStFinalTree}:\n" + codes.IL2STR(), false);
            int iPosition = codes.Search(_c => _c.Calls(mPosition), startIndex: iStFinalTree);
            int iStLocPos = codes.Search(_c => _c.IsStLoc(typeof(Vector3)), startIndex: iPosition);
            CodeInstruction ldPos = codes[iStLocPos].BuildLdLocFromStLoc();

            int iMinScale = codes.Search(_c => _c.LoadsField(f_minScale), startIndex: iStFinalTree);
            int iStScale = codes.Search(_c => _c.IsStLoc(typeof(float)), startIndex: iMinScale);
            CodeInstruction ldScale = codes[iStScale].BuildLdLocFromStLoc();

            int iGetVariation = codes.Search(_c => _c.Calls(mGetTreeVariation));
            int iStVariation = codes.Search(_c => _c.IsStLoc(typeof(TreeInfo)), startIndex: iGetVariation);
            CodeInstruction loadVariation = codes[iStVariation].BuildLdLocFromStLoc();

            int iRenderTreeInstance = codes.Search(_c => _c.Calls(mRenerTreeInstance));

            var insertions = new[]{
                ldProp,
                loadVariation,
                ldPos,
                ldScale,
                new CodeInstruction(OpCodes.Call, mOnAfterRenderInstance),
            };
            codes.InsertInstructions(iRenderTreeInstance + 1, insertions, moveLabels:false);
        }

        static MethodInfo mOnAfterRenderInstance = GetMethod(typeof(TreeOverlay), nameof(OnAfterRenderInstance));
        public static void OnAfterRenderInstance(
            NetLaneProps.Prop prop,
            TreeInfo varitation,
            Vector3 pos,
            float scale) {
            if (prop == Overlay.HoveredInfo) {
                Overlay.TreeQueue.Enqueue(new Overlay.TreeData {
                    Scale = scale,
                    Pos = pos,
                    Tree = varitation,
                });
            }
        }



    }
}
