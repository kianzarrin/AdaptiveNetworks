namespace AdaptiveRoads.Util {
    using AdaptiveRoads.LifeCycle;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using KianCommons;
    using System.Linq;

    public static class PackageManagerUtil {
        static class Delegates {
            public delegate Package GetPackage(string packageName);
            public static GetPackage GetPackageDelegate = DelegateUtil.CreateDelegate<GetPackage>(typeof(PackageManager));
        }

        public static Package GetPackage(string name) => Delegates.GetPackageDelegate(name);

        /// <summary>
        /// package of the asset that is being serialized/deserialized
        /// </summary>
        public static Package PersistencyPackage {
            get {
                int dotIndex = AssetDataExtension.MetaDataName.LastIndexOf('.');
                string packageName = AssetDataExtension.MetaDataName.Substring(0, dotIndex);
                var ret = GetPackage(packageName);
                if(ret == null) Log.Error($"failed to find package for asset metadata {AssetDataExtension.MetaDataName}");
                return ret;
            }
        }


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
