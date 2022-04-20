namespace AdaptiveRoads.Util {
    using AdaptiveRoads.LifeCycle;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using KianCommons;
    using System;
    using System.Linq;

    public static class PackageManagerUtil {
        public static Package[] GetPackages(string name) {
            var ret = PackageManager.allPackages.Where(p => p.packageName == name);
            if (ret.IsNullorEmpty()) {
                throw new Exception("did not find package:" +name);
            }
            if (Log.VERBOSE) ret.LogRet();
            return ret.ToArray();
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
                    string name = AssetDataExtension.CurrentBasicNetInfo.name;
                    int dotIndex = name.LastIndexOf('.');
                    if (dotIndex > 0) {
                        Assertion.Assert(dotIndex > 0, $"dotIndex:{dotIndex} > 0");
                        string packageName = name.Substring(0, dotIndex);
                        return GetPackages(packageName);
                    } else {
                        throw new Exception("could not analyze package name for NetInfo " + AssetDataExtension.CurrentBasicNetInfo);
                    }
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
