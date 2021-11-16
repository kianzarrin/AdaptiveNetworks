namespace AdaptiveRoads.Util {
    using AdaptiveRoads.LifeCycle;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using KianCommons;

    public static class PackageManagerUtil {
        static class Delegates {
            public delegate Package GetPackage(string packageName);
            public static GetPackage GetPackageDelegate = DelegateUtil.CreateDelegate<GetPackage>(typeof(PackageManager));
        }

        public static Package GetPackage(string name) => Delegates.GetPackageDelegate(name);

        public static Package LoadingPackage => AssetDataExtension.AssetRef.package;

        public static Package SavingPackage {
            get {
                string name;
                if(SimulationManager.instance.m_metaData.m_WorkshopPublishedFileId != PublishedFileId.invalid) {
                    name = SimulationManager.instance.m_metaData.m_WorkshopPublishedFileId.ToString();
                } else {
                    name = AssetDataExtension.SaveName;
                }
                return GetPackage(name);
            }
        }
    }
}
