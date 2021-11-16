namespace AdaptiveRoads.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using static ColossalFramework.Plugins.PluginManager;
    using HarmonyLib;
    using UnityEngine;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using ColossalFramework.Packaging;

    public static class LSMUtil {
        public const string LSM_TEST = "LoadingScreenModTest";
        public const string LSM = "LoadingScreenMod";
        internal static bool IsLSM(this PluginInfo p) =>
            p != null && p.name == "667342976" || p.name == "833779378" || p.name == LSM || p.name == LSM_TEST;

        internal static Assembly GetLSMAssembly() =>
            AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(_asm => _asm.GetName().Name == LSM || _asm.GetName().Name == LSM_TEST);

        internal static IEnumerable<Assembly> GetBothLSMAssembly() =>
            AppDomain.CurrentDomain.GetAssemblies()
            .Where(_asm => _asm.GetName().Name == LSM || _asm.GetName().Name == LSM_TEST);


        /// <param name="type">full type name minus assembly name and root name space</param>
        /// <returns>corresponding types from LSM or LSMTest or both</returns>
        public static IEnumerable<Type> GetTypeFromBothLSMs(string type) {
            var type1 = Type.GetType($"{LSM}.{type}, {LSM}", false);
            var type2 = Type.GetType($"{LSM_TEST}.{type}, {LSM_TEST}", false);
            if(type1 != null) yield return type1;
            if(type2 != null) yield return type2;
        }

        public static object GetSharing() {
            foreach(var type in GetTypeFromBothLSMs("Sharing")) {
                object sharing = AccessTools.Field(type, "inst").GetValue(null);
                if(sharing != null)
                    return sharing;
            }
            return null;
        }

        public static Mesh GetMesh(object sharing, string checksum, Package package, bool isLod) {
            if(checksum.IsNullorEmpty())
                return null;
            if(sharing == null) {
                return package.FindByChecksum(checksum)?.Instantiate<Mesh>();
            } else {
                bool isMain = !isLod;
                return InvokeMethod(sharing, "GetMesh", checksum, package, isMain) as Mesh;
            }
        }

        public static Material GetMaterial(object sharing, string checksum, Package package, bool isLod) {
            if(checksum.IsNullorEmpty())
                return null;
            if(sharing == null) {
                return package.FindByChecksum(checksum)?.Instantiate<Material>();
            } else {
                bool isMain = !isLod;
                return InvokeMethod(sharing, "GetMaterial", checksum, package, isMain) as Material;
            }
        }

        #region optimization
        public class Cache {
            public static class Delegates {
                public delegate Mesh GetMesh(string checksum, Package package, bool isMain);
                public delegate Material GetMaterial(string checksum, Package package, bool isMain);
            }
            public Package LoadingPackage;
            public static Delegates.GetMesh GetMeshDelagate;
            public static Delegates.GetMaterial GetMaterialDelagate;

            public Cache(object sharing, Package package) {
                LoadingPackage = package;
                GetMeshDelagate = DelegateUtil.CreateClosedDelegate<Delegates.GetMesh>(sharing);
                GetMaterialDelagate = DelegateUtil.CreateClosedDelegate<Delegates.GetMaterial>(sharing);
            }

            public Mesh GetMesh(string checksum, bool isMain) => GetMeshDelagate(checksum, LoadingPackage, isMain);
            public Material GetMaterial(string checksum, bool isMain) => GetMaterialDelagate(checksum, LoadingPackage, isMain);
        }

        public static Cache CacheInstance;
        public static void Init() {
            object sharing = GetSharing();
            if(sharing != null) {
                CacheInstance = new Cache(sharing, PackageManagerUtil.LoadingPackage);
            } else {
                CacheInstance = null;
            }
        }

        public static Mesh GetMesh(string checksum, bool isLod) {
            if(checksum.IsNullorEmpty())
                return null;
            if(CacheInstance == null) {
                return PackageManagerUtil.LoadingPackage.FindByChecksum(checksum)?.Instantiate<Mesh>();
            } else {
                return CacheInstance.GetMesh(checksum, isLod);
            }
        }

        public static Material GetMaterial(string checksum, bool isLod) {
            if(checksum.IsNullorEmpty())
                return null;
            if(CacheInstance == null) {
                return PackageManagerUtil.LoadingPackage.FindByChecksum(checksum)?.Instantiate<Material>();
            } else {
                return CacheInstance.GetMaterial(checksum, isLod);
            }
        }
        #endregion
    }
}
