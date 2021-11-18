namespace AdaptiveRoads.Util {
    using AdaptiveRoads.LifeCycle;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using KianCommons;
    using System;
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
                try {
                    string name = AssetDataExtension.CurrentBasicNetInfo.name;
                    int dotIndex = name.LastIndexOf('.');
                    if(dotIndex > 0) {
                        Assertion.Assert(dotIndex > 0, $"dotIndex:{dotIndex} > 0");
                        string packageName = name.Substring(0, dotIndex);
                        return GetPackage(packageName)
                            ?? throw new Exception($"Package {packageName} not found");
                    } else {
                        return AssetDataExtension.ListingMetaData?.assetRef.package
                            ?? throw new Exception($"ListingMetaData?.assetRef.package is null");
                    }
                }catch(Exception ex) {
                    ex.Log($"failed to get package for CurrentBasicNetInfo={AssetDataExtension.CurrentBasicNetInfo} and ListingMetaData={AssetDataExtension.ListingMetaData}");
                    throw ex;
                }
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
