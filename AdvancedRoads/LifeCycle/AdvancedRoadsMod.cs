namespace AdvancedRoads.LifeCycle
{
    using System;
    using JetBrains.Annotations;
    using ICities;
    using CitiesHarmony.API;
    using KianCommons;

    public class AdvancedRoadsMod : IUserMod
    {
        public static Version ModVersion => typeof(AdvancedRoadsMod).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Advanced Roads" + VersionString;
        public string Description => "fundation for roads with extra flexibality and variablity.";

        [UsedImplicitly]
        public void OnEnabled()
        {
            HarmonyHelper.EnsureHarmonyInstalled();   
            if (HelpersExtensions.InGame)
                LifeCycle.Load();
        }

        [UsedImplicitly]
        public void OnDisabled()
        {
            LifeCycle.Release();
        }

        [UsedImplicitly]
        public void OnSettingsUI(UIHelperBase helper) {
            UI.ModSettings.OnSettingsUI(helper);
        }

    }
}
