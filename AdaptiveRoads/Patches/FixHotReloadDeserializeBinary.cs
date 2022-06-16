namespace AdaptiveRoads.Patches {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using System.Text.RegularExpressions;
    using System.Diagnostics;

    internal class HotReloadPatchAttribute : Attribute { }

    [HarmonyPatch]
    [PreloadPatch]
    ///<summary>makes sure only types from the latest assembly are used.</summary>
    public static class ReadTypeMetadataPatch {
        delegate Type GetType(string typeName, bool throwOnError);
        static string assemblyName = typeof(ReadTypeMetadataPatch).Assembly.GetName().Name;

        static bool Prepare() => LifeCycle.LifeCycle.bHotReload; // only apply when hot-reloading.

        static MethodBase TargetMethod() {
            var t = Type.GetType("System.Runtime.Serialization.Formatters.Binary.ObjectReader");
            return AccessTools.DeclaredMethod(t, "ReadTypeMetadata");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo m = TranspilerUtils.DeclaredMethod<GetType>(typeof(Type));
            MethodInfo mReplaceGeneric = AccessTools.DeclaredMethod(typeof(ReadTypeMetadataPatch), nameof(ReplaceAssemblyVersion));

            foreach (var code in instructions) {
                if (code.Calls(m)) {
                    yield return new CodeInstruction(OpCodes.Call, mReplaceGeneric);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1); // load true again
                }
                yield return code;
            }
        }

        static string ReplaceAssemblyVersion(string s, bool _) =>
            ReplaceAssemblyVersionImpl(s);

        static string ReplaceAssemblyVersionImpl(string s) {
            string nd = "\\d+\\."; // matches ###.
            string pattern = $"{assemblyName}, Version={nd}{nd}{nd}\\d*, Culture=neutral, PublicKeyToken=null";
            var s2 = Regex.Replace(s, pattern, assemblyName);
            if(Log.VERBOSE) s2.LogRet(ReflectionHelpers.CurrentMethod(1, s));
            return s2;
        }
    }
}
