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
    using AdaptiveRoads.LifeCycle;

    public static class LSMUtil {
        public const string LSM_REVISITED = "LoadingScreenModRevisited";
        public const string LSM_KLYTE = "LoadingScreenModKlyte";
        public const string LSM_TEST = "LoadingScreenModTest";
        public const string LSM = "LoadingScreenMod";

        internal static Assembly GetLSMAssembly() =>
            AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(_asm => _asm.GetName().Name == LSM || _asm.GetName().Name == LSM_TEST);

        internal static IEnumerable<Assembly> GetBothLSMAssembly() =>
            AppDomain.CurrentDomain.GetAssemblies()
            .Where(_asm => _asm.GetName().Name == LSM || _asm.GetName().Name == LSM_TEST);


        /// <param name="type">full type name minus assembly name and root name space</param>
        /// <returns>corresponding types from LSM or LSMTest or both</returns>
        public static IEnumerable<Type> GetTypeFromLSMs(string type) {
            var type1 = Type.GetType($"{LSM}.{type}, {LSM}", false);
            var type2 = Type.GetType($"{LSM_TEST}.{type}, {LSM_TEST}", false);
            var type3 = Type.GetType($"{LSM}.{type}, {LSM_KLYTE}", false);
            var type4 = Type.GetType($"{LSM}.{type}, {LSM_REVISITED}", false);
            if (type1 != null) yield return type1;
            if (type2 != null) yield return type2;
            if (type3 != null) yield return type3;
            if (type4 != null) yield return type4;
        }

        public static object GetSharing() {
            foreach(var type in GetTypeFromLSMs("Sharing")) {
                object sharing = AccessTools.Field(type, "inst").GetValue(null);
                if (sharing != null) {
                    Log.DebugOnce($"sharing found in '{type.Assembly.Name()}::{type}'");
                    return sharing;
                } else {
                    Log.DebugOnce($"sharing is empty in '{type.Assembly.Name()}::{type}'");
                }
            }
            Log.DebugOnce("LSM sharing NOT found!");
            return null;
        }

        public static Mesh GetMesh(object sharing, string checksum, IEnumerable<Package> packages, bool isLod) {
            if(checksum.IsNullorEmpty()) return null;
            foreach (var package in packages) {
                Assertion.NotNull(package, "package");
                Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
                Mesh ret;
                // search for mesh in the given packages
                if (sharing == null) {
                    ret = package.FindByChecksum(checksum)?.Instantiate<Mesh>();
                } else {
                    bool isMain = !isLod;
                    try {
                        ret = InvokeMethod(sharing, "GetMesh", checksum, package, isMain) as Mesh;
                    }catch(Exception ex) {
                        ex.Log();
                        ret = package.FindByChecksum(checksum)?.Instantiate<Mesh>();
                        if (ret != null) {
                            Log.Warning("Failed to use LSM to reduce MEMORY SIZE for mesh with checksum: " + checksum);
                        }
                    }
                }
                if (ret) {
                    Log.Debug($"loaded {ret} with checksum:({checksum}) from {package}");
                    return ret;
                }
            }
            Log.Error($"could not find mesh with checksum:({checksum}) from {packages.ToSTR()}");
            return null;

        }

        public static Material GetMaterial(object sharing, string checksum, IEnumerable<Package> packages, bool isLod) {
            if(checksum.IsNullorEmpty()) return null;
            foreach (var package in packages) {
                if(package == null) { }
                Assertion.NotNull(package, "package");
                Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
                Material ret;
                if (sharing == null) {
                    ret = package.FindByChecksum(checksum)?.Instantiate<Material>();
                } else {
                    bool isMain = !isLod;
                    try {
                        ret = InvokeMethod(sharing, "GetMaterial", checksum, package, isMain) as Material;
                    } catch(Exception ex) {
                        ex.Log(false);
                        ret = package.FindByChecksum(checksum)?.Instantiate<Material>();
                        if (ret != null) {
                            Log.Warning("Failed to use LSM to reduce MEMORY SIZE for material with checksum: " + checksum);
                        }
                    }
                }
                if (ret) {
                    Log.Debug($"loaded {ret} with checksum:({checksum}) from {package}");
                    return ret;
                }
            }
            Log.Error($"could not find material with checksum:({checksum}) from {packages.ToSTR()}");
            return null;
        }

#if OPTIMISATION
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
                CacheInstance = new Cache(sharing, PackageManagerUtil.PersistencyPackage);
            } else {
                CacheInstance = null;
            }
        }

        public static Mesh GetMesh(string checksum, bool isLod) {
            if(checksum.IsNullorEmpty())
                return null;
            if(CacheInstance == null) {
                return CacheInstance.LoadingPackage.FindByChecksum(checksum)?.Instantiate<Mesh>();
            } else {
                return CacheInstance.GetMesh(checksum, isLod);
            }
        }

        public static Material GetMaterial(string checksum, bool isLod) {
            if(checksum.IsNullorEmpty())
                return null;
            if(CacheInstance == null) {
                return CacheInstance.LoadingPackage.FindByChecksum(checksum)?.Instantiate<Material>();
            } else {
                return CacheInstance.GetMaterial(checksum, isLod);
            }
        }
#endregion
#endif
    }
}
