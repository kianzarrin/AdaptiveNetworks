namespace AdaptiveRoads.Util {
    using AdaptiveRoads.LifeCycle;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public static class PackageManagerUtil {
        private  static Package[] GetPackagesOf(NetInfo info) {
            string fullName = info.name;
            int dotIndex = fullName.LastIndexOf('.');
            if (dotIndex > 0) {
                string packageName = fullName.Substring(0, dotIndex);
                var ret = PackageManager.allPackages.Where(p => p.packageName == packageName);
                if (!ret.IsNullorEmpty()) {
                    return ret.ToArray();
                }
            } else {
                throw new Exception("could not analyze package name for " + info);
            }

            // work around for clus road with space after dot in the road name.
            dotIndex = -1;
            for (int i = 0; i < fullName.Length - 1; i++) {
                if (fullName[i] == '.' && fullName[i + 1] != ' ') {
                    dotIndex = i;
                }
            }
            if (dotIndex > 0) {
                string packageName = fullName.Substring(0, dotIndex);
                var ret = PackageManager.allPackages.Where(p => p.packageName == packageName);
                if (!ret.IsNullorEmpty()) {
                    return ret.ToArray();
                }
            }

            throw new Exception(message: "did not find package for " + info);
        }

        /// <summary>
        /// searches the packages that matches the name of NetInfo being loaded.
        /// returns a list of packages that match. one of these packages should contain the mesh/material we need.
        /// throws exception if no matching package was found.
        /// </summary>
        public static Package[] GetLoadingPackages() {
            try {
                if (AssetDataExtension.ListingMetaData != null) {
                    Log.Debug($"package from ListingMetaData({AssetDataExtension.ListingMetaData.name}) is {AssetDataExtension.ListingMetaData?.assetRef.package}");
                    var ret = AssetDataExtension.ListingMetaData?.assetRef.package
                        ?? throw new Exception($"ListingMetaData?.assetRef.package is null");
                    return new[] { ret };
                } else {
                    return GetPackagesOf(AssetDataExtension.CurrentBasicNetInfo);
                }
            } catch (Exception ex) {
                ex.Log($"failed to get package for CurrentBasicNetInfo='{AssetDataExtension.CurrentBasicNetInfo}' and ListingMetaData='{AssetDataExtension.ListingMetaData}'");
                throw ex;
            }
        }

        public static Package SavingPackage {
            get {
                string packageName;
                if(SimulationManager.instance.m_metaData.m_WorkshopPublishedFileId != PublishedFileId.invalid) {
                    packageName = SimulationManager.instance.m_metaData.m_WorkshopPublishedFileId.ToString();
                } else {
                    packageName = AssetDataExtension.SaveName;
                }
                Log.Debug("getting package with name = " + packageName );
                foreach(Package package in PackageManager.allPackages) {
                    if( package.packageName == packageName &&
                        package.packageMainAsset == AssetDataExtension.SaveName && // distinguish between WS package and the package being saved.
                        package.packagePath == null // when overwriting, there would be another package with the same name. The package being saved is the one that has no path.
                        ) {
                        if(Log.VERBOSE)
                            Log.Debug("SavingPackage -> " + package);
                        return package;
                    }
                }
                Log.Error($"Failed to find saving package! packageName={packageName} SaveName={AssetDataExtension.SaveName}");
                return null;
            }
        }
    }
}
