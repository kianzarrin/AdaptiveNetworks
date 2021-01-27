namespace AdaptiveRoads.Patches.RoadEditor.AssetImporterAssetTemplatePatches {
    using HarmonyLib;
    using ColossalFramework.UI;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using static KianCommons.ReflectionHelpers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    // speed up prop loading times by increasing step size.
    // and removing sprite pictures
    [HarmonyPatch]
    public static class RefreshCoroutinePatch {
        [UsedImplicitly]
        static MethodBase TargetMethod() {
            return TranspilerUtils.GetCoroutineMoveNext(
                typeof(AssetImporterAssetTemplate),
                nameof(AssetImporterAssetTemplate.RefreshCoroutine));
        }

        //static Stopwatch sw = new Stopwatch();
        //[UsedImplicitly]
        //static void Prefix() {
        //    float ms = sw.ElapsedMilliseconds;
        //    Log.Debug("AssetImporterAssetTemplate.RefreshCoroutine() started. " +
        //        $"gap = {ms:#,0}ms ");
        //    Log.Flush();
        //    sw.Reset();
        //    sw.Start();
        //}
        //[UsedImplicitly]
        //static void Postfix() {
        //    sw.Stop();
        //    float ms = sw.ElapsedMilliseconds;
        //    Log.Debug("AssetImporterAssetTemplate.RefreshCoroutine()finished. " +
        //        $"duration = {ms:#,0}ms ");
        //    Log.Flush();
        //    sw.Reset();
        //    sw.Start();
        //}

        static MethodInfo mset_atlas = GetMethod(typeof(UISprite), "set_atlas");
        static MethodInfo mset_spriteName = GetMethod(typeof(UISprite), "set_spriteName");

        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions) {
            int n = 0;
            foreach (var code in instructions) {
                //if(code.Calls(mset_atlas) || code.Calls(mset_spriteName)) {
                //    yield return new CodeInstruction(OpCodes.Pop);// pop value
                //    yield return new CodeInstruction(OpCodes.Pop);// pop instance
                //} else
                if (code.opcode == OpCodes.Rem) {
                    n++;
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 100);
                    yield return code;
                } else {
                    yield return code;
                }

            }
            Assertion.AssertEqual(n, 2);
        }
    }
}