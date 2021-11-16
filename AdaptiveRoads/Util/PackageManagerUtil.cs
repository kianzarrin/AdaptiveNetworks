namespace AdaptiveRoads.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ColossalFramework.Packaging;
    using ColossalFramework;
    using UnityEngine;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using AdaptiveRoads.LifeCycle;
    using ColossalFramework.PlatformServices;

    public static class PackageManagerUtil {
        public static Package GetPackage(string name) {
            return InvokeMethod<PackageManager>("GetPackage", name) as Package;
        }

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
