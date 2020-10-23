namespace KianCommons {
    using CitiesHarmony.API;
    using HarmonyLib;
    using System.Reflection;
    using System;
    using System.Runtime.CompilerServices;

    public static class HarmonyUtil {
        static bool harmonyInstalled_ = false;
        internal static void AssertCitiesHarmonyInstalled() {
            if (!HarmonyHelper.IsHarmonyInstalled) {
                string m =
                    "****** ERRRROOORRRRRR!!!!!!!!!! **************\n" +
                    "**********************************************\n" +
                    "    HARMONY MOD DEPENDANCY IS NOT INSTALLED!\n\n" +
                    "solution:\n" +
                    " - exit to desktop.\n" +
                    " - unsub harmony mod.\n" +
                    " - make sure harmony mod is deleted from the content folder\n" +
                    " - resub to harmony mod.\n" +
                    " - run the game again.\n" +
                    "**********************************************\n" +
                    "**********************************************\n";
                Log.Error(m);
                throw new Exception(m);
            }
        }

        internal static void InstallHarmony(string harmonyID) {
            if (harmonyInstalled_) {
                Log.Info("skipping harmony installation because its already installed");
                return;
            }
            AssertCitiesHarmonyInstalled();
            Log.Info("Patching...");
            PatchAll(harmonyID);
            harmonyInstalled_ = true;
            Log.Info("Patched.");
        }

        /// <summary>
        /// assertion shall take place in a function that does not refrence Harmony.
        /// </summary>
        /// <param name="harmonyID"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PatchAll(string harmonyID) {
            var harmony = new Harmony(harmonyID);
            harmony.PatchAll();
        }

        internal static void UninstallHarmony(string harmonyID) {
            AssertCitiesHarmonyInstalled();
            Log.Info("UnPatching...");
            UnpatchAll(harmonyID);
            harmonyInstalled_ = false;
            Log.Info("UnPatched.");
        }

        /// <summary>
        /// assertion shall take place in a function that does not refrence Harmony.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void UnpatchAll(string harmonyID) {
            var harmony = new Harmony(harmonyID);
            harmony.UnpatchAll(harmonyID);
        }

        internal static void ManualPatch<T>(string harmonyID) {
            AssertCitiesHarmonyInstalled();
            ManualPatchUnSafe(typeof(T), harmonyID);
        }
        internal static void ManualPatch(Type t, string harmonyID) {
            AssertCitiesHarmonyInstalled();
            ManualPatchUnSafe(t, harmonyID);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ManualPatchUnSafe(Type t, string harmonyID) {
            try {
                MethodBase targetMethod =
                    AccessTools.DeclaredMethod(t, "TargetMethod")
                    .Invoke(null, null) as MethodBase ;
                Log.Info($"{t.FullName}.TorgetMethod()->{targetMethod}", true);
                Assertion.AssertNotNull(targetMethod, $"{t.FullName}.TargetMethod() returned null");
                var prefix = GetHarmonyMethod(t, "Prefix");
                var postfix = GetHarmonyMethod(t, "Postfix");
                var transpiler = GetHarmonyMethod(t, "Transpiler");
                var finalizer = GetHarmonyMethod(t, "Finalizer");
                var harmony = new Harmony(harmonyID);
                harmony.Patch(original: targetMethod, prefix: prefix, postfix: postfix, transpiler: transpiler, finalizer: finalizer);
            }
            catch(Exception e) {
                Log.Exception(e);
            }
        }

        public static HarmonyMethod GetHarmonyMethod(Type t, string name) {
            var m = AccessTools.DeclaredMethod(t, name);
            if (m == null) return null;
            Assertion.Assert(m.IsStatic, $"{m}.IsStatic");
            return new HarmonyMethod(m);
        }


    }
}