namespace AdvancedRoads.LifeCycle
{
    using System;
    using JetBrains.Annotations;
    using ICities;
    using CitiesHarmony.API;
    using AdvancedRoads.Util;
    public class AdvancedRoadsMod : IUserMod
    {
        public static Version ModVersion => typeof(AdvancedRoadsMod).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Node controller " + VersionString;
        public string Description => "control Road/junction transitions";

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
            GUI.Settings.OnSettingsUI(helper);
        }

    }
}
