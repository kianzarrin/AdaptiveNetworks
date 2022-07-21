namespace AdaptiveRoads.Patches.Node.AntiFlickering {
    using AdaptiveRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;

    [HarmonyPatch()]
    [InGamePatch]
    public static class RenderInstance {
        public static MethodBase TargetMethod() =>
            typeof(NetNode)
            .GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance, throwOnError: true);

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = TranspilerUtils.ToCodeList(instructions);
                Patch(codes, occuranceMatrix0: 1, original); //DC
                Patch(codes, occuranceMatrix0: 2, original); //DC lod
                //Patch(codes, occuranceMatrix0: 3, original); // Junction
                //Patch(codes, occuranceMatrix0: 4, original); // Junction lod
                //Patch(codes, occuranceMatrix0: 5, original); // End 
                //Patch(codes, occuranceMatrix0: 6, original); // End lod
                //Patch(codes, occuranceMatrix0: 7, original); // Bend
                //Patch(codes, occuranceMatrix0: 8, original); // Bend lod
                Patch(codes, occuranceMatrix0: 9, original); // Bend DC
                Patch(codes, occuranceMatrix0: 10, original); // Bend DC lod

                Log.Info($"{ReflectionHelpers.ThisMethod} patched {original} successfully!");
                return codes;
            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }

        static void Patch(List<CodeInstruction> codes, int occuranceMatrix0, MethodBase method) {
            MethodInfo mModifyMatrix = typeof(RenderInstance).GetMethod(nameof(ModifyMatrix), throwOnError: true);
            FieldInfo f_dataMatrix0 = ReflectionHelpers.GetField<RenderManager.Instance>(nameof(RenderManager.Instance.m_dataMatrix0)); // data.m_dataMatrix0
            FieldInfo f_dataMatrix2 = ReflectionHelpers.GetField<RenderInstanceData>(nameof(RenderInstanceData.m_dataMatrix2)); // data.m_extraData.m_dataMatrix2

            int iLoadDataMatrix0 = codes.Search(c => c.LoadsField(f_dataMatrix0), count: occuranceMatrix0);
            int iLoadDataMatrix2 = codes.Search(c => c.LoadsField(f_dataMatrix2), startIndex: iLoadDataMatrix0);
            int iLoadNodeInfo = codes.Search(c => c.IsLdLoc(typeof(NetInfo.Node), method), startIndex: iLoadDataMatrix0, count: -1);
            CodeInstruction loadDataMatrix0 = codes[iLoadDataMatrix0];
            CodeInstruction loadDataMatrix2 = codes[iLoadDataMatrix2];
            CodeInstruction loadNodeInfo = codes[iLoadNodeInfo];
            CodeInstruction loadNodeID = GetLDArg(method, "nodeID");
            CodeInstruction loadRefData = new CodeInstruction(OpCodes.Ldarga, TranspilerUtils.GetArgLoc(method, "data"));

            codes.InsertInstructions(iLoadDataMatrix0 + 1, new CodeInstruction[]{
                new CodeInstruction(OpCodes.Ldc_I4_0),
                loadNodeInfo.Clone(),
                loadNodeID.Clone(),
                loadRefData.Clone(),
                new CodeInstruction(OpCodes.Call, mModifyMatrix),
            });

            iLoadDataMatrix2 = codes.Search(c => c.LoadsField(f_dataMatrix2), startIndex: iLoadDataMatrix0);
            codes.InsertInstructions(iLoadDataMatrix2 + 1, new CodeInstruction[]{
                new CodeInstruction(OpCodes.Ldc_I4_1),
                loadNodeInfo.Clone(),
                loadNodeID.Clone(),
                loadRefData.Clone(),
                new CodeInstruction(OpCodes.Call, mModifyMatrix),
            });

            //MethodInfo mApplyFlip = typeof(RenderInstance).GetMethod(nameof(ApplyFlip), throwOnError: true);
            //FieldInfo f_dataVector0 = ReflectionHelpers.GetField<RenderManager.Instance>(nameof(RenderManager.Instance.m_dataVector0)); // data.m_dataVector0
            //int iLoadDataVector0 = codes.Search(c => c.LoadsField(f_dataVector0), startIndex: iLoadDataMatrix0, count: -1);
            //codes.InsertInstructions(iLoadDataVector0 + 1, new CodeInstruction[]{
            //    loadNodeInfo.Clone(),
            //    new CodeInstruction(OpCodes.Call, mApplyFlip),
            //});
        }

        //static Vector4 ApplyFlip(Vector4 v, NetInfo.Node nodeInfo) {
        //    bool mirror = nodeInfo?.GetMetaData()?.Mirror ?? false;
        //    if (mirror) {
        //        v.x *= -1;
        //        v.y *= -1;
        //    }
        //    return v;
        //}

        static Matrix4x4 ModifyMatrix(Matrix4x4 mat, int index, NetInfo.Node nodeInfo, ushort nodeId, ref RenderManager.Instance data) {
            //bool mirror = nodeInfo?.GetMetaData()?.Mirror ?? false;
            //if (mirror) {
            //    if (index == 0)
            //        mat = data.m_extraData.m_dataMatrix2;
            //    else
            //        mat = data.m_dataMatrix0;
            //}

            bool antiFlickering = nodeInfo?.GetMetaData()?.AntiFlickering ?? false;
            if (antiFlickering) {
                int segmentIndex = data.m_dataInt0 & 7;
                int segmentIndex2 = data.m_dataInt0 >> 4;
                int n = nodeId.ToNode().CountSegments();
                int antiFlickeringIndex = Index(segmentIndex, nodeId) * n + Index(segmentIndex2, nodeId) + 2;
                mat = Lift(mat, antiFlickeringIndex * 0.0005f);
            }

            return mat;
        }

        static int Index(int segmentIndex, ushort nodeID) {
            ref NetNode node = ref nodeID.ToNode();
            int ret = 0;
            for (int i = 0; i < segmentIndex; ++i) {
                if (node.GetSegment(i) != 0)
                    ret++;
            }
            return ret;
        }

        static Matrix4x4 Lift(Matrix4x4 mat, float deltaY) {
            Matrix4x4 ret = mat;
            // row 1 is for y
            // cols: 0=a 1=b 2=c 3=d
            ret.m10 += deltaY; //a.y
            ret.m11 += deltaY; //b.y
            ret.m12 += deltaY; //c.y
            ret.m13 += deltaY; //d.y
            return ret;
        }
    }
}