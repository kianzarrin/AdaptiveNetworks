namespace AdaptiveRoads.LifeCycle {
    using ICities;
    using JetBrains.Annotations;
    using System;

    public class UserMod : IUserMod {
        public static Version ModVersion => typeof(UserMod).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Adaptive Networks " + VersionString;
        public string Description => "foundation for networks with extra flexibility and variability.";

        [UsedImplicitly]
        public void OnEnabled() => LifeCycle.Enable();

        [UsedImplicitly]
        public void OnDisabled() => LifeCycle.Disable();

        [UsedImplicitly]
        public void OnSettingsUI(UIHelperBase helper) {
            UI.ModSettings.OnSettingsUI(helper as UIHelper);
        }

    }
}
