namespace AdaptiveRoads.Util {
    using AdaptiveRoads.LifeCycle;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using KianCommons;
    using System;
    using System.Linq;

    public static class PackageManagerUtil {
        public static Package GetPackage(string name, string mainAsset) {
            // when updating road assets we will end up wit 2 packages with same name/mainAsset.
            // only the local one is loaded in game. It also happens to be the latter which is why I use LastOrDefault().
            var ret = PackageManager.allPackages.LastOrDefault(p => p.packageName == name && p.packageMainAsset == mainAsset);
            if (Log.VERBOSE) {
                Log.Debug(ReflectionHelpers.CurrentMethod(1, name, mainAsset) + " -> " + ret);
                var packages = PackageManager.allPackages.Where(p => p.packageName == name);
                Log.Debug("similar packages are: " + packages.Select(p => $"{p}, mainAsset={p.packageMainAsset}").ToSTR());
            }
            return ret;
        }

        /// <summary>
        /// package of the asset that is being serialized/deserialized
        /// </summary>
        public static Package PersistencyPackage {
            get {
                try {
                    if(AssetDataExtension.ListingMetaData != null) {
                        Log.Debug($"package from ListingMetaData({AssetDataExtension.ListingMetaData.name}) is {AssetDataExtension.ListingMetaData?.assetRef.package}");
                        return AssetDataExtension.ListingMetaData?.assetRef.package
                            ?? throw new Exception($"ListingMetaData?.assetRef.package is null");
                    }
                    string name = AssetDataExtension.CurrentBasicNetInfo.name;
                    int dotIndex = name.LastIndexOf('.');
                    if(dotIndex > 0) {
                        Assertion.Assert(dotIndex > 0, $"dotIndex:{dotIndex} > 0");
                        string packageName = name.Substring(0, dotIndex);
                        string mainAseet = name.Substring(dotIndex + 1).Remove("_Data");
                        return GetPackage(packageName, mainAseet)
                            ?? throw new Exception($"Package '{packageName}' with main asset:'{mainAseet}' not found!");
                    } else {
                        throw new Exception("could not analyze package name: " + name);
                    }
                }catch(Exception ex) {
                    ex.Log($"failed to get package for CurrentBasicNetInfo='{AssetDataExtension.CurrentBasicNetInfo}' and ListingMetaData='{AssetDataExtension.ListingMetaData}'");
                    throw ex;
                }
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
