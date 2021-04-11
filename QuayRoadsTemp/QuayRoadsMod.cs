using CitiesHarmony.API;
using ICities;
// Make sure that "using HarmonyLib;" does not appear here!
// Only reference HarmonyLib in code that runs when Harmony is ready (DoOnHarmonyReady, IsHarmonyInstalled)

namespace QuayRoadsMod {
    public class Mod : IUserMod {
        // Make sure that HarmonyLib is not referenced in any way in your IUserMod implementation!
        // Instead, apply your patches from a separate static patcher class!
        // (otherwise it will fail to instantiate the type when CitiesHarmony is not installed)

        public string Name => "Quay Roads Mod";
        public string Description => "Trying to port the terrain deforming behaviour of quays to roads";

        public void OnEnabled() {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }
    }
}

